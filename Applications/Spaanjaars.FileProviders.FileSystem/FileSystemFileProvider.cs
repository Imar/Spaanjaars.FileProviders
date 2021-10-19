using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Spaanjaars.FileProviders.Infrastructure.Files;
using FileInfo = Spaanjaars.FileProviders.Infrastructure.Files.FileInfo;

namespace Spaanjaars.FileProviders.FileSystem
{
  /// <summary>
  /// FileProvider that targets the local file system.
  /// </summary>
  public class FileSystemFileProvider : IFileProvider
  {
    private readonly string _rootFolder;

    /// <summary>
    /// Creates a new instance of the FileSystemFileProvider class.
    /// </summary>
    /// <param name="rootFolder">The root folder for storage, like C:\Files.</param>
    public FileSystemFileProvider(string rootFolder)
    {
      ThrowIfFolderNotExists(rootFolder);
      if (!rootFolder.EndsWith("\\"))
      {
        rootFolder += "\\";
      }
      _rootFolder = rootFolder;
    }

    /// <summary>
    /// Returns all files in the specified root container.
    /// </summary>
    /// <param name="rootContainer">The root container to look in.</param>
    public Task<List<FileInfo>> GetFilesAsync(string rootContainer)
    {
      var folder = GetFolderPath(rootContainer);
      ThrowIfFolderNotExists(folder);
      var files = new DirectoryInfo(folder).GetFiles("*", SearchOption.AllDirectories);
      var result = new List<FileInfo>();
      foreach (var file in files)
      {
        string relativePath = file.FullName.Replace(folder, "", StringComparison.CurrentCultureIgnoreCase);
        result.Add(new FileInfo(file.Name, relativePath, file.FullName));
      }
      return Task.FromResult(result);
    }

    /// <summary>
    /// Returns the requested file.
    /// </summary>
    /// <param name="rootContainer">The root container to look in.</param>
    /// <param name="filePath">The path to the file in the container.</param>
    public async Task<byte[]> GetFileAsync(string rootContainer, string filePath)
    {
      var folder = GetFolderPath(rootContainer);
      ThrowIfFolderNotExists(folder);
      var file = GetFilePath(folder, filePath);
      ThrowIfFileNotExists(file);
      return await File.ReadAllBytesAsync(file);
    }

    /// <summary>
    /// Saves the specified file in the underlying file storage.
    /// </summary>
    /// <param name="rootContainer">The root container to use in the storage account.</param>
    /// <param name="filePath">The path to the file, relative to the rootContainer.</param>
    /// <param name="fileBytes">The file contents.</param>
    /// <param name="overwrite">Determines if the file should be overwritten when it already exists. When false and the file exists, an exception is thrown.</param>
    public async Task StoreFileAsync(string rootContainer, string filePath, byte[] fileBytes, bool overwrite)
    {
      var folder = GetFolderPath(rootContainer);
      var file = GetFilePath(folder, filePath);
      var fileFolder = Path.GetDirectoryName(file);
      if (fileFolder == null)
      {
        throw new Exception($"Could not get parent folder of {file}.");
      }
      Directory.CreateDirectory(fileFolder);
      if (File.Exists(file) && !overwrite)
      {
        throw new InvalidOperationException($"The file {file} already exists.");
      }
      await File.WriteAllBytesAsync(file, fileBytes);
    }

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="rootContainer">The root container to look in.</param>
    /// <param name="filePath">The path to the file to delete in the container.</param>
    public Task DeleteFileAsync(string rootContainer, string filePath)
    {
      var folder = GetFolderPath(rootContainer);
      ThrowIfFolderNotExists(folder);
      var file = GetFilePath(folder, filePath);
      ThrowIfFileNotExists(file);
      File.Delete(file);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes all files and folders from the specified root container.
    /// </summary>
    /// <param name="rootContainer">The root container to look in.</param>
    public Task ClearAsync(string rootContainer)
    {
      var folder = GetFolderPath(rootContainer);
      ThrowIfFolderNotExists(folder);
      var di = new DirectoryInfo(folder);
      foreach (var file in di.GetFiles())
      {
        file.Delete();
      }
      foreach (var dir in di.GetDirectories())
      {
        dir.Delete(true);
      }
      return Task.CompletedTask;
    }

    private void ThrowIfFolderNotExists(string folder)
    {
      if (!Directory.Exists(folder))
      {
        throw new DirectoryNotFoundException($"Root folder {folder} not found.");
      }
    }

    private void ThrowIfFileNotExists(string file)
    {
      if (!File.Exists(file))
      {
        throw new FileNotFoundException($"File {file} not  found.", file);
      }
    }

    private string GetFolderPath(string rootContainer)
    {
      if (!rootContainer.EndsWith("\\"))
      {
        rootContainer += "\\";
      }
      return Path.Combine(_rootFolder, rootContainer);
    }

    private string GetFilePath(string folder, string filePath)
    {
      return Path.Combine(folder, filePath);
    }
  }
}
