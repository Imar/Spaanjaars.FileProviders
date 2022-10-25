namespace Spaanjaars.FileProviders.Infrastructure.Files;

/// <summary>
/// A container class to hold a file's name and path.
/// </summary>
public class FileInfo
{
  /// <summary>
  /// Creates a new instance of the FileInfo class.
  /// </summary>
  /// <param name="name">The file name.</param>
  /// <param name="relativePath">The path to the file relative to its root container.</param>
  /// <param name="path">The full path of the file.</param>
  public FileInfo(string name, string relativePath, string path)
  {
    Name = name;
    RelativePath = relativePath;
    Path = path;
  }

  /// <summary>
  /// Gets the file name.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets the full path of the file.
  /// </summary>
  public string Path { get;  }

  /// <summary>
  /// Gets the path to the file relative to its root container.
  /// </summary>
  public string RelativePath { get; }
}