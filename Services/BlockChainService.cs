﻿using System.Security.Cryptography;
using BlockChain.Data;
using BlockChain.Models;
using Microsoft.EntityFrameworkCore;

namespace BlockChain.Services;

public class BlockChainService
{
    private readonly ApplicationDbContext _context;

    public BlockChainService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Block>> GetAllBlocksAsync()
    {
        return await _context.Blocks.OrderBy(b => b.Index).ToListAsync();
    }

    public async Task AddBlockAsync(string data, string privateKeyXml)
    {
        try
        {
            var blocks = await GetAllBlocksAsync();
            var prevBlock = blocks.LastOrDefault();

            if (prevBlock == null) return;

            var newBlock = new Block(blocks.Count, data, prevBlock.Hash);

            var publicKeyXml = GetPublicKeyFromPrivate(privateKeyXml);
            if (string.IsNullOrEmpty(publicKeyXml))
                throw new Exception("Public key could not be found.");

            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKeyXml);
            var privateParams = rsa.ExportParameters(true);

            newBlock.Sign(privateParams, publicKeyXml);

            _context.Blocks.Add(newBlock);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[AddBlockAsync] {e.GetType().Name}: {e.Message}");
            throw new ApplicationException("Invalid private key. Please try again with a valid key.");
        }
    }

    public async Task<Block?> GetBlockByIndexAsync(int index)
    {
        return await _context.Blocks.FirstOrDefaultAsync(b => b.Index == index);
    }

    public async Task<bool> EditBlockAsync(int index, string data)
    {
        var block = await GetBlockByIndexAsync(index);
        if (block == null) return false;

        block.Data = data;
        block.Hash = block.ComputeHash();

        _context.Blocks.Update(block);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsValidAsync()
    {
        var blocks = await GetAllBlocksAsync();

        for (int i = 1; i < blocks.Count; i++)
        {
            var current = blocks[i];
            var prevBlock = blocks[i - 1];

            if (current.PrevHash != prevBlock.Hash) return false;
            if (current.Hash != current.ComputeHash()) return false;
        }

        return true;
    }

    public async Task<List<BlockValidationViewModel>> GetValidatedBlocksAsync()
    {
        var blocks = await GetAllBlocksAsync();
        var result = new List<BlockValidationViewModel>();
        bool stillValid = true;

        for (int i = 0; i < blocks.Count; i++)
        {
            bool isValid = true;

            if (i > 0)
            {
                var prev = blocks[i - 1];
                if (stillValid)
                {
                    if (blocks[i].PrevHash != prev.Hash)
                    {
                        stillValid = false;
                        isValid = false;
                    }
                }
                else
                {
                    isValid = false;
                }
            }

            result.Add(new BlockValidationViewModel
            {
                Block = blocks[i],
                IsValid = isValid
            });
        }

        return result;
    }
    
    public string GeneratePrivateKeyXml()
    {
        using var rsa = RSA.Create();
        return rsa.ToXmlString(true);
    }
    
    public string? GetPublicKeyFromPrivate(string privateKeyXml)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKeyXml);
            return rsa.ToXmlString(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetPublicKeyFromPrivate] Invalid key: {ex.Message}");
            return null;
        }
    }

    public async Task<List<BlockValidationViewModel>> GetSignatureValidationAsync()
    {
        var blocks = await GetAllBlocksAsync();
        return blocks.Select(b => new BlockValidationViewModel
        {
            Block = b,
            IsValid = b.Verify()
        }).ToList();
    }
}