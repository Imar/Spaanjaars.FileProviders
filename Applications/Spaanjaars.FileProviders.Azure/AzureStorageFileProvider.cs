using Spaanjaars.FileProviders.Azure.Api;
using Spaanjaars.FileProviders.Infrastructure.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spaanjaars.FileProviders.Azure;

/// <summary>
/// A file provider that works with an Azure storage account as the underlying data store.
/// </summary>
public class AzureStorageFileProvider : IFileProvider
{
  private readonly StorageService _storageService;

  /// <summary>
  /// Creates a new instance of the AzureStorageFileProvider class.
  /// </summary>
  /// <param name="connectionString">The connection string used to connect to the Azure storage account</param>
  public AzureStorageFileProvider(string connectionString)
  {
    _storageService = new StorageService(connectionString);
  }

  /// <summary>
  /// Saves the specified file in the underlying file storage.
  /// </summary>
  /// <param name="rootContainer">The root container to use. On Azure, this is a blob container.</param>
  /// <param name="fileName">The name of the file.</param>
  /// <param name="fileBytes">The file contents.</param>
  /// <param name="overwrite">Determines if the file should be overwritten when it already exists. When false and the file exists, an exception is thrown.</param>
  public async Task StoreFileAsync(string rootContainer, string fileName, byte[] fileBytes, bool overwrite)
  {
    if (await _storageService.FileExists(rootContainer, fileName) && !overwrite)
    {
      throw new InvalidOperationException($"The file {fileName} already exists.");
    }
    await _storageService.UploadFile(rootContainer, fileName, fileBytes, overwrite);
  }

  /// <summary>
  /// Returns all files in the specified root container.
  /// </summary>
  /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
  public async Task<List<FileInfo>> GetFilesAsync(string rootContainer)
  {
    var files = await _storageService.GetFilesInFolderAsync(rootContainer);
    var result = new List<FileInfo>();
    foreach (var item in files)
    {
      var fileName = item.Contains("/") ? item.Substring(item.LastIndexOf('/') + 1) : item;
      result.Add(new FileInfo(fileName, item, item));
    }
    return result;
  }

  /// <summary>
  /// Gets a file from the underlying file storage.
  /// </summary>
  /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
  /// <param name="filePath">The relative path to the file to retrieve.</param>
  public async Task<byte[]> GetFileAsync(string rootContainer, string filePath)
  {
    return await _storageService.DownloadFile(rootContainer, filePath);
  }

  /// <summary>
  /// Deletes a file.
  /// </summary>
  /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
  /// <param name="fileToDelete">The path to the file to delete in the container.</param>
  public async Task DeleteFileAsync(string rootContainer, string fileToDelete)
  {
    await _storageService.DeleteFile(rootContainer, fileToDelete);
  }

  /// <summary>
  /// Deletes all files and folders from the specified root container.
  /// </summary>
  /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
  public async Task ClearAsync(string rootContainer)
  {
    var allFiles = await _storageService.GetFilesInFolderAsync(rootContainer);
    foreach (var file in allFiles)
    {
      await _storageService.DeleteFile(rootContainer, file);
    }
  }
}