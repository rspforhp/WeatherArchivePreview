using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;

namespace WeatherArchivePreview.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    public DbSet<Weather> Weathers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("connection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public class Weather
    {
        public enum eWindDirections
        {
            штиль,
            С,
            СВ,
            В,
            ЮВ,
            Ю,
            ЮЗ,
            З,
            СЗ
        }

        public Weather()
        {
        }

        public Weather(IRow row)
        {
            try
            {
                var cells = row.Cells;
                var t = TimeOnly.Parse(cells[1].StringCellValue, CultureInfo.InvariantCulture);
                var d = DateOnly.Parse(cells[0].StringCellValue);
                DateNTime = new DateTimeOffset(t.Ticks + d.DayNumber * TimeSpan.TicksPerDay, TimeSpan.Zero);
                Temperature = cells[2].NumericCellValue;
                Humidity = (int)cells[3].NumericCellValue;
                DewPoint = cells[4].NumericCellValue;
                Pressure = (int)cells[5].NumericCellValue;
                WindDirections = cells[6].StringCellValue
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Enum.Parse<eWindDirections>(s)).ToArray();
                if (cells[7].CellType == CellType.Numeric)
                    WindSpeed = (int)cells[7].NumericCellValue;
                else WindSpeed = null;
                if (cells[8].CellType == CellType.Numeric)
                    Cloudlinnes = (int)cells[8].NumericCellValue;
                else Cloudlinnes = null;
                if (cells[9].CellType == CellType.Numeric)
                    LowerCloudLimit = (int)cells[9].NumericCellValue;
                else LowerCloudLimit = null;
                if (cells[10].CellType == CellType.Numeric)
                    HorizontalVisibility = (int)cells[10].NumericCellValue;
                else HorizontalVisibility = null;
                WeatherPhenomena = cells[11].StringCellValue;
            }
            catch (ArgumentOutOfRangeException e)
            {
            }
        }

        [Key] public DateTimeOffset DateNTime { get; set; }
        public double Temperature { get; set; }
        public int Humidity { get; set; }
        public double DewPoint { get; set; }
        public int Pressure { get; set; }

        public eWindDirections[] WindDirections { get; set; }


        public int? WindSpeed { get; set; }
        public int? Cloudlinnes { get; set; }
        public int? LowerCloudLimit { get; set; }
        public int? HorizontalVisibility { get; set; }
        public string? WeatherPhenomena { get; set; }

        public override string ToString()
        {
            return
                $"Дата и время: {DateNTime.ToString("g")}, Температура: {Temperature}, Влажность {Humidity}%, Точка росы: {DewPoint}, Давление: {Pressure} мм рт.ст. , " +
                (WindDirections.Length > 0
                    ? $"Направление: {string.Join(", ", WindDirections.Select(a => a.ToString()))}, "
                    : "") + (WindSpeed != null ? $"Скорость ветра: {WindSpeed} м/с," : "") +
                (Cloudlinnes != null ? $" Облачность: {Cloudlinnes}%," : "")
                + (LowerCloudLimit != null ? $" Нижняя граница облачности: {LowerCloudLimit} м, " : "")
                + (HorizontalVisibility != null ? $" Горизонтальная видимость: {HorizontalVisibility} км, " : "")
                + (WeatherPhenomena != null ? $" Погодные явления: {WeatherPhenomena}" : "");
        }
    }
}