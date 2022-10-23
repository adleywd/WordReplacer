namespace WordReplacer.Models;

public record AppSettings
{
    public string AppVersion { get; init; } = default!;
    public string LanguageStoreKey { get; init; } = default!;
    public string CookiesStorageKey { get; init; } = default!;
}