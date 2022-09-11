using System.Runtime.InteropServices;
using WordReplacer.Enums;

namespace WordReplacer.Dto;

public class DocumentParamsDto
{
    public string? Text { get; set; } = string.Empty;
    public InputType Type { get; set; }
    public bool IsTextEmpty => string.IsNullOrEmpty(Text);
}