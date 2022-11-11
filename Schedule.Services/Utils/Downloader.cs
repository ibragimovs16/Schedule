namespace Schedule.Services.Utils;

public static class Downloader
{
    public static async Task<string> DownloadAsync(string url, string? fileName = null)
    {
        using var client = new HttpClient();
        await using var stream = await client.GetStreamAsync(url);
        
        var path = AppDomain.CurrentDomain.BaseDirectory + 
                   (fileName ?? Guid.NewGuid().ToString().Replace("-", ""));

        await using var fileStream = new FileStream(path, FileMode.OpenOrCreate);
        await stream.CopyToAsync(fileStream);

        await fileStream.DisposeAsync();
        await stream.DisposeAsync();
        client.Dispose();
        
        return path;
    }
}