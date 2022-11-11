using System.Net;

namespace Schedule.Domain.Responses;

public class BaseResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
}