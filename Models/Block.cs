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
    
    public string Signature { get; set; }
    public string PublicKey { get; set; }

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

    public void Sign(RSAParameters privateKey, string publicKey)
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(privateKey);
        
        byte[] data = Encoding.UTF8.GetBytes(Hash);
        byte[] sig = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Signature = Convert.ToBase64String(sig);
        PublicKey = publicKey;
    }

    public bool Verify()
    {
        if (string.IsNullOrWhiteSpace(Signature)) return false;

        try
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(PublicKey);
            
            byte[] data = Encoding.UTF8.GetBytes(Hash);
            byte[] sig = Convert.FromBase64String(Signature);
            return rsa.VerifyData(data, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    public void UpdateSignature(string newSignature)
    {
        Signature = newSignature;
    }
}