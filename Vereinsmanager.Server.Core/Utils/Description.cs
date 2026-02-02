namespace Vereinsmanager.Utils;

[AttributeUsage(AttributeTargets.Field)]
public class Description(string text) : Attribute
{
    public string Text { get; } = text;
}