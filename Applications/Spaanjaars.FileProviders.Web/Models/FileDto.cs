namespace Spaanjaars.FileProviders.Web.Models;

/// <summary>
/// DTO for a file.
/// </summary>
public class FileDto
{
  /// <summary>
  /// Creates a new instance of the FileDto class.
  /// </summary>
  /// <param name="name">The name of the file.</param>
  public FileDto(string name)
  {
    Name = name;
  }

  /// <summary>
  /// Gets the name of the file.
  /// </summary>
  public string Name { get; }
}