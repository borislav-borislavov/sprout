using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace Sprout.Core.Services.Configurations;

public static class SeedFileCrypto
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("SPROUTSEED1"); // format marker + version
    private const int SaltSize = 16;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 210_000;

    public static bool IsEncrypted(string path)
    {
        using var fs = File.OpenRead(path);
        if (fs.Length < Magic.Length + SaltSize + NonceSize + TagSize)
            return false;

        byte[] header = new byte[Magic.Length];
        fs.ReadExactly(header);
        return header.AsSpan().SequenceEqual(Magic);
    }

    public static void Encrypt(string path, string plaintext, string passphrase) =>
        Encrypt(path, Encoding.UTF8.GetBytes(plaintext), passphrase);

    public static void Encrypt(string path, byte[] plaintext, string passphrase)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(passphrase, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[TagSize];
        using (var aes = new AesGcm(key, TagSize))
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

        using var fs = File.Create(path);
        fs.Write(Magic);
        fs.Write(salt);
        fs.Write(nonce);
        fs.Write(tag);
        fs.Write(ciphertext);
    }

    public static string Decrypt(string path, string passphrase)
    {
        byte[] data = File.ReadAllBytes(path);
        if (!data.AsSpan(0, Magic.Length).SequenceEqual(Magic))
            throw new InvalidDataException("File is not an encrypted seed file.");

        int offset = Magic.Length;
        var salt = data.AsSpan(offset, SaltSize);
        var nonce = data.AsSpan(offset + SaltSize, NonceSize);
        var tag = data.AsSpan(offset + SaltSize + NonceSize, TagSize);
        var ciphertext = data.AsSpan(offset + SaltSize + NonceSize + TagSize);

        byte[] key = Rfc2898DeriveBytes.Pbkdf2(passphrase, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        byte[] plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext); // throws if wrong password or tampered
        return Encoding.UTF8.GetString(plaintext);
    }
}
