#nullable enable
namespace Vereinsmanager.Services.Models;

[AttributeUsage(AttributeTargets.Field)]
public class PermissionDescription(PermissionGroup group, PermissionCategory type) : Attribute
{
    public PermissionGroup Group { get; } = group;
    public PermissionCategory Type { get; } = type;
}
