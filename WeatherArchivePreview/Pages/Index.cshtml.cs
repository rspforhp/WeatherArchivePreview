using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeatherArchivePreview.Pages;

public class Index : PageModel
{
    public void OnFirstButtonOnClick()
    {
        Console.WriteLine("test");
    }

    public void OnGet()
    {
    }
}