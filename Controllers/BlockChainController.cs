using BlockChain.Models;
using BlockChain.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlockChain.Controllers;

public class BlockChainController : Controller
{
    private readonly BlockChainService _blockChainService;

    public BlockChainController(BlockChainService blockChainService)
    {
        _blockChainService = blockChainService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.AlertMessage = TempData["Message"];
        ViewBag.AlertType = TempData["Type"];
        
        var validatedBlocks = await _blockChainService.GetValidatedBlocksAsync();
        var isSignatureValid = await _blockChainService.GetSignatureValidationAsync();
        
        var model = validatedBlocks.Select((block, i) => new BlockValidationViewModel
        {
            Block = block.Block,
            IsValid = block.IsValid,
            IsSignatureValid = isSignatureValid[i].IsValid
        }).ToList();

        ViewBag.IsChainValid = model.All(b => b.IsValid);
        
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(string data, string privateKey)
    {
        if (string.IsNullOrWhiteSpace(data) || string.IsNullOrWhiteSpace(privateKey))
        {
            TempData["Message"] = "Please enter both data and private key.";
            TempData["Type"] = "Error";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _blockChainService.AddBlockAsync(data, privateKey);
            TempData["Message"] = "Block successfully added.";
            TempData["Type"] = "Success";
        }
        catch (Exception e)
        {
            TempData["Message"] = "Error: " + e.Message;
            TempData["Type"] = "Error";
        }
        
        return RedirectToAction("Index");
    }
    
    [HttpGet]
    public IActionResult GenerateKey()
    {
        var privateKey = _blockChainService.GeneratePrivateKeyXml();
        if (string.IsNullOrWhiteSpace(privateKey))
            return BadRequest("Error generating key");

        return Content(privateKey, "text/plain");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int index)
    {
        var block = await _blockChainService.GetBlockByIndexAsync(index);
        if (block == null) return NotFound();
        return View(block);
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(int index, string data)
    {
        var result = await _blockChainService.EditBlockAsync(index, data);
        if (!result) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}