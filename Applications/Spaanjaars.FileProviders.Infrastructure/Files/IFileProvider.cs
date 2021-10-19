using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spaanjaars.FileProviders.Infrastructure.Files
{
  /// <summary>
  /// An interface to abstract file storage such as the local file system or Azure storage.
  /// </summary>
  public interface IFileProvider
  {
    /// <summary>
    /// Returns all files in the specified root container.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    Task<List<FileInfo>> GetFilesAsync(string rootContainer);

    /// <summary>
    /// Returns the requested file.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    /// <param name="filePath">The path to the file in the container.</param>
    Task<byte[]> GetFileAsync(string rootContainer, string filePath);

    /// <summary>
    /// Saves the specified file in the underlying file storage.
    /// </summary>
    /// <param name="rootContainer">The root container to use in the storage account.</param>
    /// <param name="filePath">The name of the file.</param>
    /// <param name="fileBytes">The file contents.</param>
    /// <param name="overwrite">Determines if the file should be overwritten when it already exists. When false and the file exists, an exception is thrown.</param>
    Task StoreFileAsync(string rootContainer, string filePath, byte[] fileBytes, bool overwrite);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    /// <param name="filePath">The path to the file to delete in the container.</param>
    Task DeleteFileAsync(string rootContainer, string filePath);

    /// <summary>
    /// Deletes all files and folders from the specified root container.
    /// </summary>
    /// <param name="rootContainer">The root container to look in. On Azure, this is a blob container.</param>
    //Task ClearAsync(string rootContainer);
  }
}
