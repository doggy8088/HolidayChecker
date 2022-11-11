# HolidayChecker

使用 C# 與 CsvHelper 套件解析《臺北市政府行政機關辦公日曆表》公開資料

## 使用方式

- 查詢今天是否為假日

    ```sh
    HolidayChecker
    ```

    ```txt
    取得《臺北市政府行政機關辦公日曆表》資料共花費 844ms
    {Holiday}
      Date: {DateOnly}
        Year: 2022
        Month: 11
        Day: 12
        DayOfWeek: DayOfWeek.Saturday
        DayOfYear: 316
        DayNumber: 738470
      Name: "星期六"
      IsHoliday: true
      HolidayCategory: "星期六、星期日"
      Description: ""
    ```

- 查詢特定日子是否為假日

    ```sh
    HolidayChecker 2023/1/1
    ```

    ```txt
    取得《臺北市政府行政機關辦公日曆表》資料共花費 618ms
    {Holiday}
      Date: {DateOnly}
        Year: 2023
        Month: 1
        Day: 1
        DayOfWeek: DayOfWeek.Sunday
        DayOfYear: 1
        DayNumber: 738520
      Name: "中華民國開國紀念日"
      IsHoliday: true
      HolidayCategory: "放假之紀念日及節日"
      Description: "全國各機關學校放假一日，適逢星期日，於一月二日補假一日。"
    ```
