using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2;
using UltimyrArchives.Updater.Extensions;

namespace UltimyrArchives.Updater.Converters;

public sealed class HeroConverter(KVObject baseHero) : KVObjectConverter
{
    private readonly BaseHeroValues _baseHero = ConvertBaseHero(baseHero);

    public Hero Convert(string name, KVObject kvHero) => new()
    {
        InternalName       = name,
        Id                 = kvHero["HeroID"].ToInt32(CultureInfo.InvariantCulture),
        HeroOrderId        = kvHero["HeroOrderID"].ToInt16(CultureInfo.InvariantCulture),
        NameAliases        = kvHero.GetValueOrDefault("NameAliases").ParseArray<string>(),
        HeroGlowColor      = ParseColor(kvHero.GetValueOrDefault("HeroGlowColor")),
        SimilarHeroes      = kvHero.GetValueOrDefault("SimilarHeroes").ParseArray<int>(),
        Complexity         = kvHero["Complexity"].ToByte(CultureInfo.InvariantCulture),
        Role               = kvHero["Role"].ParseEnumArray<Role>(),
        Rolelevels         = kvHero["Rolelevels"].ParseArray<byte>(),
        Abilities          = ParseAbilities(kvHero),
        AbilityTalentStart = kvHero.GetInt16OrDefault("AbilityTalentStart", _baseHero.AbilityTalentStart, CultureInfo.InvariantCulture),
        // Attributes
        AttributePrimary          = kvHero["AttributePrimary"].ToEnum<AttributePrimary>(),
        AttributeBaseAgility      = kvHero["AttributeBaseAgility"].ToInt16(CultureInfo.InvariantCulture),
        AttributeAgilityGain      = kvHero["AttributeAgilityGain"].ToSingle(CultureInfo.InvariantCulture),
        AttributeBaseStrength     = kvHero["AttributeBaseStrength"].ToInt16(CultureInfo.InvariantCulture),
        AttributeStrengthGain     = kvHero["AttributeStrengthGain"].ToSingle(CultureInfo.InvariantCulture),
        AttributeBaseIntelligence = kvHero["AttributeBaseIntelligence"].ToInt16(CultureInfo.InvariantCulture),
        AttributeIntelligenceGain = kvHero["AttributeIntelligenceGain"].ToSingle(CultureInfo.InvariantCulture),
        AttackCapabilities        = kvHero["AttackCapabilities"].ToEnum<AttackCapabilities>(),
        // Everything below here typically inherits the default.
        AttackDamageMin      = kvHero.GetInt16OrDefault("AttackDamageMin", _baseHero.AttackDamageMin, CultureInfo.InvariantCulture),
        AttackDamageMax      = kvHero.GetInt16OrDefault("AttackDamageMax", _baseHero.AttackDamageMax, CultureInfo.InvariantCulture),
        AttackRate           = kvHero.GetSingleOrDefault("AttackRate", _baseHero.AttackRate, CultureInfo.InvariantCulture),
        BaseAttackSpeed      = kvHero.GetInt16OrDefault("BaseAttackSpeed", _baseHero.BaseAttackSpeed, CultureInfo.InvariantCulture),
        AttackAnimationPoint = kvHero.GetSingleOrDefault("AttackAnimationPoint", _baseHero.AttackAnimationPoint, CultureInfo.InvariantCulture),
        AttackRange          = kvHero.GetSingleOrDefault("AttackRange", _baseHero.AttackRange, CultureInfo.InvariantCulture),
        ProjectileSpeed      = kvHero.GetSingleOrDefault("ProjectileSpeed", _baseHero.ProjectileSpeed, CultureInfo.InvariantCulture),
        ArmorPhysical        = kvHero.GetInt16OrDefault("ArmorPhysical", _baseHero.ArmorPhysical, CultureInfo.InvariantCulture),
        MagicalResistance    = kvHero.GetInt16OrDefault("MagicalResistance", _baseHero.MagicalResistance, CultureInfo.InvariantCulture),
        MovementSpeed        = kvHero.GetInt16OrDefault("MovementSpeed", _baseHero.MovementSpeed, CultureInfo.InvariantCulture),
        MovementTurnRate     = kvHero.GetSingleOrDefault("MovementTurnRate", _baseHero.MovementTurnRate, CultureInfo.InvariantCulture),
        VisionDaytimeRange   = kvHero.GetInt16OrDefault("VisionDaytimeRange", _baseHero.VisionDaytimeRange, CultureInfo.InvariantCulture),
        VisionNighttimeRange = kvHero.GetInt16OrDefault("VisionNighttimeRange", _baseHero.VisionNighttimeRange, CultureInfo.InvariantCulture),
        StatusHealth         = kvHero.GetInt16OrDefault("StatusHealth", _baseHero.StatusHealth, CultureInfo.InvariantCulture),
        StatusHealthRegen    = kvHero.GetSingleOrDefault("StatusHealthRegen", _baseHero.StatusHealthRegen, CultureInfo.InvariantCulture),
        StatusMana           = kvHero.GetInt16OrDefault("StatusMana", _baseHero.StatusMana, CultureInfo.InvariantCulture),
        StatusManaRegen      = kvHero.GetSingleOrDefault("StatusManaRegen", _baseHero.StatusManaRegen, CultureInfo.InvariantCulture),
    };

    private static uint ParseColor(KVObject? value)
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
        var abilityValues = kvHero.Where(x => Rx.AbilityKey.IsMatch(x.Key)).ToArray();
        var abilities     = new Dictionary<int, string>(abilityValues.Length);
        foreach (var ability in abilityValues)
        {
            var match = Rx.AbilityKey.Match(ability.Key);
            var index = int.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture);
            abilities.Add(index, ability.Value.ToString(CultureInfo.InvariantCulture));
        }

        return abilities;
    }

    private static BaseHeroValues ConvertBaseHero(KVObject baseHero) => new()
    {
        AbilityTalentStart   = baseHero["AbilityTalentStart"].ToInt16(),
        AttackDamageMin      = baseHero["AttackDamageMin"].ToInt16(),
        AttackDamageMax      = baseHero["AttackDamageMax"].ToInt16(),
        AttackRate           = baseHero["AttackRate"].ToSingle(),
        BaseAttackSpeed      = baseHero["BaseAttackSpeed"].ToInt16(),
        AttackAnimationPoint = baseHero["AttackAnimationPoint"].ToSingle(),
        AttackRange          = baseHero["AttackRange"].ToSingle(),
        ProjectileSpeed      = baseHero["ProjectileSpeed"].ToSingle(),
        ArmorPhysical        = baseHero["ArmorPhysical"].ToInt16(),
        MagicalResistance    = baseHero["MagicalResistance"].ToInt16(),
        MovementSpeed        = baseHero["MovementSpeed"].ToInt16(),
        MovementTurnRate     = baseHero["MovementTurnRate"].ToSingle(),
        VisionDaytimeRange   = baseHero["VisionDaytimeRange"].ToInt16(),
        VisionNighttimeRange = baseHero["VisionNighttimeRange"].ToInt16(),
        StatusHealth         = baseHero["StatusHealth"].ToInt16(),
        StatusHealthRegen    = baseHero["StatusHealthRegen"].ToSingle(),
        StatusMana           = baseHero["StatusMana"].ToInt16(),
        StatusManaRegen      = baseHero["StatusManaRegen"].ToSingle(),
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
