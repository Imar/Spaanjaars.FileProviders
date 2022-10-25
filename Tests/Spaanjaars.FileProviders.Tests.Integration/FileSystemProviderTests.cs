using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spaanjaars.FileProviders.FileSystem;

namespace Spaanjaars.FileProviders.Tests.Integration;

[TestClass]
public class FileSystemProviderTests
{
  const string RootFolderTestFiles = "C:\\TestFiles\\";
  private FileSystemFileProvider _systemUnderTest;

  [TestCleanup]
  public void CleanUp()
  {
    DirectoryInfo di = new DirectoryInfo(RootFolderTestFiles);
    foreach (var file in di.GetFiles())
    {
      file.Delete();
    }
    foreach (var dir in di.GetDirectories())
    {
      dir.Delete(true);
    }
  }

  private FileSystemFileProvider SystemUnderTest
  {
    get
    {
      return _systemUnderTest ??= new FileSystemFileProvider(RootFolderTestFiles);
    }
  }

  [TestMethod]
  public async Task CanSaveFile()
  {
    var name = $"{Guid.NewGuid()}.txt";
    await SystemUnderTest.StoreFileAsync("Test", name, Encoding.UTF8.GetBytes("Hello world"), false);
    Assert.IsTrue(File.Exists(RootFolderTestFiles + "Test\\" + name));
  }

  [TestMethod]
  public async Task CanOverwriteFileWhenItExistsAndOverwritingIsAllowed()
  {
    var name = $"{Guid.NewGuid()}.txt";
    await WriteTestFile("Test", name);
    Assert.IsTrue(File.Exists(RootFolderTestFiles + "Test\\" + name));
    await SystemUnderTest.StoreFileAsync("Test", name, Encoding.UTF8.GetBytes("Hello world"), true);
    Assert.IsTrue(File.Exists(RootFolderTestFiles + "Test\\" + name));
  }

  [TestMethod]
  public async Task CannotOverwriteFileWhenItExistsAndOverwritingIsNotAllowed()
  {
    var name = $"{Guid.NewGuid()}.txt";
    await WriteTestFile("Test", name);
    Assert.IsTrue(File.Exists(RootFolderTestFiles + "Test\\" + name));
    try
    {
      await SystemUnderTest.StoreFileAsync("Test", name, Encoding.UTF8.GetBytes("Hello world"), false);
      Assert.Fail("Should not get here");
    }
    catch (Exception ex)
    {
      Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
    }
  }

  [TestMethod]
  public async Task CanDeleteFile()
  {
    var file = await WriteTestFile("Test");
    Assert.IsTrue(File.Exists(RootFolderTestFiles + "Test\\" + file));
    await SystemUnderTest.DeleteFileAsync("Test", file);
    Assert.IsFalse(File.Exists(RootFolderTestFiles + "Test\\" + file));
  }

  [TestMethod]
  public async Task CanGetFile()
  {
    var file1 = await WriteTestFile("TestGet");
    var fileBytes = await SystemUnderTest.GetFileAsync("TestGet", file1);
    var result = Encoding.UTF8.GetString(fileBytes);
    Assert.AreEqual("Delete me", result);
  }

  [TestMethod]
  public async Task CanGetFiles()
  {
    var file1 = await WriteTestFile("TestGet");
    var file2 = await WriteTestFile("TestGet");
    var files = (await SystemUnderTest.GetFilesAsync("TestGet")).ToList();
    Assert.IsNotNull(files.SingleOrDefault(x => x.Path.EndsWith(file1)));
    Assert.IsNotNull(files.SingleOrDefault(x => x.Path.EndsWith(file2)));
  }

  private async Task<string> WriteTestFile(string subFolder, string fileName = null)
  {
    var name = fileName ?? $"{Guid.NewGuid()}.txt";
    Directory.CreateDirectory(RootFolderTestFiles + subFolder);
    var filePath = RootFolderTestFiles + $"{subFolder}\\" + name;
    await File.WriteAllTextAsync(filePath, "Delete me");
    return name;
  }
}