using WordReplacer.WebApp.Enums;

namespace WordReplacer.WebApp.Utilities;

public static class InputTypeExtensions
{
    /// <summary>
    /// It checks if the input type is a text area.
    /// </summary>
    /// <param name="InputType">The input type to check.</param>
    public static bool IsTextArea(this InputType inputType)
    {
        return inputType == InputType.List;
    }
    
    /// <summary>
    /// It checks if the input type is a text field.
    /// </summary>
    /// <param name="InputType">The input type to check.</param>
    public static bool IsTextField(this InputType inputType)
    {
        return inputType == InputType.Text;
    }
}