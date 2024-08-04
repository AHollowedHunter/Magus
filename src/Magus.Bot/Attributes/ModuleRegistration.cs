namespace Magus.Bot.Attributes;

/// <summary>
/// Use to annotate classes implementing InteractionModuleBase
/// 
/// NOTE: A module is not the same as a command group, a module may have individual top-level commands.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ModuleRegistration : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="location">To what group of guilds should a command be registered. See <see cref="Attributes.Location"/></param>
    /// <param name="isEnabled">Use to determine if a module is registered or not. Defaults to <c>true</c></param>
    public ModuleRegistration(Location location, bool isEnabled = true)
    {
        Location = location;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Indicates where the module should be registered.
    /// </summary>
    public Location Location { get; }

    /// <summary>
    /// Set if the module is enabled. Defaults to true to enable annotated modules.
    /// 
    /// If false, the module is not loaded and registered to its location, and will be removed if remove missing is true.
    /// </summary>
    public bool IsEnabled { get; }
}

public enum Location
{
    /// <summary>
    /// Globally registered
    /// </summary>
    GLOBAL = 0,

    /// <summary>
    /// Only guilds that allow testing.
    /// 
    /// [FUTURE USE]
    /// </summary>
    TESTING = 1,

    /// <summary>
    /// Register only managed guilds, to restrict access to only bot management.
    /// </summary>
    MANAGEMENT = 2,

    /// <summary>
    /// Only guilds with premium sub.
    /// 
    /// [POTENTIAL 🤣 FUTURE USE]
    /// </summary>
    PREMIUM = 3,
}
