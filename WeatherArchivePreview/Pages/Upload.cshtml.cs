using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeatherArchivePreview.Pages;

public class Upload : PageModel
{
    private IWebHostEnvironment _environment;

    public Upload(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void OnGet()
    {
    }
}