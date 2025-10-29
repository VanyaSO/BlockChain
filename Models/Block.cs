using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Cryptography;

namespace BlockChain.Models;

public class Block
{
    [Key]
    public int Id { get; set; }
    public int Index { get; set; }
    public string Data { get; set; }
    public string PrevHash { get; set; }
    public string Hash { get; set; }
    public DateTime Timestamp { get; set; }

    public Block()
    {
    }

    public Block(int index, string data, string prevHash)
    {
        Index = index;
        Data = data;
        PrevHash = prevHash;
        Timestamp = DateTime.UtcNow;
        Hash = ComputeHash();
    }

    public string ComputeHash()
    {
        var ts = new DateTime(Timestamp.Ticks - (Timestamp.Ticks % TimeSpan.TicksPerMillisecond), DateTimeKind.Utc);
        var raw = Index + Data + PrevHash + ts.ToString("O");

        using var sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return BitConverter.ToString(bytes).Replace("-", "");
    }
}