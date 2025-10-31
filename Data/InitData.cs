using System.Security.Cryptography;
using BlockChain.Models;

namespace BlockChain.Data;

public static class InitData
{
    public static void Init(ApplicationDbContext context)
    {
        if (!context.Blocks.Any())
        {
            using var rsa = RSA.Create(2048);
            var privateKey = rsa.ExportParameters(true);
            var publicKey = rsa.ToXmlString(false);
            
            var genBlock = new Block(0, "genesis-block", "");
            genBlock.Sign(privateKey, publicKey);
            
            context.Blocks.Add(genBlock);
            context.SaveChanges();
        }
    }
}