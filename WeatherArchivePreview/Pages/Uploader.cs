using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;
using WeatherArchivePreview.Data;

namespace WeatherArchivePreview.Pages;

public class Uploader : Controller
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private IWebHostEnvironment _environment;

    public Uploader(IWebHostEnvironment environment, IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        _environment = environment;
    }


    [HttpPost]
    public IActionResult up_post()
    {
        Console.WriteLine("test3");

        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();
        context.Database.BeginTransaction();
        var wL = new List<AppDbContext.Weather>();
        lock (wL)
        {
            var weathers = context.Weathers;
            var filelist = HttpContext.Request.Form.Files;
            if (filelist.Count > 0)
                Parallel.ForEach(filelist, delegate(IFormFile file)
                {
                    var workbook = new XSSFWorkbook(file.OpenReadStream());
                    Parallel.For(0, workbook.NumberOfSheets, delegate(int sheetNum, ParallelLoopState state)
                    {
                        var sheet = workbook.GetSheetAt(sheetNum);
                        Parallel.For(4, sheet.LastRowNum + 1, delegate(int rowNum, ParallelLoopState loopState)
                        {
                            var row = sheet.GetRow(rowNum);
                            var w = new AppDbContext.Weather(row);
                            wL.Add(w);
                        });
                    });
                });
        }

        var list = context.Weathers.Select(a => a).ToList();
        foreach (var w in wL)
            if (list.Find(a => a.DateNTime == w.DateNTime) == null)
                context.Weathers.Add(w);


        context.SaveChanges();
        context.Database.CommitTransaction();

        return Ok("Uploaded!");
    }
}