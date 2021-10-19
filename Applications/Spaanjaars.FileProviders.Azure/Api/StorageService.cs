using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace Spaanjaars.FileProviders.Azure.Api
{
  /// <summary>
  /// Service class to work with files in an Azure storage account.
  /// </summary>
  public class StorageService
  {
    private readonly string _connectionString;

    /// <summary>
    /// Creates a new instance of the Settings class.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the storage account.</param>
    public StorageService(string connectionString)
    {
      _connectionString = connectionString;
    }

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    /// <param name="fileToDelete">The path to the file to delete in the container.</param>
    public async Task DeleteFile(string rootContainer, string fileToDelete)
    {
      var container = await BuildContainerClient(rootContainer);
      var blockBlob = container.GetBlobClient(fileToDelete);
      await blockBlob.DeleteIfExistsAsync();
    }

    /// <summary>
    /// Indicates if the specified file exists.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    /// <param name="fileName">The path to the file to retrieve.</param>
    public async Task<bool> FileExists(string rootContainer, string fileName)
    {
      var container = await BuildContainerClient(rootContainer);
      var blockBlob = container.GetBlobClient(fileName);
      return await blockBlob.ExistsAsync();
    }

    /// <summary>
    /// Downloads the specified file.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    /// <param name="fileName">The path to the file to retrieve.</param>
    public async Task<byte[]> DownloadFile(string rootContainer, string fileName)
    {
      var container = await BuildContainerClient(rootContainer);
      var blockBlob = container.GetBlobClient(fileName);
      await using MemoryStream s = new MemoryStream();
      await blockBlob.DownloadToAsync(s);
      return s.ToArray();
    }

    /// <summary>
    /// Returns all files in the specified root container matching the partial file name.
    /// </summary>
    /// <param name="rootContainer">The root blob container to look in.</param>
    public async Task<IEnumerable<string>> GetFilesInFolderAsync(string rootContainer)
    {
      var container = await BuildContainerClient(rootContainer);
      var list = new List<string>();
      foreach (var item in container.GetBlobs())
      {
        list.Add(item.Name);
      }
      return list;
    }

    /// <summary>
    /// Uploads a new file.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    /// <param name="fileName">The relative path to the file used as the name and location in Azure.</param>
    /// <param name="file">The file contents to upload.</param>
    /// <param name="overwrite">Determines if the file can be overwritten when it already exists. When false and the file exists, an exception is thrown.</param>
    public async Task UploadFile(string rootContainer, string fileName, byte[] file, bool overwrite)
    {
      fileName = fileName.Replace("\\", "/");
      var container = await BuildContainerClient(rootContainer);
      var blockBlobClient = container.GetBlobClient(fileName);
      await blockBlobClient.UploadAsync(new MemoryStream(file), overwrite);
    }

    private async Task<BlobContainerClient> BuildContainerClient(string rootContainer)
    {
      rootContainer = FixUpRootContainer(rootContainer);

      // Retrieve storage account information from connection string
      var serviceClient = new BlobServiceClient(_connectionString);

      // Create a container for organizing blobs within the storage account.
      var container = serviceClient.GetBlobContainerClient(rootContainer);

      await container.CreateIfNotExistsAsync();
      return container;
    }

    /// <summary>
    /// Replaces all characters except for a-z, numbers and the hyphen (-).
    /// </summary>
    /// <param name="input">The name to fix.</param>
    private string FixUpRootContainer(string input)
    {
      if (string.IsNullOrEmpty(input))
      {
        throw new ArgumentException("Missing root container.", nameof(input));
      }
      var rgx = new Regex("[^a-zA-Z0-0-]");
      return rgx.Replace(input, "").ToLower();
    }
  }
}


