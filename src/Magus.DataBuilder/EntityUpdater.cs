using Magus.Common.Options;
using Magus.Data;
using Magus.Data.Enums;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.Magus;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using ValveKeyValue;
using static Magus.Data.Models.Dota.Ability;
using static Magus.Data.Models.Dota.BaseSpell;

namespace Magus.DataBuilder;

/// <summary>
/// This class is held together with virtual duct tape and hope.
/// 
/// Maybe once day it will be rewritten ¯\_(ツ)_/¯
/// </summary>
public class EntityUpdater
{
    private readonly IAsyncDataService _db;
    private readonly LocalisationOptions _localisationOptions;
    private readonly ILogger<EntityUpdater> _logger;
    private readonly KVSerializer _kvSerializer;

    private readonly Dictionary<string, int> _abilityIds = [];
    private readonly Dictionary<string, int> _ItemIds = [];

    private readonly Dictionary<(string Language, string Key), string> _abilityValues;
    private readonly Dictionary<(string Language, string Key), string> _dotaValues;
    private readonly Dictionary<(string Language, string Key), string> _heroLoreValues;
    private readonly List<Ability> _abilities;
    private readonly List<Ability> _heroAbilities;
    private readonly List<Talent> _talents;
    private readonly List<Hero> _heroes;
    private readonly List<Item> _items;
    private Dictionary<string, byte> _neutralItemTiers;
    private Hero _baseHero;

    private static readonly string ValueSeparator = "\u00A0/\u00A0";

    private static readonly Regex NameGender = new("#\\|(\\p{L}+)\\|#");

    public EntityUpdater(IAsyncDataService db, IOptions<LocalisationOptions> localisationOptions, ILogger<EntityUpdater> logger)
    {
        _db = db;
        _localisationOptions = localisationOptions.Value;
        _logger = logger;

        _kvSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        _abilityValues = new();
        _dotaValues = new();
        _heroLoreValues = new();
        _abilities = new();
        _heroAbilities = new();
        _talents = new();
        _heroes = new();
        _items = new();
        _neutralItemTiers = new();
        _baseHero = new();
    }

    public async Task Update()
    {
        _logger.LogInformation("Starting Entity Update");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await SetEntityValues();
        await SetEntities();

        await CreateAndStoreEntityLocalisations();

        await StoreEntityEmbeds();

        stopwatch.Stop();
        var timeTaken = stopwatch.Elapsed.TotalSeconds;
        _logger.LogInformation("Finished Entity Update");
        _logger.LogInformation("Time Taken: {0:0.#}s", timeTaken);
    }

