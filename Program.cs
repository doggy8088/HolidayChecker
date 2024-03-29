﻿Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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

// ----------------------------------------------------------------------------

static async Task<List<Holiday>> GetHolidays()
{
    var config = new CsvConfiguration(CultureInfo.CurrentCulture)
    {
        Mode = CsvMode.RFC4180,
        PrepareHeaderForMatch = args => args.Header.ToLower()
    };

    var client = new HttpClient();
    var url = "https://data.taipei/api/frontstage/tpeod/dataset/resource.download?rid=964e936d-d971-4567-a467-aa67b930f98e";
    var stream = await client.GetStreamAsync(url);
    using var reader = new StreamReader(stream);
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
    [Display(Name = "Date")]
    public DateOnly Date { get; set; }

    [Display(Name = "name")]
    public string? Name { get; set; }

    [Display(Name = "isHoliday")]
    public bool IsHoliday { get; set; }

    [Display(Name = "holidayCategory")]
    public string? HolidayCategory { get; set; }

    [Display(Name = "description")]
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

        var datestr = text.Substring(0, 4) + "-" + text.Substring(4, 2) + "-" + text.Substring(6, 2);
        if (DateOnly.TryParse(datestr, out DateOnly date))
        {
            return date;
        }
        else
        {
            return default(DateOnly);
        }
    }
}
