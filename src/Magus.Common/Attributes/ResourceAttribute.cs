namespace Magus.Common.Attributes;


[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ResourceAttribute : Attribute
{
    /// <summary>
    /// Mark a property with the resource name
    /// </summary>
    public ResourceAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// The resource name
    /// </summary>
    public string Name { get; }
}
