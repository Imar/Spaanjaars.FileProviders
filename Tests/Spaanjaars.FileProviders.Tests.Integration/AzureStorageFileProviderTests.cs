using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spaanjaars.FileProviders.Azure;

namespace Spaanjaars.FileProviders.Tests.Integration
{
  /// <summary>
  /// *Integration* tests for the AzureStorageFileProvider.
  /// </summary>
  [TestClass]
  public class AzureStorageFileProviderTests
  {
    private const string ConnectionString = "UseDevelopmentStorage=true";
    private const string RootContainer = "Test";
    private AzureStorageFileProvider _systemUnderTest;

    [TestCleanup]
    public async Task CleanUp()
    {
      await SystemUnderTest.ClearAsync(RootContainer);
      (await SystemUnderTest.GetFilesAsync(RootContainer)).Count.Should().Be(0);
    }

    private AzureStorageFileProvider SystemUnderTest
    {
      get
      {
        return _systemUnderTest ??= new AzureStorageFileProvider(ConnectionString);
      }
    }

    [TestMethod]
    public async Task CanSaveFile()
    {
      var name = $"{Guid.NewGuid()}.txt";
      var fileContents = Encoding.UTF8.GetBytes("Hello world");
      await SystemUnderTest.StoreFileAsync(RootContainer, name, fileContents, false);

      var checkFile = await SystemUnderTest.GetFileAsync(RootContainer, name);
      checkFile.Should().BeEquivalentTo(fileContents);
    }

    [TestMethod]
    public async Task CanOverwriteFileWhenItExistsAndOverwritingIsAllowed()
    {
      var name = $"{Guid.NewGuid()}.txt";
      await WriteTestFile(name);
      var fileContents = Encoding.UTF8.GetBytes("Hello world " + Guid.NewGuid());

      Func<Task> act = () => SystemUnderTest.StoreFileAsync(RootContainer, name, fileContents, true);
      await act.Should().NotThrowAsync<InvalidOperationException>();

      var checkFile = await SystemUnderTest.GetFileAsync(RootContainer, name);
      checkFile.Should().BeEquivalentTo(fileContents);
    }

    [TestMethod]
    public async Task CannotOverwriteFileWhenItExistsAndOverwritingIsNotAllowed()
    {
      var name = $"{Guid.NewGuid()}.txt";
      await WriteTestFile(name);
      Func<Task> act = () => SystemUnderTest.StoreFileAsync(RootContainer, name, Encoding.UTF8.GetBytes("Hello world"), false);
      await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task CanDeleteFile()
    {
      var file = await WriteTestFile();
      var checkFile = SystemUnderTest.GetFileAsync(RootContainer, file);
      checkFile.Should().NotBeNull();
      await SystemUnderTest.DeleteFileAsync(RootContainer, file);
      checkFile = SystemUnderTest.GetFileAsync(RootContainer, file);
      checkFile.Should().NotBeNull();
    }

    [TestMethod]
    public async Task CanGetFile()
    {
      var fileContents = "Test CanGetFile";
      var file1 = await WriteTestFile(null, fileContents);
      var fileBytes = await SystemUnderTest.GetFileAsync(RootContainer, file1);
      var result = Encoding.UTF8.GetString(fileBytes);
      Assert.AreEqual(fileContents, result);
    }

    [TestMethod]
    public async Task CanGetFiles()
    {
      var file1 = await WriteTestFile();
      var file2 = await WriteTestFile();
      var files = (await SystemUnderTest.GetFilesAsync(RootContainer)).ToList();
      Assert.IsNotNull(files.SingleOrDefault(x => x.Path.EndsWith(file1)));
      Assert.IsNotNull(files.SingleOrDefault(x => x.Path.EndsWith(file2)));
    }

    private async Task<string> WriteTestFile(string fileName = null, string fileContents = null)
    {
      var name = fileName ?? $"{Guid.NewGuid()}.txt";
      var contents = Encoding.UTF8.GetBytes(fileContents ?? "Hello world");
      await SystemUnderTest.StoreFileAsync(RootContainer, name, contents, false);
      var checkFile = await SystemUnderTest.GetFileAsync(RootContainer, name);
      checkFile.Should().BeEquivalentTo(contents);
      return name;
    }
  }
}
