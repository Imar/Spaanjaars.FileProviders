using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace Spaanjaars.FileProviders.Web.Controllers;

public class OldWayController : Controller
{
  private readonly IWebHostEnvironment _webHostEnvironment;

  public OldWayController(IWebHostEnvironment webHostEnvironment)
  {
    _webHostEnvironment = webHostEnvironment;
  }

  public async Task<IActionResult> Index()
  {
    var path = Path.Combine(_webHostEnvironment.ContentRootPath, "DemoSettings\\Demo.txt");
    var contents = await System.IO.File.ReadAllTextAsync(path);

    // Do something with contents here. As an example, I am just returning it to the browser.
    return Ok(contents);
  }
}