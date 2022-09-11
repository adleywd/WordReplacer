using WordReplacer.Enums;

namespace WordReplacer.Utilities;

public static class InputTypeExtensions
{
    public static bool IsTextArea(this InputType inputType)
    {
        return inputType == InputType.List;
    }
    
    public static bool IsTextField(this InputType inputType)
    {
        return inputType == InputType.Text;
    }
}