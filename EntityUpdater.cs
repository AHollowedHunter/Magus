using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.DataBuilder.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder
{
    public class EntityUpdater
    {
        private readonly IDatabaseService _db;
        private readonly IConfiguration _config;
        private readonly ILogger<PatchNoteUpdater> _logger;
        private readonly HttpClient _httpClient;
        private readonly KVSerializer _kvSerializer;

        private readonly string _sourceDefaultLanguage;
        private readonly Dictionary<string, string[]> _sourceLocaleMappings;
        private readonly Dictionary<(string Language, string Key), string> _abilityValues;
        private readonly Dictionary<(string Language, string Key), string> _dotaValues;
        private readonly Dictionary<(string Language, string Key), string> _heroLoreValues;
        private readonly List<Ability> _abilities;
        private readonly List<Talent> _talents;
        private readonly List<Hero> _heroes;
        private readonly List<Item> _items;
        private Hero _baseHero;

        public EntityUpdater(IDatabaseService db, IConfiguration config, ILogger<PatchNoteUpdater> logger, HttpClient httpClient)
        {
            _db         = db;
            _config     = config;
            _logger     = logger;
            _httpClient = httpClient;

            _kvSerializer          = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            _sourceDefaultLanguage = _config.GetSection("Localisation").GetValue("DefaultLanguage", "english");
            _sourceLocaleMappings  = _config.GetSection("Localisation").GetSection("SourceLocaleMappings").Get<Dictionary<string, string[]>>();
            _abilityValues         = new();
            _dotaValues            = new();
            _heroLoreValues        = new();
            _abilities             = new();
            _talents               = new();
            _heroes                = new();
            _items                 = new();
            _baseHero              = new();
        }

        public async Task Update()
        {
            _logger.LogInformation("Starting Entity Update");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await SetEntityValues();
            await SetEntities();

            //StorePatchNoteEmbeds();
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

            foreach (var language in _sourceLocaleMappings)
            {
                _logger.LogDebug("Processing values for {0}", language.Key);

                var abilities = await GetKVObjectFromUri(Dota2GameFiles.Localization.GetAbilities(language.Key));
                var dota      = await GetKVObjectFromUri(Dota2GameFiles.Localization.GetDota(language.Key));
                var heroLore  = await GetKVObjectFromUri(Dota2GameFiles.Localization.GetHeroLore(language.Key));

                foreach (var note in abilities.Children.First(x => x.Name == "Tokens"))
                    _abilityValues.Add((language.Key, note.Name), CleanSimple(note.Value.ToString() ?? ""));

                foreach (var note in dota.Children.First(x => x.Name == "Tokens"))
                    _dotaValues.Add((language.Key, note.Name), CleanSimple(note.Value.ToString() ?? ""));

                foreach (var note in heroLore.Children.First(x => x.Name == "Tokens"))
                    _heroLoreValues.Add((language.Key, note.Name), CleanSimple(note.Value.ToString() ?? ""));
            }
            _logger.LogInformation("Finished setting Entity values");
        }

        private async Task SetEntities()
        {
            _logger.LogInformation("Setting Entities");

            var mixedAbilities    = await GetKVObjectFromUri(Dota2GameFiles.NpcAbilities);
            var heroes       = await GetKVObjectFromUri(Dota2GameFiles.NpcHeroes);
            var items        = await GetKVObjectFromUri(Dota2GameFiles.Items);
            var talentRegex  = new Regex("special_bonus_\\w+");
            var mainAbilities      = new List<KVObject>();

            _abilities.Clear();
            _talents.Clear();
            _heroes.Clear();
            _items.Clear();

            foreach (var ability in mixedAbilities.Children.Where(x => x.Name != "Version"))
            {
                if (ability.Children.Count() == 0)
                    continue;
                if (!talentRegex.IsMatch(ability.Name))
                {
                    mainAbilities.Add(ability);
                    continue;
                }

                _logger.LogDebug("Processing talent {0}", ability.Name);
                foreach (var language in _sourceLocaleMappings.Keys)
                {
                    _talents.Add(CreateTalent(language, ability));
                }
            }

            foreach (var ability in mainAbilities)
            {
                _logger.LogDebug("Processing ability {0}", ability.Name);
                foreach (var language in _sourceLocaleMappings.Keys)
                {
                    _abilities.Add(CreateAbility(language, ability));
                }
            }

            _baseHero = CreateHero("", heroes.Children.First(x => x.Name == "npc_dota_hero_base"));

            foreach (var hero in heroes.Children.Where(x => x.Name != "Version" && x.Name != "npc_dota_hero_base" && x.Name != "npc_dota_hero_target_dummy"))
            {
                _logger.LogDebug("Processing hero {0}", hero.Name);
                foreach (var language in _sourceLocaleMappings.Keys)
                {
                    _heroes.Add(CreateHero(language, hero));
                }
            }

            foreach (var item in items.Children.Where(x => x.Name != "Version"))
            {
                _logger.LogDebug("Processing item {0}", item.Name);
                foreach (var language in _sourceLocaleMappings.Keys)
                {
                    //_items.Add(CreateAbility(language, item));
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
                FormatTalent(talent);
                if (Regex.IsMatch(talent.Description, @"[{}]") && _heroes.Any(x => x.Talents.Any(x => x == talent)))
                    _logger.LogWarning("{0} description formatting wrong: {1}", talent.InternalName, talent.Description);
            }

            //foreach (var ability in _abilities)
            //{
            //    FormatTalent(ability);
            //}
            _logger.LogInformation("Completed formatting entity values");
        }

        private Talent CreateTalent(string language, KVObject kvTalent)
        {
            var talent = new Talent();

            talent.Id              = (int)kvTalent.Children.First(x => x.Name == "ID").Value!;
            talent.InternalName    = kvTalent.Name;
            talent.Language        = language;
            talent.AbilityType     = kvTalent.ParseChildEnum<AbilityType>("AbilityType");
            talent.AbilityBehavior = kvTalent.ParseChildEnum<AbilityBehavior>("AbilityBehavior");

            talent.TalentValues = GetTalentValues(kvTalent);
            talent.Description  = GetAbilityValue(language, talent.InternalName);
            //FormatTalent(talent);

            if (kvTalent.Children.Any(x => x.Name == "AbilityValues"))
                _logger.LogWarning("{0} contains AbilityValues!", kvTalent.Name);
            if (kvTalent.Children.FirstOrDefault(x => x.Name == "AbilitySpecial")?.Children.Count() > 1)
                _logger.LogWarning("{0} contains more than one AbilitySpecial!", kvTalent.Name);

            _logger.LogTrace("Processed {0,8} {1,-64} in {2}", "talent", talent.InternalName, language);
            return talent;
        }

        private void FormatTalent(Talent talent)
        {
            var valueKeyRegex              = new Regex(@"(?<=[+-]?{s:)\w+(?=})");
            var bonusValueKeyRegex         = new Regex(@"(?<=[+-]?{s:bonus_)\w+(?=})");

            var valueKeys = valueKeyRegex.Matches(talent.Description);
            foreach (var valueKey in valueKeys.AsEnumerable())
            {
                var value = talent.TalentValues.FirstOrDefault(x => x.Name == valueKey.Value)?.Values;
                if (value != null)
                    talent.Description = Regex.Replace(talent.Description, @$"(?<=[+-]?){{s:{valueKey.Value}}}", string.Join(',', value));
            }
            var bonusValueKeys = bonusValueKeyRegex.Matches(talent.Description);
            foreach (var bonusValueKey in bonusValueKeys.AsEnumerable())
            {
                var key = bonusValueKey.Value;
                var ability = _abilities.FirstOrDefault(x => x.AbilityValues.Any(y => y.LinkedSpecialBonus == talent.InternalName && y.Name == key));
                var abilityValue = ability?.AbilityValues.First(x => x.Name == key);
                if (abilityValue != null) {
                    talent.Description = Regex.Replace(talent.Description, @$"(?<=[+-]?){{s:bonus_{key}}}", abilityValue.SpecialBonusValue?.ToString() ?? "");
                }
            }
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
                            Name              = value.Name,
                            Values            = value.ParseList<float>(),
                            AdLinkedAbilities = item.ParseChildValue<string>("ad_linked_abilities"),
                        });
                    }
                    else
                    {
                        talentValues.Add(new()
                        {
                            Name   = item.Name,
                            Values = item.ParseList<float>(),
                        });
                    }
                }
            }
            return talentValues;
        }

        private Ability CreateAbility(string language, KVObject kvAbility)
        {
            var ability = new Ability();

            ability.Id           = (int)kvAbility.Children.First(x => x.Name == "ID").Value!;
            ability.InternalName = kvAbility.Name;
            ability.Language     = language;
            ability.Name         = GetAbilityValue(language, ability.InternalName);
            ability.Description  = GetAbilityValue(language, ability.InternalName, "Description");
            ability.Lore         = GetAbilityValue(language, ability.InternalName, "Lore");
            ability.Notes        = GetAbilityNoteValues(language, ability.InternalName);

            ability.AbilityType           = kvAbility.ParseChildEnum<AbilityType>("AbilityType");
            ability.AbilityBehavior       = kvAbility.ParseChildEnum<AbilityBehavior>("AbilityBehavior");
            ability.AbilityUnitTargetTeam = kvAbility.ParseChildEnum<AbilityUnitTargetTeam>("AbilityUnitTargetTeam");
            ability.AbilityUnitTargetType = kvAbility.ParseChildEnum<AbilityUnitTargetType>("AbilityUnitTargetType");
            ability.AbilityUnitDamageType = kvAbility.ParseChildEnum<AbilityUnitDamageType>("AbilityUnitDamageType");
            ability.SpellImmunityType     = kvAbility.ParseChildEnum<SpellImmunityType>("SpellImmunityType");
            ability.SpellDispellableType  = kvAbility.ParseChildEnum<SpellDispellableType>("SpellDispellableType");

            ability.AbilityCastRange   = kvAbility.ParseChildValueList<float>("AbilityCastRange");
            ability.AbilityCastPoint   = kvAbility.ParseChildValueList<float>("AbilityCastPoint");
            ability.AbilityChannelTime = kvAbility.ParseChildValueList<float>("AbilityChannelTime");
            ability.AbilityCooldown    = kvAbility.ParseChildValueList<float>("AbilityCooldown");
            ability.AbilityDuration    = kvAbility.ParseChildValueList<float>("AbilityDuration");
            ability.AbilityDamage      = kvAbility.ParseChildValueList<float>("AbilityDamage");
            ability.AbilityManaCost    = kvAbility.ParseChildValueList<float>("AbilityManaCost");

            ability.AbilityValues = GetAbilityValues(kvAbility);

            _logger.LogTrace("Processed {0,7} {1,-64} in {2}\"", "ability", ability.InternalName, language);
            return ability;
        }

        private IEnumerable<BaseSpell.AbilityValue> GetAbilityValues(KVObject kvAbility)
        {
            var abilityValues = new List<BaseSpell.AbilityValue>();

            var kvAbilityValues = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityValues" || x.Name == "AbilitySpecial");
            if (kvAbilityValues != null)
            {
                foreach (var item in kvAbilityValues.Children)
                {
                    if (item.Value.ValueType == KVValueType.Collection)
                    {
                        if (Regex.IsMatch(item.Name, @"\d+"))
                        {
                            var value = item.First(x => x.Name != "var_type" && x.Name != "ad_linked_abilities");
                            abilityValues.Add(new()
                            {
                                Name               = value.Name,
                                Values             = value.ParseList<float>(),
                                LinkedSpecialBonus = item.ParseChildValue<string>("ad_linked_abilities"),
                            });
                        }
                        else
                        {
                            if (item.Any(x => x.Name == "LinkedSpecialBonus"))
                            {
                                abilityValues.Add(new()
                                {
                                    Name               = item.Name,
                                    Values             = item.ParseChildValueList<float>("value"),
                                    LinkedSpecialBonus = item.ParseChildValue<string>("LinkedSpecialBonus"),
                                });
                            }
                            else
                            {
                                var bonus = item.FirstOrDefault(x => x.Name.StartsWith("special_bonus_"));
                                var values = item.ParseChildValue<string>("value") != "FIELD_INTEGER" ? item.ParseChildValueList<float>("value") : Enumerable.Empty<float>();
                                abilityValues.Add(new()
                                {
                                    Name               = item.Name,
                                    Values             = values,
                                    LinkedSpecialBonus = bonus?.Name,
                                    SpecialBonusValue  = bonus.ParseValue<string>()
                                });
                            }
                        }
                    }
                    else
                    {
                        abilityValues.Add(new()
                        {
                            Name   = item.Name,
                            Values = item.ParseList<float>(),
                        });
                    }
                }
            }
            return abilityValues;
        }

        private Hero CreateHero(string language, KVObject kvhero)
        {
            var hero = new Hero();

            hero.InternalName = kvhero.Name;
            hero.Language     = language;
            hero.Id           = kvhero.ParseChildValue<int>("HeroID");
            hero.Name         = GetHeroValue(language, hero.InternalName);
            hero.NameAliases  = kvhero.ParseChildValueList<string>("NameAliases");
            hero.Bio          = GetHeroValue(language, hero.InternalName, "bio");
            hero.Hype         = GetHeroValue(language, hero.InternalName, "hype");
            hero.NpeDesc      = GetHeroValue(language, hero.InternalName, "npedesc1");
            hero.HeroOrderID  = kvhero.ParseChildValue<short>("HeroOrderID");

            hero.AttributeBaseAgility      = kvhero.ParseChildValue<byte>("AttributeBaseAgility");
            hero.AttributeBaseStrength     = kvhero.ParseChildValue<byte>("AttributeBaseStrength");
            hero.AttributeBaseIntelligence = kvhero.ParseChildValue<byte>("AttributeBaseIntelligence");
            hero.AttributeAgilityGain      = kvhero.ParseChildValue<float>("AttributeAgilityGain");
            hero.AttributeStrengthGain     = kvhero.ParseChildValue<float>("AttributeStrengthGain");
            hero.AttributeIntelligenceGain = kvhero.ParseChildValue<float>("AttributeIntelligenceGain");
            hero.AttributePrimary          = kvhero.ParseChildEnum<AttributePrimary>("AttributePrimary");

            hero.Complexity = kvhero.ParseChildValue<byte>("Complexity");
            hero.Role       = kvhero.ParseChildEnumList<Role>("Role").ToArray();
            hero.Rolelevels = kvhero.ParseChildValueList<byte>("Rolelevels").ToArray();

            // Where defaults defined below, these are known to be defaults. Ignoring the rest at my own peril
            hero.AttackCapabilities   = kvhero.ParseChildEnum<AttackCapabilities>("AttackCapabilities");
            hero.AttackDamageMin      = kvhero.ParseChildValue<short>("AttackDamageMin");
            hero.AttackDamageMax      = kvhero.ParseChildValue<short>("AttackDamageMax");
            hero.AttackRate           = kvhero.ParseChildValue<float>("AttackRate", _baseHero.AttackRate);
            hero.BaseAttackSpeed      = kvhero.ParseChildValue<short>("BaseAttackSpeed", _baseHero.BaseAttackSpeed);
            hero.AttackAnimationPoint = kvhero.ParseChildValue<float>("AttackAnimationPoint");
            hero.AttackRange          = kvhero.ParseChildValue<float>("AttackRange");
            hero.ProjectileSpeed      = kvhero.ParseChildValue<float>("ProjectileSpeed", _baseHero.ProjectileSpeed);
            hero.ArmorPhysical        = kvhero.ParseChildValue<short>("ArmorPhysical", _baseHero.ArmorPhysical);
            hero.MagicalResistance    = kvhero.ParseChildValue<short>("MagicalResistance", _baseHero.MagicalResistance);
            hero.MovementSpeed        = kvhero.ParseChildValue<short>("MovementSpeed");
            hero.MovementTurnRate     = kvhero.ParseChildValue<float>("MovementTurnRate", _baseHero.MovementTurnRate);
            hero.VisionDaytimeRange   = kvhero.ParseChildValue<short>("VisionDaytimeRange", _baseHero.VisionDaytimeRange);
            hero.VisionNighttimeRange = kvhero.ParseChildValue<short>("VisionNighttimeRange", _baseHero.VisionNighttimeRange);
            hero.StatusHealth         = kvhero.ParseChildValue<short>("StatusHealth", _baseHero.StatusHealth);
            hero.StatusHealthRegen    = kvhero.ParseChildValue<float>("StatusHealthRegen", _baseHero.StatusHealthRegen);
            hero.StatusMana           = kvhero.ParseChildValue<short>("StatusMana", _baseHero.StatusMana);
            hero.StatusManaRegen      = kvhero.ParseChildValue<float>("StatusManaRegen", _baseHero.StatusHealthRegen);

            hero.Abilities = GetHeroAbilities(language, kvhero);
            hero.Talents = GetHeroTalents(language, kvhero);

            if (hero.Talents.Count() != 8) _logger.LogWarning("Hero {0} doesn't have 8 talents but {1}", hero.InternalName, hero.Talents.Count());

            _logger.LogTrace("Processed hero {0,-40} in {1}", hero.InternalName, language);
            return hero;
        }

        /// <summary>
        /// Replaces some simple tags, without any special formatting/replacements
        /// Useful for hero descriptions etc. but not Ability/Item values 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Cleaned value</returns>
        private static string CleanSimple(string value)
        {
            var boldRegex    = new Regex(@"(?i)<[/]?\s*b\s*>");
            var italicsRegex = new Regex(@"(?i)<[/]?\s*i\s*>");
            value            = value.Replace("<br>", "\n");
            value            = boldRegex.Replace(value, "**");
            value            = italicsRegex.Replace(value, "*");

            return value;
        }

        private static string CleanLocaleValue(string value)
        {
            var onlyBreak      = new Regex(@"^\s*<br>\s*$");
            var tableRegex     = new Regex(@"<table>(.|\n)*<\/table>");
            var boldRegex      = new Regex(@"(?i)<[/]?\s*b\s*/?>");
            var infoRegex      = new Regex(@"(?i)<[/]?\s*info\s*/?>");
            var highlightRegex = new Regex(@"(?i)<[/]?[\s.]*(class=""(New|Reworked)"")?[^>]*>");
            var htmlTagRegex   = new Regex(@"(?i)<[/]?\s*[^>]*>");
            value              = onlyBreak.Replace(value, "\n");
            value              = value.Replace("<br>", "\n");
            value              = value.Replace("&nbsp;", "");
            value              = value.Replace("*", "\\*");
            value              = tableRegex.Replace(value, "");
            value              = boldRegex.Replace(value, "**");
            value              = infoRegex.Replace(value, "*");
            value              = highlightRegex.Replace(value, "__");
            value              = htmlTagRegex.Replace(value, "");

            return value;
        }

        private async Task<KVObject> GetKVObjectFromUri(string uri)
        {
            var rawString = await _httpClient.GetStringAsync(uri);
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(rawString));
            return _kvSerializer.Deserialize(stream, new KVSerializerOptions() { HasEscapeSequences = true });
        }

        private string GetAbilityValue(string language, string internalName, string? postfix = null)
        {
            var key = (language, Key: $"DOTA_Tooltip_ability_{internalName}{(postfix != null ? $"_{postfix}" : "")}");
            if (_abilityValues.ContainsKey(key))
            {
                return _abilityValues[key];
            }
            else
            {
                _abilityValues.TryGetValue((_sourceDefaultLanguage, key.Key), out var value);
                return value ?? "";
            }
        }

        private IEnumerable<string> GetAbilityNoteValues(string language, string internalName)
        {
            var noteRegex = new Regex($"DOTA_Tooltip_ability_{internalName}_Note\\d+");
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

        private string GetHeroValue(string language, string internalName, string? postfix = null)
        {
            var key = (language, Key: $"{internalName}{(postfix != null ? $"_{postfix}" : "")}");
            if (_dotaValues.ContainsKey(key))
            {
                return _dotaValues[key];
            }
            else if (_heroLoreValues.ContainsKey(key))
            {
                return _heroLoreValues[key];
            }
            else
            {
                if (!_dotaValues.TryGetValue((_sourceDefaultLanguage, key.Key), out var value))
                {
                    _heroLoreValues.TryGetValue((_sourceDefaultLanguage, key.Key), out value);
                }
                return value ?? "";
            }
        }

        private IEnumerable<Ability> GetHeroAbilities(string language, KVObject kvHero)
        {
            var abilityRegex       = new Regex("Ability\\d+");
            var hiddenOrEmptyRegex = new Regex("([\\w]+_empty\\d*)|([\\w]+_hidden\\d*)");
            var abilityNames       = kvHero.Children.Where(x => abilityRegex.IsMatch(x.Name)).Select(x => x.Value.ToString());
            var abilities          = new List<Ability>();
            foreach (var name in abilityNames)
            {
                if (string.IsNullOrEmpty(name)  || name == "special_bonus_attributes" || hiddenOrEmptyRegex.IsMatch(name))
                    continue;
                var ability = _abilities.FirstOrDefault(x => x.InternalName == name && x.Language == language);
                if (ability != null)
                    abilities.Add(ability);
            }
            return abilities;
        }

        private IEnumerable<Talent> GetHeroTalents(string language, KVObject kvHero)
        {
            var abilityRegex       = new Regex("Ability\\d+");
            var hiddenOrEmptyRegex = new Regex("([\\w]+_empty\\d*)|([\\w]+_hidden\\d*)");
            var talentNames        = kvHero.Children.Where(x => abilityRegex.IsMatch(x.Name)).Select(x => x.Value.ToString());
            var talents            = new List<Talent>();
            foreach (var name in talentNames)
            {
                if (string.IsNullOrEmpty(name) || name == "special_bonus_attributes" || hiddenOrEmptyRegex.IsMatch(name))
                    continue;
                var talent = _talents.FirstOrDefault(x => x.InternalName == name && x.Language == language);
                if (talent != null)
                    talents.Add(talent);
            }
            return talents;
        }
    }
}