    private async Task SetEntityValues()
    {
        _logger.LogInformation("Setting Entity values");

        _abilityValues.Clear();
        _dotaValues.Clear();
        _heroLoreValues.Clear();

        foreach (var language in _localisationOptions.SourceLocaleMappings)
        {
            _logger.LogDebug("Processing values for {0}", language.Key);

            var abilities = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.Localization.GetAbilities(language.Key));
            var dota      = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.Localization.GetDota(language.Key));
            var heroLore  = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.Localization.GetHeroLore(language.Key));

            foreach (var note in abilities.Children.First(x => x.Name == "Tokens"))
                _abilityValues.TryAdd((language.Key, note.Name.ToLower()), note.Value.ToString() ?? "");

            foreach (var note in dota.Children.First(x => x.Name == "Tokens"))
                _dotaValues.TryAdd((language.Key, note.Name.ToLower()), CleanSimple(note.Value.ToString() ?? ""));

            foreach (var note in heroLore.Children.First(x => x.Name == "Tokens"))
                _heroLoreValues.TryAdd((language.Key, note.Name.ToLower()), CleanSimple(note.Value.ToString() ?? ""));
        }
        _logger.LogInformation("Finished setting Entity values");
    }

    private async Task SetEntities()
    {
        _logger.LogInformation("Setting Entities");

        var abilityIds = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.NpcAbilityIds);
        _abilityIds.Clear();
        foreach (var ability in abilityIds.Children.Single(x => x.Name == "UnitAbilities").Children.Single(x => x.Name == "Locked"))
        {
            _abilityIds.Add(ability.Name, ability.ParseValue<int>());
        }
        _ItemIds.Clear();
        foreach (var item in abilityIds.Children.Single(x => x.Name == "ItemAbilities").Children.Single(x => x.Name == "Locked"))
        {
            _ItemIds.Add(item.Name, item.ParseValue<int>());
        }

        // bodge this
        var mixedAbilities = (await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.NpcAbilities, false)).Children.ToList();
        var heroAbilityFiles = Directory.GetFiles(Dota2GameFiles.BasePath + "/scripts/npc/heroes");
        foreach (var file in heroAbilityFiles)
        {
            mixedAbilities.AddRange(await _kvSerializer.GetKVObjectFromLocalUri(file, false));
        }
        //

        var heroes         = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.NpcHeroes);
        var items          = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.Items);
        var neutralItems   = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.NeutralItems);

        var talentRegex    = new Regex("special_bonus_\\w+");
        var mainAbilities  = new List<KVObject>();

        _abilities.Clear();
        _talents.Clear();
        _heroes.Clear();
        _items.Clear();

        foreach (var ability in mixedAbilities.Where(x => x.Name != "Version"))
        {
            if (!ability.Children.Any())
                continue;
            if (!talentRegex.IsMatch(ability.Name))
            {
                mainAbilities.Add(ability);
                continue;
            }

            _logger.LogDebug("Processing talent {0}", ability.Name);
            foreach (var language in _localisationOptions.SourceLocaleMappings.Keys)
            {
                try
                {
                    _talents.Add(CreateTalent(language, ability));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating talent");
                }
            }
        }

        foreach (var ability in mainAbilities)
        {
            _logger.LogDebug("Processing ability {0}", ability.Name);
            foreach (var language in _localisationOptions.SourceLocaleMappings.Keys)
            {
                try
                {
                    _abilities.Add(CreateAbility(language, ability));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating ability");
                }
            }
        }

        _baseHero = CreateHero("", heroes.Children.First(x => x.Name == "npc_dota_hero_base"));

        foreach (var hero in heroes.Children.Where(x => x.Name != "Version" && x.Name != "npc_dota_hero_base" && x.Name != "npc_dota_hero_target_dummy"))
        {
            _logger.LogDebug("Processing hero {0}", hero.Name);
            foreach (var language in _localisationOptions.SourceLocaleMappings.Keys)
            {
                try
                {
                    _heroes.Add(CreateHero(language, hero));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating hero");
                }
            }
        }

        _neutralItemTiers = GetNeutralItemTiers(neutralItems);

        foreach (var item in items.Children.Where(x => x.Name != "Version"))
        {
            // Ignore items that are obsolete, not in neutralItems, etc.
            if (item.ParseChildValue<bool>("IsObsolete"))
                continue;
            if (item.ParseChildValue<bool>("ItemIsNeutralDrop") && !_neutralItemTiers.ContainsKey(item.Name))
                continue;

            _logger.LogDebug("Processing item {0}", item.Name);
            foreach (var language in _localisationOptions.SourceLocaleMappings.Keys)
            {
                try
                {
                    _items.Add(CreateItem(language, item));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating item");
                }
            }
        }

        FormatEntities();

        _logger.LogInformation("Finished setting entities");
    }

    private void FormatEntities()
    {
        _logger.LogInformation("Formatting entity values");
        foreach (var talent in _talents)
        {
            try
            {
                FormatTalent(talent);
                //if ((Regex.IsMatch(talent.Description, @"[{}]") || Regex.IsMatch(talent.Note, @"[{}]")) && _heroes.Any(x => x.Talents.Any(x => x == talent)))
                //    _logger.LogWarning("{0} description/note formatting wrong: {1} / {2}", talent.InternalName, talent.Description, talent.Note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting talent {0} in {1}", talent.InternalName, talent.Language);
            }
        }

        // Only format used abilities, in respective lists
        foreach (var ability in _heroAbilities)
        {
            try
            {
                FormatAbility(ability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting ability {0,-32} in {1}", ability.InternalName, ability.Language);
            }
        }

        foreach (var item in _items)
        {
            try
            {
                FormatItem(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting item {0,-32} in {1}", item.InternalName, item.Language);
            }
        }

        _logger.LogInformation("Completed formatting entity values");
    }

    private Talent CreateTalent(string language, KVObject kvTalent)
    {
        var talent = new Talent();

        talent.Id = _abilityIds[kvTalent.Name];
        talent.InternalName = kvTalent.Name;
        talent.Language = language;
        talent.AbilityType = kvTalent.ParseChildEnum<AbilityType>("AbilityType");
        talent.AbilityBehavior = kvTalent.ParseChildEnum<AbilityBehavior>("AbilityBehavior");

        talent.TalentValues = GetTalentValues(kvTalent);
        talent.Description = GetAbilityValue(language, talent.InternalName);
        talent.Note = GetAbilityValue(language, talent.InternalName, "Description");

        _logger.LogTrace("Processed {0,8} {1,-64} in {2}", "talent", talent.InternalName, language);
        return talent;
    }

    private void FormatTalent(Talent talent)
    {
        var valueKeyRegex      = new Regex(@"(?<=[+-]?{s:)\w+(?=})");
        var bonusValueKeyRegex = new Regex(@"(?<=[+-]?{s:bonus_)\w+(?=})");
        var doubleSymbols      = new Regex(@"[%+-](?=[%+-][^%+-])");

        var descriptionValueKeys = valueKeyRegex.Matches(talent.Description);
        foreach (var valueKey in descriptionValueKeys.AsEnumerable())
        {
            var value = talent.TalentValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Value;
            if (value != null)
                talent.Description = Regex.Replace(talent.Description, @$"(?<=[+-]?){{s:{valueKey.Value}}}", value);
        }
        var descriptionBonusValueKeys = bonusValueKeyRegex.Matches(talent.Description);
        foreach (var bonusValueKey in descriptionBonusValueKeys.AsEnumerable())
        {
            var key = bonusValueKey.Value;
            var ability = _abilities.FirstOrDefault(x => x.AbilityValues.Any(y => y.LinkedSpecialBonus == talent.InternalName && y.Name == key));
            var abilityValue = ability?.AbilityValues.First(x => x.Name == key);
            if (abilityValue != null)
            {
                talent.Description = Regex.Replace(talent.Description, @$"(?<=[+-]?){{s:bonus_{key}}}", abilityValue.SpecialBonusValue?.ToString() ?? "");
            }
        }
        var noteValueKeys = valueKeyRegex.Matches(talent.Note);
        foreach (var valueKey in noteValueKeys.AsEnumerable())
        {
            var value = talent.TalentValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Value;
            if (value != null)
                talent.Note = Regex.Replace(talent.Note, @$"(?<=[+-]?){{s:{valueKey.Value}}}", value);
        }
        var noteBonusValueKeys = bonusValueKeyRegex.Matches(talent.Note);
        foreach (var bonusValueKey in noteBonusValueKeys.AsEnumerable())
        {
            var key = bonusValueKey.Value;
            var ability = _abilities.FirstOrDefault(x => x.AbilityValues.Any(y => y.LinkedSpecialBonus == talent.InternalName && y.Name == key));
            var abilityValue = ability?.AbilityValues.First(x => x.Name == key);
            if (abilityValue != null)
            {
                talent.Note = Regex.Replace(talent.Note, @$"(?<=[+-]?){{s:bonus_{key}}}", abilityValue.SpecialBonusValue?.ToString() ?? "");
            }
        }
        talent.Description = doubleSymbols.Replace(talent.Description, string.Empty);
        talent.Note = doubleSymbols.Replace(talent.Note, string.Empty);
    }

    private IEnumerable<Talent.TalentValue> GetTalentValues(KVObject kvTalent)
    {
        var talentValues = new List<Talent.TalentValue>();

        var kvAbilitySpecial = kvTalent.Children.FirstOrDefault(x => x.Name == "AbilitySpecial");
        var kvAbilityValues = kvTalent.Children.FirstOrDefault(x => x.Name == "AbilityValues" || x.Name == "AbilitySpecial");
        if (kvAbilityValues != null)
        {
            foreach (var item in kvAbilityValues.Children)
            {
                if (item.Value.ValueType == KVValueType.Collection)
                {
                    var value = item.First(x => x.Name != "var_type" && x.Name != "ad_linked_abilities");
                    talentValues.Add(new()
                    {
                        Name = value.Name,
                        Value = value.ParseValue<string>() ?? "",
                        AdLinkedAbilities = item.ParseChildValue<string>("ad_linked_abilities"),
                    });
                }
                else
                {
                    talentValues.Add(new()
                    {
                        Name = item.Name,
                        Value = item.ParseValue<string>() ?? "",
                    });
                }
            }
        }
        return talentValues;
    }

    private Ability CreateAbility(string language, KVObject kvAbility)
    {
        var ability = new Ability();

        ability.Id = _abilityIds[kvAbility.Name];
        ability.InternalName = kvAbility.Name;
        ability.Language = language;
        ability.Name = GetAbilityValue(language, ability.InternalName)!;
        ability.Description = GetAbilityValue(language, ability.InternalName, "Description");
        ability.Lore = GetAbilityValue(language, ability.InternalName, "Lore");
        ability.Notes = GetAbilityNoteValues(language, ability.InternalName);

        ability.AbilityType = kvAbility.ParseChildEnum<AbilityType>("AbilityType");
        ability.AbilityBehavior = kvAbility.ParseChildEnum<AbilityBehavior>("AbilityBehavior");
        ability.AbilityUnitTargetTeam = kvAbility.ParseChildEnum<AbilityUnitTargetTeam>("AbilityUnitTargetTeam");
        ability.AbilityUnitTargetType = kvAbility.ParseChildEnum<AbilityUnitTargetType>("AbilityUnitTargetType");
        ability.AbilityUnitDamageType = kvAbility.ParseChildEnum<AbilityUnitDamageType>("AbilityUnitDamageType");
        ability.SpellImmunityType = kvAbility.ParseChildEnum<SpellImmunityType>("SpellImmunityType");
        ability.SpellDispellableType = kvAbility.ParseChildEnum<SpellDispellableType>("SpellDispellableType");

        ability.AbilityCastRange = kvAbility.ParseChildValueList<float>("AbilityCastRange");
        ability.AbilityCastPoint = kvAbility.ParseChildValueList<float>("AbilityCastPoint");
        ability.AbilityChannelTime = kvAbility.ParseChildValueList<float>("AbilityChannelTime");
        ability.AbilityCharges = kvAbility.ParseChildValueList<float>("AbilityCharges");
        ability.AbilityChargeRestoreTime = kvAbility.ParseChildValueList<float>("AbilityChargeRestoreTime");
        ability.AbilityCooldown = kvAbility.ParseChildValueList<float>("AbilityCooldown");
        ability.AbilityDuration = kvAbility.ParseChildValueList<float>("AbilityDuration");
        ability.AbilityDamage = kvAbility.ParseChildValueList<float>("AbilityDamage");
        ability.AbilityManaCost = kvAbility.ParseChildValueList<float>("AbilityManaCost");
        ability.AbilityHealthCost = kvAbility.ParseChildValueList<float>("AbilityHealthCost");

        ability.AbilityIsGrantedByScepter = kvAbility.ParseChildValue<bool>("IsGrantedByScepter");
        ability.AbilityIsGrantedByShard = kvAbility.ParseChildValue<bool>("IsGrantedByShard");
        ability.AbilityHasScepter = kvAbility.ParseChildValue<bool>("HasScepterUpgrade");
        ability.AbilityHasShard = kvAbility.ParseChildValue<bool>("HasShardUpgrade");
        ability.ScepterDescription = GetAbilityValue(language, ability.InternalName, "scepter_description");
        ability.ShardDescription = GetAbilityValue(language, ability.InternalName, "shard_description");

        ability.AbilityValues = GetAbilityValues(language, kvAbility);
        ability.DisplayedValues = GetDisplayedValues(language, kvAbility);
        ability.ScepterValues = GetUpgradeValue(language, kvAbility);
        ability.ShardValues = GetUpgradeValue(language, kvAbility, "Shard");

        _logger.LogTrace("Processed {0,7} {1,-64} in {2}\"", "ability", ability.InternalName, language);
        return ability;
    }

    private IList<AbilityValue> GetAbilityValues(string language, KVObject kvAbility)
    {
        var abilityValues = new List<AbilityValue>();

        var upgradeRegex = new Regex(@"(?i)(shard|scepter)_\w+|\w+(shard|scepter)");
        var bonusRegex   = new Regex(@"(?i)LinkedSpecialBonus|ad_linked_abilities|special_bonus_\w+");
        // has to catch non-values, such as text refs...
        /*
         * dynamic_value - true: added patch 7.37
         */
        var nonValueName = new Regex(@"(?i)special_bonus_\w+|var_type|ad_linked_abilities|LinkedSpecialBonus|RequiresScepter|RequiresShard|\w+[^_]Tooltip|RequiresFacet|dynamic_value"); 

        var kvAbilityValues = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityValues" || x.Name == "AbilitySpecial");
        if (kvAbilityValues != null)
        {
            foreach (var kvAbilityValue in kvAbilityValues.Children)
            {
                if (upgradeRegex.IsMatch(kvAbilityValue.Name) || kvAbilityValue.ParseChildValue<bool>("RequiresScepter") || kvAbilityValue.ParseChildValue<bool>("RequiresShard"))
                    continue;

                AbilityValue abilityValue;
                if (kvAbilityValue.Value.ValueType == KVValueType.Collection)
                {

                    var valueName   = kvAbilityValue.Name;
                    // .. and this assumes there is only 1 value in the list? correct?
                    // should 'nonValueName' check for 'value'? or does this need to be a specific list of values to exclude?
                    // should instead the value be try-parsed and handled if not?
                    var valueObject = kvAbilityValue.FirstOrDefault(x => !nonValueName.IsMatch(x.Name));
                    if (valueObject == null)
                    {
                        _logger.LogDebug("Ability {0} with AbilityValue {1} is upgrade value only", kvAbility.Name, valueName);
                        continue;
                    }

                    var values      = valueObject.Value.ToString() != "FIELD_INTEGER" ? valueObject.ParseList<float>() : Array.Empty<float>();
                    var linkedBonus = kvAbilityValue.FirstOrDefault(x => bonusRegex.IsMatch(x.Name));
                    if (Regex.IsMatch(kvAbilityValue.Name, @"\d+"))
                        valueName = valueObject.Name;

                    abilityValue = new()
                    {
                        Name = valueName,
                        Values = values,
                        LinkedSpecialBonus = linkedBonus?.Name,
                        SpecialBonusValue = linkedBonus?.ParseValue<string>(),
                        Description = GetAbilityValue(language, kvAbility.Name, valueName),
                    };

                }
                else
                {
                    abilityValue = new()
                    {
                        Name = kvAbilityValue.Name,
                        Values = kvAbilityValue.ParseList<float>(true),
                        Description = GetAbilityValue(language, kvAbility.Name, kvAbilityValue.Name),
                    };
                }
                abilityValues.Add(abilityValue);
            }
        }
        return abilityValues;
    }

    private IDictionary<string, string> GetDisplayedValues(string language, KVObject kvAbility)
    {
        var displayValues    = new Dictionary<string,string>();
        var abilityRegex     = new Regex($@"(?i)DOTA_Tooltip_ability_{kvAbility.Name}_");
        var otherValuesRegex = new Regex(@"(?i)\w+(Note\d*|Lore|Description|shard|scepter|abilitydraft_note)");

        var values = _abilityValues.Where(x => x.Key.Language == language && abilityRegex.IsMatch(x.Key.Key) && !otherValuesRegex.IsMatch(x.Key.Key));
        foreach (var value in values)
        {
            displayValues.Add(abilityRegex.Replace(value.Key.Key, string.Empty), value.Value);
        }
        return displayValues;
    }

    private IDictionary<string, string> GetItemDisplayedValues(string language, KVObject kvItem)
    {
        var displayValues    = new Dictionary<string,string>();
        var abilityRegex     = new Regex($@"(?i)DOTA_Tooltip_ability_{kvItem.Name}_(?=[^\d])");
        var otherValuesRegex = new Regex(@"(?i)\w+(Note\d*|Lore|Description|shard|scepter|abilitydraft_note)");

        var values = _abilityValues.Where(x => x.Key.Language == language && abilityRegex.IsMatch(x.Key.Key) && !otherValuesRegex.IsMatch(x.Key.Key));
        foreach (var value in values)
        {
            displayValues.Add(abilityRegex.Replace(value.Key.Key, string.Empty), value.Value);
        }
        return displayValues;
    }

    private IList<UpgradeValues> GetUpgradeValue(string language, KVObject kvAbility, string type = "Scepter")
    {
        var abilityValues = new List<UpgradeValues>();

        var upgradeRegex = new Regex(@$"(?i)({type})_\w+|\w+({type})");
        var nonValueName = new Regex(@$"(?i)special_bonus_(?:(?!{type}))\w+|var_type|ad_linked_abilities|LinkedSpecialBonus|RequiresScepter|RequiresShard|\w+[^_]Tooltip");

        var kvAbilityValues = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityValues" || x.Name == "AbilitySpecial");
        if (kvAbilityValues != null)
        {
            foreach (var kvAbilityValue in kvAbilityValues.Children)
            {
                if (!(upgradeRegex.IsMatch(kvAbilityValue.Name)
                      || kvAbilityValue.Any(x => x.Name.Equals($"special_bonus_{type}", StringComparison.InvariantCultureIgnoreCase))
                      || kvAbilityValue.Children.Any(x => x.Name.StartsWith(type, StringComparison.InvariantCultureIgnoreCase))
                      || kvAbilityValue.ParseChildValue<bool>($"Requires{type}")))
                    continue;

                UpgradeValues abilityValue;
                if (kvAbilityValue.Value.ValueType == KVValueType.Collection)
                {
                    var valueName   = kvAbilityValue.Name;
                    var valueObject = kvAbilityValue.FirstOrDefault(x => !nonValueName.IsMatch(x.Name));
                    if (valueObject == null)
                    {
                        _logger.LogDebug("Ability {0} with {1}Value {2} is upgrade value only", kvAbility.Name, type, valueName);
                        continue;
                    }

                    var values = kvAbilityValue.ParseChildValueList<float>($"special_bonus_{type.ToLower()}", true);
                    if (kvAbilityValue.ParseChildValue<string>($"special_bonus_{type.ToLower()}")?.Contains("%") ?? false)
                    {
                        var mainValues = valueObject.ParseList<float>() ?? Array.Empty<float>();
                        for (var i = 0; i < mainValues.Count; i++)
                        {
                            var percentage = values.Count == 1 ? values.First() : values[i];
                            mainValues[i] = mainValues[i] + (mainValues[i] * (percentage / 100));
                        }
                        values = mainValues;
                    }
                    if (values == null || values.Count() == 0)
                        values = valueObject.Value.ToString() != "FIELD_INTEGER" ? valueObject.ParseList<float>() : Array.Empty<float>();

                    if (Regex.IsMatch(kvAbilityValue.Name, @"\d+"))
                        valueName = valueObject.Name;

                    abilityValue = new()
                    {
                        Name = valueName,
                        Values = values,
                        Description = GetAbilityValue(language, kvAbility.Name, valueName),
                    };

                }
                else
                {
                    abilityValue = new()
                    {
                        Name = kvAbilityValue.Name,
                        Values = kvAbilityValue.ParseList<float>(),
                        Description = GetAbilityValue(language, kvAbility.Name, kvAbilityValue.Name),
                    };
                }
                abilityValues.Add(abilityValue);
            }
        }
        return abilityValues;
    }

    private void FormatAbility(Ability ability)
    {
        var valueKeyRegex      = new Regex(@"(?<=%)\w+(?=%)");
        var bonusValueKeyRegex = new Regex(@"%\w+%");
        var escapedPercentage  = new Regex(@"%%(?=[^%])");

        if (ability.Description == null)
            return;

        var descriptionValueKeys = valueKeyRegex.Matches(ability.Description);
        foreach (var valueKey in descriptionValueKeys.AsEnumerable())
        {
            var value = ability.AbilityValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Values;
            value ??= GetAbilityProperty(valueKey.Value, ability);
            if (value != null)
            {
                var formattedValue =  Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct()));
                if (Regex.IsMatch(ability.Description, String.Format(@"(?<=%?){0}(?=%%%)", valueKey.Value)))
                {
                    formattedValue = Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct().Select(x => x.ToString() + "%")));
                }
                ability.Description = Regex.Replace(ability.Description,
                                                    @$"%{valueKey.Value}%",
                                                    Discord.Format.Bold(string.Join(ValueSeparator, formattedValue))); // Use distinct as some all duplicates
            }
        }
        ability.Description = escapedPercentage.Replace(ability.Description, "");
        ability.Description = CleanSimple(ability.Description);

        foreach (var value in ability.DisplayedValues)
        {
            var postFix = value.Value.StartsWith('%') ? "%" : string.Empty;
            ability.DisplayedValues[value.Key] = ability.DisplayedValues[value.Key].Trim('%');
            var values = ability.AbilityValues.FirstOrDefault(x => x.Name.Equals(value.Key, StringComparison.InvariantCultureIgnoreCase))?.Values.Distinct().Select(x => x.ToString() + postFix);
            if (values == null || values.Count() == 0)
                values = GetAbilityProperty(value.Key, ability)?.Distinct().Select(x => x.ToString() + postFix);
            var joinedValue = string.Join(ValueSeparator, values ?? Enumerable.Empty<string>());

            if (string.IsNullOrWhiteSpace(joinedValue))
            {
                _logger.LogDebug("Skipped a display value for {0} with key {1}", ability.InternalName, value.Key);
                ability.DisplayedValues.Remove(value.Key);
                continue;
            }

            ability.DisplayedValues[value.Key] += $" {Discord.Format.Bold(joinedValue)}"; // Append value after header
            ability.DisplayedValues[value.Key] = CleanSimple(ability.DisplayedValues[value.Key]);
        }

        foreach (var value in ability.ShardValues.Where(x => !string.IsNullOrEmpty(x.Description)))
        {
            var postFix = value.Description!.StartsWith('%') ? "%" : string.Empty;
            value.Description = value.Description.Trim('%');
            var values = value.Values.Distinct().Select(x => x.ToString() + postFix);
            if (values.Count() == 0)
                values = ability.AbilityValues.First(x => x.Name.Equals(value.Name, StringComparison.InvariantCultureIgnoreCase)).Values.Distinct().Select(x => x.ToString() + postFix);
            if (values.Count() == 0)
                values = ability.ShardValues.First(x => x.Name.Equals(value.Name, StringComparison.InvariantCultureIgnoreCase)).Values.Distinct().Select(x => x.ToString() + postFix);
            var joinedValue = string.Join(ValueSeparator, values);
            value.Description += $" {Discord.Format.Bold(joinedValue)}"; // Append value after header
            value.Description = CleanSimple(value.Description);
        }
        foreach (var value in ability.ScepterValues.Where(x => !string.IsNullOrEmpty(x.Description)))
        {
            var postFix = value.Description!.StartsWith('%') ? "%" : string.Empty;
            value.Description = value.Description.Trim('%');
            var values = value.Values.Distinct().Select(x => x.ToString() + postFix);
            if (values.Count() == 0)
                values = ability.AbilityValues.First(x => x.Name.Equals(value.Name, StringComparison.InvariantCultureIgnoreCase)).Values.Distinct().Select(x => x.ToString() + postFix);
            if (values.Count() == 0)
                values = ability.ScepterValues.First(x => x.Name.Equals(value.Name, StringComparison.InvariantCultureIgnoreCase)).Values.Distinct().Select(x => x.ToString() + postFix);
            var joinedValue = string.Join(ValueSeparator, values);

            value.Description += $" {Discord.Format.Bold(joinedValue)}"; // Append value after header
            value.Description = CleanSimple(value.Description);
        }
        if (ability.AbilityHasShard && string.IsNullOrEmpty(ability.ShardDescription))
        {
            _logger.LogDebug("Skipped shard with no desc for {0}", ability.InternalName);
            ability.AbilityHasShard = false;
        }
        else if (ability.AbilityHasShard || !string.IsNullOrEmpty(ability.ShardDescription))
        {
            var shardValueKeys = valueKeyRegex.Matches(ability.ShardDescription!);
            foreach (var valueKey in shardValueKeys.AsEnumerable())
            {
                var value = ability.ShardValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Values;
                value ??= ability.ShardValues.FirstOrDefault(x => x.Name == valueKey.Value.Replace("bonus_", "", StringComparison.OrdinalIgnoreCase))?.Values;
                value ??= GetAbilityProperty(valueKey.Value, ability);
                value ??= GetAbilityProperty(valueKey.Value.Replace("bonus_", "", StringComparison.OrdinalIgnoreCase), ability);
                // Added for special values that aren't tagged correctly... thanks valve
                value ??= ability.AbilityValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Values;
                value ??= ability.ScepterValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Values;
                value ??= ability.ShardValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Values;
                // Final fallback attempt
                if (value == null || value?.Count == 0)
                    value = ability.ShardValues.FirstOrDefault(x => x.Name.Equals(valueKey.Value, StringComparison.InvariantCultureIgnoreCase))?.Values.Distinct().ToList();
                if (value != null)
                    if (value != null)
                    {
                        var formattedValue =  Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct()));
                        if (Regex.IsMatch(ability.ShardDescription!, String.Format(@"(?<=%?){0}(?=%%%)", valueKey.Value)))
                        {
                            formattedValue = Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct().Select(x => x.ToString() + "%")));
                        }
                        ability.ShardDescription = Regex.Replace(ability.ShardDescription!,
                                                            @$"%{valueKey.Value}%",
                                                            Discord.Format.Bold(string.Join(ValueSeparator, formattedValue))); // Use distinct as some all duplicates
                    }
                ability.ShardDescription = escapedPercentage.Replace(ability.ShardDescription!, "");
            }
            ability.ShardDescription = CleanSimple(ability.ShardDescription!);
        }

        // conditionally ignore and log missing descriptions/use HasScepter
        if (ability.AbilityHasScepter && string.IsNullOrEmpty(ability.ScepterDescription))
        {
            _logger.LogDebug("Skipped scepter with no desc for {0}", ability.InternalName);
            ability.AbilityHasScepter = false;
        }
        else if (ability.AbilityHasScepter || !string.IsNullOrEmpty(ability.ScepterDescription))
        {
            var scepterValueKeys = valueKeyRegex.Matches(ability.ScepterDescription!);
            foreach (var valueKey in scepterValueKeys.AsEnumerable())
            {
                var value = ability.ScepterValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Values;
                value ??= ability.ScepterValues.FirstOrDefault(x => x.Name == valueKey.Value.Replace("bonus_", "", StringComparison.OrdinalIgnoreCase))?.Values;
                value ??= GetAbilityProperty(valueKey.Value, ability);
                value ??= GetAbilityProperty(valueKey.Value.Replace("bonus_", "", StringComparison.OrdinalIgnoreCase), ability);
                if (value == null || value?.Count == 0)
                    value = ability.ScepterValues.FirstOrDefault(x => x.Name.Equals(valueKey.Value, StringComparison.InvariantCultureIgnoreCase))?.Values.Distinct().ToList();
                if (value != null)
                {
                    var formattedValue =  Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct()));
                    if (Regex.IsMatch(ability.ScepterDescription!, String.Format(@"(?<=%?){0}(?=%%%)", valueKey.Value)))
                    {
                        formattedValue = Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct().Select(x => x.ToString() + "%")));
                    }
                    ability.ScepterDescription = Regex.Replace(ability.ScepterDescription!,
                                                        @$"%{valueKey.Value}%",
                                                        formattedValue); // Use distinct as some all duplicates
                }
                ability.ScepterDescription = escapedPercentage.Replace(ability.ScepterDescription!, "");
            }
            ability.ScepterDescription = CleanSimple(ability.ScepterDescription!);
        }

        var newNotes = new List<string>();
        foreach (var note in ability.Notes)
        {
            var noteValueKeys = valueKeyRegex.Matches(note);
            var newNote = note;
            foreach (var bonusValueKey in noteValueKeys.AsEnumerable())
            {
                var value = ability.AbilityValues.FirstOrDefault(x => x.Name == bonusValueKey.Value)?.Values;
                value ??= GetAbilityProperty(bonusValueKey.Value, ability);
                if (value == null && ability.AbilityIsGrantedByScepter)
                    value = ability.ScepterValues.FirstOrDefault(x => x.Name == bonusValueKey.Value)?.Values;
                if (value == null && ability.AbilityIsGrantedByShard)
                    value = ability.ShardValues.FirstOrDefault(x => x.Name == bonusValueKey.Value)?.Values;
                if (value != null)
                {
                    var formattedValue = Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct()));
                    if (Regex.IsMatch(ability.Description, String.Format(@"(?<=%?){0}(?=%%%)", bonusValueKey.Value)))
                    {
                        formattedValue = Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct().Select(x => x.ToString() + "%")));
                    }
                    newNote = Regex.Replace(newNote, @$"%{bonusValueKey.Value}%", formattedValue);
                }
            }
            newNote = escapedPercentage.Replace(newNote, "");
            newNote = CleanLocaleValue(newNote);
            newNotes.Add(newNote);
        }
        ability.Notes = newNotes;
    }

    private IList<float>? GetAbilityProperty(string name, BaseSpell ability)

        => name.ToLower() switch
        {
            "abilitycastrange" => ability.AbilityCastRange,
            "abilitychanneltime" => ability.AbilityChannelTime,
            "abilitycooldown" => ability.AbilityCooldown,
            "abilitydamage" => ability.AbilityDamage,
            "abilityduration" => ability.AbilityDuration,
            "abilitymanacost" => ability.AbilityManaCost,
            "abilityhealthcost" => ability.AbilityHealthCost,
            _ => null
        };


    private Hero CreateHero(string language, KVObject kvhero)
    {
        var hero = new Hero();

        hero.InternalName = kvhero.Name;
        hero.Language = language;
        hero.Id = kvhero.ParseChildValue<int>("HeroID");
        hero.Name = NameGender.Replace(GetHeroValue(language, hero.InternalName, isName: true), string.Empty);
        hero.NameAliases = kvhero.ParseChildValueList<string>("NameAliases");
        hero.Bio = GetHeroValue(language, hero.InternalName, "bio");
        hero.Hype = GetHeroValue(language, hero.InternalName, "hype");
        hero.NpeDesc = GetHeroValue(language, hero.InternalName, "npedesc1");
        hero.HeroOrderID = kvhero.ParseChildValue<short>("HeroOrderID");

        hero.AttributeBaseAgility = kvhero.ParseChildValue<byte>("AttributeBaseAgility");
        hero.AttributeBaseStrength = kvhero.ParseChildValue<byte>("AttributeBaseStrength");
        hero.AttributeBaseIntelligence = kvhero.ParseChildValue<byte>("AttributeBaseIntelligence");
        hero.AttributeAgilityGain = kvhero.ParseChildValue<float>("AttributeAgilityGain");
        hero.AttributeStrengthGain = kvhero.ParseChildValue<float>("AttributeStrengthGain");
        hero.AttributeIntelligenceGain = kvhero.ParseChildValue<float>("AttributeIntelligenceGain");
        hero.AttributePrimary = kvhero.ParseChildEnum<AttributePrimary>("AttributePrimary");

        hero.Complexity = kvhero.ParseChildValue<byte>("Complexity");
        hero.Role = kvhero.ParseChildEnumList<Role>("Role").ToArray();
        hero.Rolelevels = kvhero.ParseChildValueList<byte>("Rolelevels").ToArray();

        // Where defaults defined below, these are known to be defaults. Ignoring the rest at my own peril
        hero.AttackCapabilities = kvhero.ParseChildEnum<AttackCapabilities>("AttackCapabilities");
        hero.AttackDamageMin = kvhero.ParseChildValue<short>("AttackDamageMin");
        hero.AttackDamageMax = kvhero.ParseChildValue<short>("AttackDamageMax");
        hero.AttackRate = kvhero.ParseChildValue<float>("AttackRate", _baseHero.AttackRate);
        hero.BaseAttackSpeed = kvhero.ParseChildValue<short>("BaseAttackSpeed", _baseHero.BaseAttackSpeed);
        hero.AttackAnimationPoint = kvhero.ParseChildValue<float>("AttackAnimationPoint");
        hero.AttackRange = kvhero.ParseChildValue<float>("AttackRange");
        hero.ProjectileSpeed = kvhero.ParseChildValue<float>("ProjectileSpeed", _baseHero.ProjectileSpeed);
        hero.ArmorPhysical = kvhero.ParseChildValue<short>("ArmorPhysical", _baseHero.ArmorPhysical);
        hero.MagicalResistance = kvhero.ParseChildValue<short>("MagicalResistance", _baseHero.MagicalResistance);
        hero.MovementSpeed = kvhero.ParseChildValue<short>("MovementSpeed");
        hero.MovementTurnRate = kvhero.ParseChildValue<float>("MovementTurnRate", _baseHero.MovementTurnRate);
        hero.VisionDaytimeRange = kvhero.ParseChildValue<short>("VisionDaytimeRange", _baseHero.VisionDaytimeRange);
        hero.VisionNighttimeRange = kvhero.ParseChildValue<short>("VisionNighttimeRange", _baseHero.VisionNighttimeRange);
        hero.StatusHealth = kvhero.ParseChildValue<short>("StatusHealth", _baseHero.StatusHealth);
        hero.StatusHealthRegen = kvhero.ParseChildValue<float>("StatusHealthRegen", _baseHero.StatusHealthRegen);
        hero.StatusMana = kvhero.ParseChildValue<short>("StatusMana", _baseHero.StatusMana);
        hero.StatusManaRegen = kvhero.ParseChildValue<float>("StatusManaRegen", _baseHero.StatusHealthRegen);

        hero.Abilities = GetHeroAbilities(language, kvhero);
        hero.Talents = GetHeroTalents(language, kvhero);

        _heroAbilities.AddRange(hero.Abilities);

        //var abilityTalentLinks = new HashSet<(Ability, Talent)>();
        //foreach (var ability in hero.Abilities)
        //    foreach (var link in ability.AbilityValues.Where(x => x.LinkedSpecialBonus != null))
        //        abilityTalentLinks.Add((ability, hero.Talents.First(x => x.InternalName == link.SpecialBonusValue)));

        //foreach (var talent in hero.Talents)
        //    foreach (var link in talent.TalentValues.Where(x => x.AdLinkedAbilities != null))
        //        abilityTalentLinks.Add((hero.Abilities.First(x => x.InternalName == link.AdLinkedAbilities), talent));


        if (hero.Talents.Count() != 8) _logger.LogWarning("Hero {0} doesn't have 8 talents but {1}", hero.InternalName, hero.Talents.Count());

        _logger.LogTrace("Processed hero {0,-40} in {1}", hero.InternalName, language);
        return hero;
    }

    /// <summary>
    /// Replaces some simple tags, without any special formatting/replacements
    /// Useful for hero descriptions etc. but not Ability/Item values 
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Cleaned value</returns>
    private static string CleanSimple(string value)
    {
        var boldRegex    = new Regex(@"(?i)<[/]?\s*b\s*>");
        var italicsRegex = new Regex(@"(?i)<[/]?\s*i\s*>");
        var htmlTagRegex = new Regex(@"(?i)<[/]?\s*[^>]*>");
        value = value.Replace("<br>", "\n");
        value = boldRegex.Replace(value, "**");
        value = italicsRegex.Replace(value, "*");
        value = htmlTagRegex.Replace(value, "");

        return value;
    }

    /// <summary>
    /// Replaces "$agi"-style placeholder variables and appends it after the variables
    /// </summary>
    /// <param name="value"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private string CleanPlaceholders(string description, string values, string language)
    {
        var matches = Regex.Matches(description, @"\$\w+");
        var signRegex = new Regex(@"[+-](?=\w+)");
        description = CleanSimple(description);
        if (matches.Count > 0)
            foreach (Match match in matches)
                description = description.Replace(match.Value, $"\u00A0{values}\u00A0\u00A0{GetAbilityVariable(language, match.Value.Trim('$'))}");
        else if (signRegex.IsMatch(description))
            description = signRegex.Replace(description, m => string.Format("{0}\u00A0{1}\u00A0", m.Value, signRegex.Replace(values, string.Empty)));
        else
            description += $"\u00A0{values}";

        return description;
    }

    private string GetAbilityVariable(string language, string name)
    {
        var key = (language, Key: $"dota_ability_variable_{name}".ToLower());
        if (_abilityValues.ContainsKey(key))
        {
            return _abilityValues[key];
        }
        else
        {
            _abilityValues.TryGetValue((_localisationOptions.DefaultLanguage, key.Key), out var value);
            return value ?? String.Empty;
        }
    }

    private static string CleanLocaleValue(string value)
    {
        var boldRegex    = new Regex(@"(?i)<[/]?\s*b\s*/?>");
        var italicsRegex = new Regex(@"(?i)<[/]?\s*i\s*>");
        var htmlTagRegex = new Regex(@"(?i)<[/]?\s*[^>]*>");
        value = value.Replace("<br>", "\n");
        value = boldRegex.Replace(value, "**");
        value = italicsRegex.Replace(value, "*");
        value = htmlTagRegex.Replace(value, "");

        return value;
    }

    private string GetAbilityValue(string language, string internalName, string? valueName = null)
    {
        var postfix = valueName switch
        {
            null                  => "",
            _                     => "_" + valueName,
        };

        var key = (language, Key: $"DOTA_Tooltip_ability_{internalName}{postfix}".ToLower());
        if (_abilityValues.ContainsKey(key))
        {
            return _abilityValues[key];
        }
        else
        {
            _abilityValues.TryGetValue((_localisationOptions.DefaultLanguage, key.Key), out var value);
            return value ?? String.Empty;
        }
    }

    private IList<string> GetAbilityNoteValues(string language, string internalName)
    {
        var noteRegex = new Regex($"(?i)DOTA_Tooltip_ability_{internalName}_Note\\d+");
        // Distinct as the same Key will be present in different languages.
        // Watch out if another language has more notes than default language...
        var notesCount = _abilityValues.Keys.Where(x => x.Language == language && noteRegex.IsMatch(x.Key)).Distinct().Count();
        var notes = new List<string>();
        for (var i = 0; i < notesCount; i++)
        {
            notes.Add(GetAbilityValue(language, internalName, $"Note{i}"));
        }
        return notes;
    }

    private string GetHeroValue(string language, string internalName, string? postfix = null, bool isName = false)
    {
        var key = $"{internalName}{(postfix != null ? $"_{postfix}" : "")}{(isName ? ":n" : null)}".ToLower();
        var localeKey = (language, Key:key);
        if (_dotaValues.ContainsKey(localeKey))
        {
            return _dotaValues[localeKey];
        }
        else if (_heroLoreValues.ContainsKey(localeKey))
        {
            return _heroLoreValues[localeKey];
        }
        else
        {
            if (!_dotaValues.TryGetValue((_localisationOptions.DefaultLanguage, localeKey.Key), out var value))
            {
                _heroLoreValues.TryGetValue((_localisationOptions.DefaultLanguage, localeKey.Key), out value);
            }
            return value ?? "";
        }
    }

    private IList<Ability> GetHeroAbilities(string language, KVObject kvHero)
    {
        var abilityRegex       = new Regex("Ability\\d+");
        var hiddenOrEmptyRegex = new Regex("([\\w]+_empty\\d*)|([\\w]+_hidden\\d*)");
        var abilityNames       = kvHero.Children.Where(x => abilityRegex.IsMatch(x.Name)).Select(x => x.Value.ToString());
        var abilities          = new List<Ability>();
        foreach (var name in abilityNames)
        {
            if (string.IsNullOrEmpty(name) || name == "special_bonus_attributes" || hiddenOrEmptyRegex.IsMatch(name))
                continue;
            var ability = _abilities.FirstOrDefault(x => x.InternalName == name && x.Language == language);

            if (ability == null)
                continue;

            // Hidden and Not Learnable indicate secondary abilities that are linked to a main ability (e.g. Sun Ray Toggle)
            // Skip these, as they are empty
            if (ability.AbilityBehavior.HasFlag(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_HIDDEN | AbilityBehavior.DOTA_ABILITY_BEHAVIOR_NOT_LEARNABLE)
                && string.IsNullOrEmpty(ability.Description))
                continue;

            abilities.Add(ability);
        }
        return abilities;
    }

    private IList<Talent> GetHeroTalents(string language, KVObject kvHero)
    {
        var abilityRegex       = new Regex("Ability\\d+");
        var hiddenOrEmptyRegex = new Regex("([\\w]+_empty\\d*)|([\\w]+_hidden\\d*)");
        var talentNames        = kvHero.Children.Where(x => abilityRegex.IsMatch(x.Name)).Select(x => x.Value.ToString());
        var talents            = new List<Talent>();

        var counter = 0f;
        foreach (var name in talentNames)
        {
            if (string.IsNullOrEmpty(name) || name == "special_bonus_attributes" || hiddenOrEmptyRegex.IsMatch(name))
                continue;
            var talent = _talents.FirstOrDefault(x => x.InternalName == name && x.Language == language);
            if (talent == null)
                continue;

            byte level       = (byte)(10 + (5 * Math.Floor(counter / 2)));
            byte side        = (byte)(counter % 2);
            talent.Position = (level, side);
            talents.Add(talent);
            counter++;
        }
        return talents;
    }

    private Dictionary<string, byte> GetNeutralItemTiers(KVObject kvNeutralItems)
    {
        Dictionary<string, byte> neutralItems = new Dictionary<string, byte>();

        foreach (var tier in kvNeutralItems)
            foreach (var item in tier.First(x => x.Name == "items").Children)
                neutralItems.Add(item.Name, byte.Parse(tier.Name));

        return neutralItems;
    }

    private Item CreateItem(string language, KVObject kvItem)
    {
        var item = new Item();

        item.Id = _ItemIds[kvItem.Name];
        item.InternalName = kvItem.Name;
        item.Language = language;

        var itemName = string.Empty;
        if (item.InternalName.StartsWith("item_dagon_")) // Damn dagon
            itemName = GetAbilityValue(language, item.InternalName + "L");
        else
            itemName = GetAbilityValue(language, item.InternalName);
        item.Name = itemName;

        item.Description = GetAbilityValue(language, item.InternalName, "Description");
        item.Lore = GetAbilityValue(language, item.InternalName, "Lore");
        item.Notes = GetAbilityNoteValues(language, item.InternalName);

        item.AbilityType = kvItem.ParseChildEnum<AbilityType>("AbilityType");
        item.AbilityBehavior = kvItem.ParseChildEnum<AbilityBehavior>("AbilityBehavior");
        item.AbilityUnitTargetTeam = kvItem.ParseChildEnum<AbilityUnitTargetTeam>("AbilityUnitTargetTeam");
        item.AbilityUnitTargetType = kvItem.ParseChildEnum<AbilityUnitTargetType>("AbilityUnitTargetType");
        item.AbilityUnitDamageType = kvItem.ParseChildEnum<AbilityUnitDamageType>("AbilityUnitDamageType");
        item.SpellImmunityType = kvItem.ParseChildEnum<SpellImmunityType>("SpellImmunityType");
        item.SpellDispellableType = kvItem.ParseChildEnum<SpellDispellableType>("SpellDispellableType");

        item.AbilityCastRange = kvItem.ParseChildValueList<float>("AbilityCastRange");
        item.AbilityCastPoint = kvItem.ParseChildValueList<float>("AbilityCastPoint");
        item.AbilityChannelTime = kvItem.ParseChildValueList<float>("AbilityChannelTime");
        item.AbilityCharges = kvItem.ParseChildValueList<float>("AbilityCharges");
        item.AbilityChargeRestoreTime = kvItem.ParseChildValueList<float>("AbilityChargeRestoreTime");
        item.AbilityCooldown = kvItem.ParseChildValueList<float>("AbilityCooldown");
        item.AbilityDuration = kvItem.ParseChildValueList<float>("AbilityDuration");
        item.AbilityDamage = kvItem.ParseChildValueList<float>("AbilityDamage");
        item.AbilityManaCost = kvItem.ParseChildValueList<float>("AbilityManaCost");
        item.AbilityHealthCost = kvItem.ParseChildValueList<float>("AbilityHealthCost");

        item.ItemAliases = kvItem.ParseChildValueList<string>("ItemAliases");
        item.ItemBaseLevel = kvItem.ParseChildValue<byte>("ItemBaseLevel");
        item.MaxUpgradeLevel = kvItem.ParseChildValue<byte>("MaxUpgradeLevel");
        item.ItemCost = kvItem.ParseChildValue<short>("ItemCost", emptyValueReturnDefault: true);
        item.ItemInitialCharges = kvItem.ParseChildValue<int>("ItemInitialCharges");
        item.ItemInitialStockTime = kvItem.ParseChildValue<float>("ItemInitialStockTime");
        item.ItemIsNeutralDrop = kvItem.ParseChildValue<bool>("ItemIsNeutralDrop");
        item.ItemNeutralTier = _neutralItemTiers.FirstOrDefault(x => x.Key == item.InternalName).Value;
        item.ItemPurchasable = kvItem.ParseChildValue<bool>("ItemPurchasable", true);
        item.ItemRecipe = kvItem.ParseChildValue<bool>("ItemRecipe");
        //item.ItemRequirements   = kvItem.ParseChildValueList<string[]>("ItemRequirements");
        item.ItemResult = kvItem.ParseChildValue<string>("ItemResult");
        item.ItemStockInitial = kvItem.ParseChildValue<byte>("ItemStockInitial");
        item.ItemStockMax = kvItem.ParseChildValue<byte>("ItemStockMax");
        item.ItemStockTime = kvItem.ParseChildValue<float>("ItemStockTime");

        item.AbilityValues = GetAbilityValues(language, kvItem);
        item.DisplayedValues = GetItemDisplayedValues(language, kvItem);

        _logger.LogTrace("Processed {0,7} {1,-64} in {2}\"", "ability", item.InternalName, language);
        return item;
    }

    private void FormatItem(Item item)
    {
        var valueKeyRegex         = new Regex(@"(?<=%)\w+(?=%)");
        var valuePlaceholderRegex = new Regex(@"%\w+[%]{1,3}");
        var escapedPercentage     = new Regex(@"%%(?=[^%]?)");
        var itemSpellRegex        = new Regex(@"<h1>.*?(?=<h1>|\Z|$)", RegexOptions.Singleline);
        var spellNameRegex        = new Regex(@"<h1>(.+?)</h1>");

        if (item.Description == null)
            return;

        var itemSpells        = new List<Item.Spell>();
        var spellDescriptions = itemSpellRegex.Matches(item.Description);
        foreach (Match spell in spellDescriptions)
        {
            var name        = spellNameRegex.Match(spell.Value).Groups[1].Value; //This should match the group
            var description = spellNameRegex.Replace(spell.Value, string.Empty);

            var descriptionPlaceholders = valuePlaceholderRegex.Matches(item.Description);
            foreach (Match placeholder in descriptionPlaceholders)
            {
                var key     = valueKeyRegex.Match(placeholder.Value).Value;
                var postFix = escapedPercentage.IsMatch(placeholder.Value) ? "%" : string.Empty;
                var values  = item.AbilityValues.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))?.Values.Select(x => x.ToString() + postFix).ToArray();
                if (values == null || values.Length == 0)
                    values = GetAbilityProperty(key, item)?.Select(x => x.ToString() + postFix).ToArray();
                values ??= [];
                var joinedValue = string.Empty;
                // This formally assumed dagon only, but should hopefully now work for anything
                // that has multiple 'levels' like dagon.
                if (item.ItemBaseLevel > 0 && values.Length >= item.ItemBaseLevel)
                {
                    values[item.ItemBaseLevel - 1] = Discord.Format.Bold(values[item.ItemBaseLevel - 1]);
                    joinedValue = string.Join(ValueSeparator, values);
                }
                else
                {
                    joinedValue = Discord.Format.Bold(string.Join(ValueSeparator, values));
                }
                description = Regex.Replace(description, $@"%{key}[%]{{1,3}}", joinedValue);
            }
            description = CleanSimple(description);
            itemSpells.Add(new() { Name = name, Description = description });
        }
        item.Spells = itemSpells;

        foreach (var value in item.DisplayedValues)
        {
            var postFix = value.Value.StartsWith('%') ? "%" : string.Empty;
            item.DisplayedValues[value.Key] = item.DisplayedValues[value.Key].Trim('%');
            var values = item.AbilityValues.FirstOrDefault(x => x.Name.Equals(value.Key, StringComparison.InvariantCultureIgnoreCase))?.Values.Select(x => x.ToString() + postFix).ToArray();
            if (values == null || values.Length == 0)
                values = GetAbilityProperty(value.Key, item)?.Distinct().Select(x => x.ToString() + postFix).ToArray();
            if (values == null || values.Length == 0)
            {
                _logger.LogDebug("Skipped a display value for {0} with key {1}", item.InternalName, value.Key);
                item.DisplayedValues.Remove(value.Key);
                continue;
            }
            var joinedValue = string.Empty;
            if (item.ItemBaseLevel > 0 && values.Length >= item.ItemBaseLevel)
            {
                values![item.ItemBaseLevel - 1] = Discord.Format.Bold(values[item.ItemBaseLevel - 1]);
                joinedValue = string.Join(ValueSeparator, values ?? Enumerable.Empty<string>());
            }
            else
            {
                joinedValue = Discord.Format.Bold(string.Join(ValueSeparator, values ?? Enumerable.Empty<string>()));
            }
            var displayedValue = item.DisplayedValues[value.Key];
            displayedValue = CleanPlaceholders(displayedValue, joinedValue, item.Language);

            item.DisplayedValues[value.Key] = displayedValue;
        }

        var newNotes = new List<string>();
        foreach (var note in item.Notes)
        {
            var noteValueKeys = valueKeyRegex.Matches(note);
            var newNote = note;
            foreach (var bonusValueKey in noteValueKeys.AsEnumerable())
            {
                var value = item.AbilityValues.FirstOrDefault(x => x.Name == bonusValueKey.Value)?.Values;
                value ??= GetAbilityProperty(bonusValueKey.Value, item);
                if (value != null)
                {
                    var formattedValue = Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct()));
                    if (Regex.IsMatch(item.Description, String.Format(@"(?<=%?){0}(?=%%%)", bonusValueKey.Value)))
                    {
                        formattedValue = Discord.Format.Bold(string.Join(ValueSeparator, value.Distinct().Select(x => x.ToString() + "%")));
                    }
                    newNote = Regex.Replace(newNote, @$"%{bonusValueKey.Value}%", formattedValue);
                }
            }
            newNote = escapedPercentage.Replace(newNote, "");
            newNote = CleanLocaleValue(newNote);
            newNotes.Add(newNote);
        }
        item.Notes = newNotes;
    }

    /// <summary>
    /// This is a messy ad-hoc slotted into this existing monolith.
    /// </summary>
    private async Task CreateAndStoreEntityLocalisations()
    {
        _logger.LogInformation("Creating entity localisation records.");

        var entityLocalisation = new List<EntityLocalisation>();

        foreach (var heroLocalisationsGroup in _heroes.GroupBy(x => (x.Id, x.InternalName)))
        {
            _logger.LogDebug("Processing hero and ability EntityLocalisations for {key}", heroLocalisationsGroup.Key);

            var heroLocalisation = new EntityLocalisation()
            {
                EntityId     = heroLocalisationsGroup.Key.Id,
                InternalName = heroLocalisationsGroup.Key.InternalName,
                DefaultTag   = _localisationOptions.DefaultTag,
                Type         = EntityType.HERO,
                NameLocalisations = new Dictionary<string, string>(),
            };
            SetEntityLocalisationId(heroLocalisation);

            // Set the default first, to be compared against after.
            // This is dirty, to be fixed with localisation + DataBuilder refactor.
            heroLocalisation.NameLocalisations[_localisationOptions.DefaultTag] = heroLocalisationsGroup.First(x => x.Language == _localisationOptions.DefaultLanguage).Name;

            foreach (var localisedHero in heroLocalisationsGroup)
            {
                if (localisedHero.Name != heroLocalisation.DefaultName)
                {
                    // now this is filthy... but only language with two tags is english so it won't matter about only accessing one index as its default anyway.
                    heroLocalisation.NameLocalisations[_localisationOptions.SourceLocaleMappings[localisedHero.Language][0]] = localisedHero.Name;
                }
            }
            entityLocalisation.Add(heroLocalisation);
        }

        // Ensure indexes
        _db.CreateCollection<EntityLocalisation>();
        await _db.EnsureIndex<EntityLocalisation>(x => x.EntityId);
        await _db.EnsureIndex<EntityLocalisation>(x => x.InternalName, caseSensitive: false);
        // Add records
        await _db.UpsertRecords(entityLocalisation);

        _logger.LogInformation("Finished creating entity localisation records.");
    }

    private static void SetEntityLocalisationId(EntityLocalisation entity)
    {
        var str = $"{entity.EntityId}_{entity.InternalName}_{entity.Type}";
        var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
        entity.Id = BitConverter.ToUInt64(hash);
    }

    private async Task StoreEntityEmbeds()
    {
        _logger.LogInformation("Converting entities to Info Embed records");
        var heroInfoEmbeds    = new List<HeroInfoEmbed>();
        var abilityInfoEmbeds = new List<AbilityInfoEmbed>();
        var itemInfoEmbeds    = new List<ItemInfoEmbed>();
        var latestPatch       = await _db.GetLatestPatch();

        foreach (var hero in _heroes)
        {
            _logger.LogDebug("Processing hero and ability info embeds for {0,-40} in {1}", hero.InternalName, hero.Language);
            heroInfoEmbeds.AddRange(hero.GetHeroInfoEmbeds(_localisationOptions.SourceLocaleMappings, latestPatch));
            foreach (var heroAbility in hero.Abilities)
            {
                abilityInfoEmbeds.AddRange(heroAbility.CreateAbilityInfoEmbeds(_localisationOptions.SourceLocaleMappings, latestPatch, hero));
            }
        }
        foreach (var item in _items.Where(x => !x.ItemRecipe))
        {
            _logger.LogDebug("Processing item info embeds for {0,-40} in {1}", item.InternalName, item.Language);
            itemInfoEmbeds.AddRange(item.CreateItemInfoEmbeds(_localisationOptions.SourceLocaleMappings, latestPatch));
        }


        _logger.LogInformation("Updating entity info embeds in Database");

        await EnsureIndexes();
        await _db.UpsertRecords(heroInfoEmbeds);
        await _db.UpsertRecords(abilityInfoEmbeds);
        await _db.UpsertRecords(itemInfoEmbeds);
    }

    private async Task EnsureIndexes()
    {
        _db.CreateCollection<HeroInfoEmbed>();
        await _db.EnsureIndex<HeroInfoEmbed>(x => x.Locale);
        await _db.EnsureIndex<HeroInfoEmbed>(x => x.InternalName, caseSensitive: false);
        await _db.EnsureIndex<HeroInfoEmbed>(x => x.Name, caseSensitive: false);
        await _db.EnsureIndex<HeroInfoEmbed>(x => x.RealName!, caseSensitive: false);
        await _db.EnsureIndex<HeroInfoEmbed>(x => x.Aliases!, caseSensitive: false);
        Thread.Sleep(2000);
        _db.CreateCollection<AbilityInfoEmbed>();
        await _db.EnsureIndex<AbilityInfoEmbed>(x => x.Locale);
        await _db.EnsureIndex<AbilityInfoEmbed>(x => x.InternalName, caseSensitive: false);
        await _db.EnsureIndex<AbilityInfoEmbed>(x => x.Name, caseSensitive: false);
        Thread.Sleep(1000);
        _db.CreateCollection<ItemInfoEmbed>();
        await _db.EnsureIndex<ItemInfoEmbed>(x => x.Locale);
        await _db.EnsureIndex<ItemInfoEmbed>(x => x.InternalName, caseSensitive: false);
        await _db.EnsureIndex<ItemInfoEmbed>(x => x.Name, caseSensitive: false);
        await _db.EnsureIndex<ItemInfoEmbed>(x => x.Aliases!, caseSensitive: false);
    }
}
