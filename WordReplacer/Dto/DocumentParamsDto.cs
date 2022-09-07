using WordReplacer.Enums;

namespace WordReplacer.Dto;

public class DocumentParamsDto
{
    public string Label { get; set; } = string.Empty;
    public InputType Type { get; set; }
}