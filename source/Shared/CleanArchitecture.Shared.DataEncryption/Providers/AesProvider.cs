using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Providers;

/// <summary>
/// Implements the Advanced Encryption Standard (AES) symmetric algorithm.
/// </summary>
public class AesProvider : IEncryptionProvider
{
    /// <summary>
    /// AES block size constant.
    /// </summary>
    public const int AesBlockSize = 128;

    /// <summary>
    /// Initialization vector size constant.
    /// </summary>
    public const int InitializationVectorSize = 16;

    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly CipherMode _mode;
    private readonly PaddingMode _padding;  
    public AesProvider(string key, string initializationVector, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
    {
        if(string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key), "");
        }
        if (string.IsNullOrEmpty(initializationVector))
        {
            throw new ArgumentNullException(nameof(initializationVector), "");
        }
        _key = Convert.FromBase64String(key);
        _iv = Convert.FromBase64String(initializationVector);
        _mode = mode;
        _padding = padding;
    }

    public string Encrypt(string input)
    {

        if (string.IsNullOrEmpty(input))
        {
            return null;
        }       
        using Aes aes = CreateCryptographyProvider(_key, _iv, _mode, _padding);
        using ICryptoTransform cryptTransform = aes.CreateEncryptor();
        byte[] plaintext = Encoding.UTF8.GetBytes(input);
        byte[] cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);
        return Convert.ToBase64String(cipherText);
    }

    

    /// <inheritdoc />
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            return null;
        }

        

        using Aes aes = CreateCryptographyProvider(_key, _iv, _mode, _padding);
        using ICryptoTransform cryptTransform = aes.CreateDecryptor();
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] plainBytes = cryptTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);

    }

  
   
   

    /// <summary>
    /// Generates an AES cryptography provider.
    /// </summary>
    /// <returns></returns>
    private static Aes CreateCryptographyProvider(byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
    {
        var aes = Aes.Create();

        aes.Mode = mode;
        aes.KeySize = key.Length * 8;
        aes.BlockSize = AesBlockSize;
        aes.FeedbackSize = AesBlockSize;
        aes.Padding = padding;
        aes.Key = key;
        aes.IV = iv;

        return aes;
    }

    /// <summary>
    /// Generates an AES key.
    /// </summary>
    /// <remarks>
    /// The key size of the Aes encryption must be 128, 192 or 256 bits. 
    /// Please check https://blogs.msdn.microsoft.com/shawnfa/2006/10/09/the-differences-between-rijndael-and-aes/ for more informations.
    /// </remarks>
    /// <param name="keySize">AES Key size</param>
    /// <returns></returns>
    public static AesKeyInfo GenerateKey(AesKeySize keySize)
    {
        var aes = Aes.Create();

        aes.KeySize = (int)keySize;
        aes.BlockSize = AesBlockSize;

        aes.GenerateKey();
        aes.GenerateIV();

        return new AesKeyInfo(aes.Key, aes.IV);
    }
}