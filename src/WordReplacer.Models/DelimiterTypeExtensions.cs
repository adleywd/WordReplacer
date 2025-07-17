namespace WordReplacer.Models
{
    public static class DelimiterTypeExtensions
    {
        public static string GetDelimiterString(this DelimiterType type, string? custom = null)
        {
            return type switch
            {
                DelimiterType.None => string.Empty,
                DelimiterType.NewLine => "\n",
                DelimiterType.Semicolon => ";",
                DelimiterType.Colon => ":",
                DelimiterType.Period => ".",
                DelimiterType.Pipe => "|",
                DelimiterType.Comma => ",",
                DelimiterType.Tab => "\t",
                DelimiterType.Custom => custom ?? string.Empty,
                _ => string.Empty
            };
        }
    }
}
