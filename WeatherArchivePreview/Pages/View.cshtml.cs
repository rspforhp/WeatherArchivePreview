using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WeatherArchivePreview.Data;

namespace WeatherArchivePreview.Pages;

public class View : PageModel
{
    public readonly IDbContextFactory<AppDbContext> _contextFactory;
    private IWebHostEnvironment _environment;
    public List<string> Months;

    public List<AppDbContext.Weather> Weathers;
    public List<int> Years;

    public View(IWebHostEnvironment environment, IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        _environment = environment;
    }

    [BindProperty] public string Year { get; set; }

    [BindProperty] public string Month { get; set; }

    public void OnPost()
    {
        Console.WriteLine($"Number is {Year}");
        var monthName = "None";
        if (Month != "None")
        {
            var ind = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.ToList().IndexOf(Month);
            monthName = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[ind];
        }

        Response.Redirect($"/View?page={Request.Query["page"]}&year_filter={Year}&month_filter={monthName}");
    }

    public void OnGet()
    {
        Year = Request.Query["year_filter"];
        var m = Request.Query["month_filter"];
        if (m.Count > 0 && m != "None" && m.ToString() != null)
        {
            var ind = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames.ToList().IndexOf(m);

            Month = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[ind];
        }

        Months = new List<string>();
        for (var i = 1; i < 13; i++) Months.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
        Weathers = new List<AppDbContext.Weather>();
        var page = int.Parse(Request.Query["page"]);
        using var db = _contextFactory.CreateDbContext();
        List<AppDbContext.Weather> ws;
        try
        {
            ws = db.Weathers.ToList();
        }
        catch (Exception e)
        {
            //Database doesnt exist;P
            return;
        }

        {
            var ar = ws.OrderBy(a => a.DateNTime).ToList();
            Years = ar.Select(a => a.DateNTime.Year).Distinct().ToList();
            if (!string.IsNullOrEmpty(Month) && Month != "None")
                ar = ar.FindAll(a =>
                    CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[a.DateNTime.Month - 1] == Month);
            if (!string.IsNullOrEmpty(Year) && Year != "None")
                ar = ar.FindAll(a => a.DateNTime.Year.ToString() == Year);
            if (page * 30 > ar.Count)
            {
                var loc =
                    $"/View?page={ar.Count / 30 + (ar.Count % 30 > 0 ? 0 : -1)}&month_filter={Request.Query["month_filter"]}&year_filter={Request.Query["year_filter"]}";
                Response.Redirect(loc);
                return;
            }

            for (var i = 0; i < 30; i++)
                try
                {
                    Weathers.Add(ar[i + page * 30]);
                }
                catch (ArgumentException e)
                {
                }
        }
    }
}