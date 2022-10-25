using System;
using System.Threading.Tasks;
using Spaanjaars.FileProviders.Azure;
using Spaanjaars.FileProviders.FileSystem;

namespace Spaanjaars.FileProviders.Migrator;

internal class Program
{
  private static readonly string[] RootContainers = { "Images", "Settings" }; // Update to match root containers you like to move.

  static async Task Main(string[] args)
  {
    var source = new FileSystemFileProvider("C:\\Files"); // Hardcoded as this is a throw-away app anyway.
    var target = new AzureStorageFileProvider("UseDevelopmentStorage=true"); // Hardcoded as this is a throw-away app anyway.

    Console.WriteLine("Continuing will delete all files in the target. Do you want to continue?");
    if (Console.ReadKey(true).Key != ConsoleKey.Y)
    {
      Console.WriteLine("Exiting");
      return;
    }
    var count = 0;
    foreach (var rootContainer in RootContainers)
    {
      await target.ClearAsync(rootContainer);

      var allFileInfos = await source.GetFilesAsync(rootContainer);
      foreach (var fileInfo in allFileInfos)
      {
        var fileBytes = await source.GetFileAsync(rootContainer, fileInfo.Path);
        await target.StoreFileAsync(rootContainer, fileInfo.RelativePath, fileBytes, false);
      }
      count += allFileInfos.Count;
    }
    Console.WriteLine($"Done copying {count} files.");
  }
}