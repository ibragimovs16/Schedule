namespace Schedule.Domain.Exceptions;

public class ParserExceptions : Exception
{
    private ParserExceptions(string message) : base(message)
    {
    }
    
    public class IncorrectGroupExceptions : ParserExceptions
    {
        public IncorrectGroupExceptions(string message) : base(message)
        {
        }
    }
    
    public class CellNotFoundExceptions : ParserExceptions
    {
        public CellNotFoundExceptions(string message) : base(message)
        {
        }
    }
}