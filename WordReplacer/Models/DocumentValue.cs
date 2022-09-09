using WordReplacer.Enums;

namespace WordReplacer.Models;

public class DocumentValue
{
    public string HtmlId { get; set; } = string.Empty;
    public bool IsOldValue { get; set; }
    public bool IsNewValue => !IsOldValue;
    public string Label { get; set; } = string.Empty;
    public string HelperText { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public InputType Type { get; set; }
    public bool RepeatReplaceForEachLine { get; set; }
}