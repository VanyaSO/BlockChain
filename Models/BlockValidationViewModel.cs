namespace BlockChain.Models;

public class BlockValidationViewModel
{
    public Block Block { get; set; } = null!;
    public bool IsValid { get; set; }
}
