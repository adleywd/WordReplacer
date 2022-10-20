namespace WordReplacer.Common
{
    public record AppSettings
    {
        public string LanguageStoreKey { get; init; } = default!;
        public string AppVersion { get; init; } = default!;
    }
}