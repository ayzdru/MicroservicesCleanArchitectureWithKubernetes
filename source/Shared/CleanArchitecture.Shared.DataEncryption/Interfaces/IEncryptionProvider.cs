namespace Microsoft.EntityFrameworkCore.DataEncryption;

/// <summary>
/// Provides a mechanism to encrypt and decrypt data.
/// </summary>
public interface IEncryptionProvider
{
    string Encrypt(string input);   
    string Decrypt(string input);
}