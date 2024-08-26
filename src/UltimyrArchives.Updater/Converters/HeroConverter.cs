using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2;
using UltimyrArchives.Updater.Extensions;

namespace UltimyrArchives.Updater.Converters;

public sealed class HeroConverter(KVObject baseHero) : KVObjectConverter
{
    private readonly BaseHeroValues _baseHero = ConvertBaseHero(baseHero);

    public Hero Convert(KVObject kvHero) => new()
    {
        InternalName       = kvHero.Name,
        Id                 = kvHero.GetRequiredInt32("HeroID", CultureInfo.InvariantCulture),
        HeroOrderId        = kvHero.GetRequiredInt16("HeroOrderID", CultureInfo.InvariantCulture),
        NameAliases        = kvHero["NameAliases"].ParseArray<string>(),
        HeroGlowColor      = ParseColor(kvHero["HeroGlowColor"]),
        SimilarHeroes      = kvHero["SimilarHeroes"].ParseArray<int>(),
        Complexity         = kvHero.GetRequiredByte("Complexity", CultureInfo.InvariantCulture),
        Role               = kvHero["Role"].ParseEnumArray<Role>(),
        Rolelevels         = kvHero["Rolelevels"].ParseArray<byte>(),
        Abilities          = ParseAbilities(kvHero),
        AbilityTalentStart = kvHero["AbilityTalentStart"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.AbilityTalentStart,
        Facets             = ConvertList(kvHero.GetRequiredValue("Facets"), FacetConverter),
        // Attributes
        AttributePrimary          = kvHero.GetRequiredEnum<AttributePrimary>("AttributePrimary"),
        AttributeBaseAgility      = kvHero.GetRequiredInt16("AttributeBaseAgility", CultureInfo.InvariantCulture),
        AttributeAgilityGain      = kvHero.GetRequiredSingle("AttributeAgilityGain", CultureInfo.InvariantCulture),
        AttributeBaseStrength     = kvHero.GetRequiredInt16("AttributeBaseStrength", CultureInfo.InvariantCulture),
        AttributeStrengthGain     = kvHero.GetRequiredSingle("AttributeStrengthGain", CultureInfo.InvariantCulture),
        AttributeBaseIntelligence = kvHero.GetRequiredInt16("AttributeBaseIntelligence", CultureInfo.InvariantCulture),
        AttributeIntelligenceGain = kvHero.GetRequiredSingle("AttributeIntelligenceGain", CultureInfo.InvariantCulture),
        AttackCapabilities        = kvHero.GetRequiredEnum<AttackCapabilities>("AttackCapabilities"),
        // Everything below here typically inherits the default.
        AttackDamageMin      = kvHero["AttackDamageMin"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.AttackDamageMin,
        AttackDamageMax      = kvHero["AttackDamageMax"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.AttackDamageMax,
        AttackRate           = kvHero["AttackRate"]?.ToSingle(CultureInfo.InvariantCulture) ?? _baseHero.AttackRate,
        BaseAttackSpeed      = kvHero["BaseAttackSpeed"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.BaseAttackSpeed,
        AttackAnimationPoint = kvHero["AttackAnimationPoint"]?.ToSingle(CultureInfo.InvariantCulture) ?? _baseHero.AttackAnimationPoint,
        AttackRange          = kvHero["AttackRange"]?.ToSingle(CultureInfo.InvariantCulture) ?? _baseHero.AttackRange,
        ProjectileSpeed      = kvHero["ProjectileSpeed"]?.ToSingle(CultureInfo.InvariantCulture) ?? _baseHero.ProjectileSpeed,
        ArmorPhysical        = kvHero["ArmorPhysical"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.ArmorPhysical,
        MagicalResistance    = kvHero["MagicalResistance"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.MagicalResistance,
        MovementSpeed        = kvHero["MovementSpeed"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.MovementSpeed,
        MovementTurnRate     = kvHero["MovementTurnRate"]?.ToSingle(CultureInfo.InvariantCulture) ?? _baseHero.MovementTurnRate,
        VisionDaytimeRange   = kvHero["VisionDaytimeRange"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.VisionDaytimeRange,
        VisionNighttimeRange = kvHero["VisionNighttimeRange"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.VisionNighttimeRange,
        StatusHealth         = kvHero["StatusHealth"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.StatusHealth,
        StatusHealthRegen    = kvHero["StatusHealthRegen"]?.ToSingle(CultureInfo.InvariantCulture) ?? _baseHero.StatusHealthRegen,
        StatusMana           = kvHero["StatusMana"]?.ToInt16(CultureInfo.InvariantCulture) ?? _baseHero.StatusMana,
        StatusManaRegen      = kvHero["StatusManaRegen"]?.ToSingle(CultureInfo.InvariantCulture) ?? _baseHero.StatusManaRegen,
    };

    private static uint ParseColor(KVValue? value)
    {
        var values = value.ParseArray<uint>();
        if (values.Length == 0)
            return 0;

        uint colour = 0xFF000000; // A
        colour ^= values[0] << 16; // R
        colour ^= values[1] << 8; // G
        colour ^= values[2] << 0; // B
        return colour;
    }

    private static Dictionary<int, string> ParseAbilities(KVObject kvHero)
    {
        var abilityValues = kvHero.Where(x => Rx.AbilityKey.Match(x.Name).Success).ToArray();
        var abilities     = new Dictionary<int, string>(abilityValues.Length);
        foreach (var ability in abilityValues)
        {
            var match = Rx.AbilityKey.Match(ability.Name);
            var index = int.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture);
            abilities.Add(index, ability.Value.ToString(CultureInfo.InvariantCulture));
        }

        return abilities;
    }

    private static Facet FacetConverter(KVObject facet) => new()
    {
        InternalName            = facet.Name,
        Icon                    = facet.GetRequiredString("Icon", CultureInfo.InvariantCulture),
        Color                   = facet.GetRequiredString("Color", CultureInfo.InvariantCulture),
        GradientId              = facet["GradientId"]?.ToInt16(CultureInfo.InvariantCulture) ?? 0,
        Deprecated              = facet["Deprecated"].ToBoolFromString(),
        Abilities               = ConvertList(facet["Abilities"], FacetAbilitiesConverter),
        KeyValueOverrides       = ConvertList(facet["KeyValueOverrides"], KeyValueTupleConverter),
        AbilityIconReplacements = ConvertList(facet["AbilityIconReplacements"], KeyValueTupleConverter),
    };

    private static FacetAbility FacetAbilitiesConverter(KVObject obj) => new()
    {
        AbilityName      = obj.GetRequiredString("AbilityName", CultureInfo.InvariantCulture),
        AbilityIndex     = obj["AbilityIndex"]?.ToInt32(CultureInfo.InvariantCulture),
        AutoLevelAbility = obj["AutoLevelAbility"].ToBoolFromString(),
        ReplaceAbility   = obj["ReplaceAbility"]?.ToString(CultureInfo.InvariantCulture),
    };

    private static (string Key, string Value) KeyValueTupleConverter(KVObject o)
        => (o.Name, o.Value.ToString(CultureInfo.InvariantCulture));

    private static BaseHeroValues ConvertBaseHero(KVObject baseHero) => new()
    {
        AbilityTalentStart   = baseHero.GetRequiredInt16("AbilityTalentStart"),
        AttackDamageMin      = baseHero.GetRequiredInt16("AttackDamageMin"),
        AttackDamageMax      = baseHero.GetRequiredInt16("AttackDamageMax"),
        AttackRate           = baseHero.GetRequiredSingle("AttackRate"),
        BaseAttackSpeed      = baseHero.GetRequiredInt16("BaseAttackSpeed"),
        AttackAnimationPoint = baseHero.GetRequiredSingle("AttackAnimationPoint"),
        AttackRange          = baseHero.GetRequiredSingle("AttackRange"),
        ProjectileSpeed      = baseHero.GetRequiredSingle("ProjectileSpeed"),
        ArmorPhysical        = baseHero.GetRequiredInt16("ArmorPhysical"),
        MagicalResistance    = baseHero.GetRequiredInt16("MagicalResistance"),
        MovementSpeed        = baseHero.GetRequiredInt16("MovementSpeed"),
        MovementTurnRate     = baseHero.GetRequiredSingle("MovementTurnRate"),
        VisionDaytimeRange   = baseHero.GetRequiredInt16("VisionDaytimeRange"),
        VisionNighttimeRange = baseHero.GetRequiredInt16("VisionNighttimeRange"),
        StatusHealth         = baseHero.GetRequiredInt16("StatusHealth"),
        StatusHealthRegen    = baseHero.GetRequiredSingle("StatusHealthRegen"),
        StatusMana           = baseHero.GetRequiredInt16("StatusMana"),
        StatusManaRegen      = baseHero.GetRequiredSingle("StatusManaRegen"),
    };

    private record BaseHeroValues
    {
        public short AbilityTalentStart   { get; init; }
        public short AttackDamageMin      { get; init; }
        public short AttackDamageMax      { get; init; }
        public float AttackRate           { get; init; }
        public short BaseAttackSpeed      { get; init; }
        public float AttackAnimationPoint { get; init; }
        public float AttackRange          { get; init; }
        public float ProjectileSpeed      { get; init; }
        public short ArmorPhysical        { get; init; }
        public short MagicalResistance    { get; init; }
        public short MovementSpeed        { get; init; }
        public float MovementTurnRate     { get; init; }
        public short VisionDaytimeRange   { get; init; }
        public short VisionNighttimeRange { get; init; }
        public short StatusHealth         { get; init; }
        public float StatusHealthRegen    { get; init; }
        public short StatusMana           { get; init; }
        public float StatusManaRegen      { get; init; }
    }
}
