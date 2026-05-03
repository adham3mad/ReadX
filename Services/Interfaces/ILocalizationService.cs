namespace ReadX.Api.Services.Interfaces;

public interface ILocalizationService
{
    string GetMessage(string key, string? lang = null);
}
