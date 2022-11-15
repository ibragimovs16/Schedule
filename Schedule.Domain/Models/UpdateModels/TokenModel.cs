using Newtonsoft.Json;

namespace Schedule.Domain.Models.UpdateModels;

public class TokenModel
{
    public string? AccessToken { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? RefreshToken { get; set; }
}