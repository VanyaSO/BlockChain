using BlockChain.Models;

namespace BlockChain.Data;

public static class InitData
{
    public static void Init(ApplicationDbContext context)
    {
        if (!context.Blocks.Any())
        {
            var genBlock = new Block(0, "genesis-block", "");
            context.Blocks.Add(genBlock);
            context.SaveChanges();
        }
    }
}