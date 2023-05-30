using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TProvider"></typeparam>
internal sealed class EncryptionConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
{
    /// <summary>
    /// Creates a new <see cref="EncryptionConverter{TModel,TProvider}"/> instance.
    /// </summary>
    /// <param name="encryptionProvider">Encryption provider to use.</param>
    /// <param name="storageFormat">Encryption storage format.</param>
    /// <param name="mappingHints">Mapping hints.</param>
    public EncryptionConverter(IEncryptionProvider encryptionProvider, StorageFormat storageFormat, ConverterMappingHints mappingHints = null)
        : base(
            x => Encrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            x => Decrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            mappingHints)
    {
    }

    private static TOutput Encrypt<TInput, TOutput>(TInput input, IEncryptionProvider encryptionProvider, StorageFormat storageFormat)
    {
        string inputData = null;

        switch (storageFormat)
        {
            case StorageFormat.Default or StorageFormat.Base64:
                inputData = input as string;
                break;
            case StorageFormat.Binary:
                inputData = Encoding.UTF8.GetString(input as byte[]);
                break;
        }

        string encryptedRaw = encryptionProvider.Encrypt(inputData);

        if (encryptedRaw is null)
        {
            return default;
        }

        object encryptedData = storageFormat switch
        {
            StorageFormat.Default or StorageFormat.Base64 => encryptedRaw,
            _ => Encoding.UTF8.GetBytes(encryptedRaw)
        };

        return (TOutput)Convert.ChangeType(encryptedData, typeof(TOutput));
    }    
    private static TModel Decrypt<TInput, TOupout>(TProvider input, IEncryptionProvider encryptionProvider, StorageFormat storageFormat)
    {
        Type destinationType = typeof(TModel);
        string inputData = null;

        switch(storageFormat)
        {
            case StorageFormat.Default or StorageFormat.Base64:
                inputData = input as string;
                break;
            case StorageFormat.Binary:
                inputData = Encoding.UTF8.GetString(input as byte[]);
                break;
        }
        string decryptedRaw = encryptionProvider.Decrypt(inputData);
        object decryptedData = null;

        if (destinationType == typeof(string))
        {
            decryptedData = decryptedRaw.Trim('\0');
        }
        else if (destinationType == typeof(byte[]))
        {
            decryptedData = Encoding.UTF8.GetBytes(decryptedRaw);
        }

        return (TModel)Convert.ChangeType(decryptedData, typeof(TModel));
    }
}