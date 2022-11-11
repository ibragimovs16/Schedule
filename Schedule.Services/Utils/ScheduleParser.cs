using ClosedXML.Excel;
using Schedule.Domain.Exceptions;

namespace Schedule.Services.Utils;

public static class ScheduleParser
{
    public struct Subject
    {
        public int PairNumber { get; set; }
        public string Name { get; set; }
    }

    public struct GroupData
    {
        public string Date { get; set; }
        public List<Subject> Subjects { get; set; }
    }
    
    public struct Schedule
    {
        public string Group { get; set; }
        public List<GroupData> Data { get; set; }
    }

    public struct Result
    {
        public bool IsSuccess { get; set; }
        public Schedule Schedule { get; set; }
        public Exception Exception { get; set; }
    }

    public static async Task<Result> ParseAsync(string groupName, string scheduleUrl)
    {
        var path = await Downloader.DownloadAsync(scheduleUrl, Guid.NewGuid() + ".xlsx");

        var wb = new XLWorkbook(path);

        bool IsGroupNameInCell(IXLCell item) =>
            item.Value.ToString()!.ToLower()
                .Contains(groupName.ToLower());

        var isGroupNameCorrect = wb.Worksheets
            .Any(ws => ws.Cells()
                    .Where(IsGroupNameInCell)
                    .Count() == 1
            );

        if (!isGroupNameCorrect)
        {
            File.Delete(path);
            return new Result
            {
                IsSuccess = false,
                Exception = new ParserExceptions.IncorrectGroupExceptions("Incorrect group name")
            };
        }

        var schedule = new Schedule
        {
            Data = new List<GroupData>()
        };

        foreach (var wbWorksheet in wb.Worksheets.Where(ws => ws.Cells().Any(IsGroupNameInCell)))
        {
            var cell = wbWorksheet.Cells()
                .FirstOrDefault(IsGroupNameInCell);

            if (cell is null)
            {
                File.Delete(path);
                return new Result
                {
                    IsSuccess = false,
                    Exception = new ParserExceptions.CellNotFoundExceptions("Cell with current group not found")
                };
            }

            var rawName = cell.Value.ToString()!.Split('(')[0].Split(',');
            var name = rawName.Length == 1
                ? rawName[0].ToLower().Trim()
                : rawName.First(item => item.ToLower().Contains(groupName.ToLower())).ToLower().Trim();
            schedule.Group = name;

            var group = new GroupData
            {
                Date = wbWorksheet.Name,
                Subjects = new List<Subject>()
            };

            for (var i = 0; i < 5; i++)
            {
                var value = wbWorksheet.Cell(cell.Address.ColumnLetter + (cell.Address.RowNumber + i + 1)).Value
                    .ToString();
                if (string.IsNullOrWhiteSpace(value)) continue;

                group.Subjects.Add(new Subject
                {
                    PairNumber = i + 1,
                    Name = value
                });
            }

            schedule.Data.Add(group);
        }

        File.Delete(path);
        
        return new Result
        {
            IsSuccess = true,
            Schedule = schedule
        };
    }
}