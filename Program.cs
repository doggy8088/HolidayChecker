using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

Stopwatch sw = new();

sw.Start();
var data = await GetHolidays();
sw.Stop();

Console.WriteLine($"取得《臺北市政府行政機關辦公日曆表》資料共花費 {sw.ElapsedMilliseconds}ms");

DateOnly date = DateOnly.FromDateTime(DateTime.Now);

if (args.Length > 0)
{
    date = DateOnly.Parse(args[0]);
}

var holiday = data.FirstOrDefault(d => d.Date.Equals(date));

if (holiday == null)
{
    holiday = new Holiday()
    {
        Date = date,
        Name = DateTimeFormatInfo.CurrentInfo.DayNames[(byte)date.DayOfWeek],
        IsHoliday = false,
        HolidayCategory = "",
        Description = ""
    };

    Console.WriteLine("查無資料，預設為非假日！");
}

Console.WriteLine(holiday.Dump());

static async Task<List<Holiday>> GetHolidays()
{
    var encoding = Encoding.GetEncoding("BIG5");

    var config = new CsvConfiguration(CultureInfo.CurrentCulture)
    {
        Mode = CsvMode.RFC4180,
        PrepareHeaderForMatch = args => args.Header.ToLower()
    };

    var client = new HttpClient();
    var url = "https://data.taipei/api/frontstage/tpeod/dataset/resource.download?rid=29d9771d-c0ee-40d4-8dfb-3866b0b7adaa";
    var stream = await client.GetStreamAsync(url);
    using var reader = new StreamReader(stream, encoding);
    using var csv = new CsvReader(reader, config);
    csv.Context.RegisterClassMap<HolidayMap>();

    List<Holiday> list = new();

    foreach (var item in csv.GetRecords<Holiday>())
    {
        if (String.IsNullOrEmpty(item.Name))
        {
            item.Name = item.HolidayCategory;
        }

        if (item.Name == "星期六、星期日")
        {
            item.Name = DateTimeFormatInfo.CurrentInfo.DayNames[(byte)item.Date.DayOfWeek];
        }

        // 軍人節只有軍人才放假，勞工不放假
        if (item.Name == "軍人節")
        {
            item.IsHoliday = false;
        }

        list.Add(item);
    }

    return list;
}

#nullable enable

class Holiday
{
    [Display(Name = "日期")]
    public DateOnly Date { get; set; }

    [Display(Name = "假日名稱")]
    public string? Name { get; set; }

    [Display(Name = "是否為假日")]
    public bool IsHoliday { get; set; }

    [Display(Name = "假日種類")]
    public string? HolidayCategory { get; set; }

    [Display(Name = "備註")]
    public string? Description { get; set; }
}

class HolidayMap : ClassMap<Holiday>
{
    public HolidayMap()
    {
        Map(m => m.Date).TypeConverter(new DateOnlyConverter());
        Map(m => m.Name);
        Map(m => m.IsHoliday).TypeConverter(new IsHolidayConverter());
        Map(m => m.HolidayCategory);
        Map(m => m.Description);
    }
}

class IsHolidayConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text == null) return false;

        foreach (var nullValue in memberMapData.TypeConverterOptions.NullValues)
        {
            if (text == nullValue) return false;
        }

        return (text == "是");
    }
}

class DateOnlyConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text == null) return default(DateOnly);

        foreach (var nullValue in memberMapData.TypeConverterOptions.NullValues)
        {
            if (text == nullValue) return default(DateOnly);
        }

        DateOnly date;
        if (DateOnly.TryParse(text, out date))
        {
            return date;
        }
        else
        {
            return default(DateOnly);
        }
    }
}
