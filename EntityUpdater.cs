using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.DataBuilder.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly List<Hero> _heroes;
        private readonly List<Item> _items;

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
            _heroes                = new();
            _items                 = new();
        }

        public async Task Update()
        {
            _logger.LogInformation("Starting Entity Update");
            var startTime = DateTimeOffset.Now;
            await SetEntityValues();
            await SetEntities();

            //StorePatchNoteEmbeds();
            var endTime   = DateTimeOffset.Now;
            var timeTaken = endTime-startTime;
            _logger.LogInformation("Finished Entity Update");
            _logger.LogInformation("Time Taken: {0:0.#}s", timeTaken.TotalSeconds);
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

            var abilities = await GetKVObjectFromUri(Dota2GameFiles.NpcAbilities);
            var heroes = await GetKVObjectFromUri(Dota2GameFiles.NpcHeroes);
            var items = await GetKVObjectFromUri(Dota2GameFiles.Items);

            _abilities.Clear();
            _heroes.Clear();
            _items.Clear();

            foreach (var ability in abilities.Children.Where(x => x.Name != "Version"))
            {
                if (ability.Children.Count() == 0) continue;
                _logger.LogDebug("Processing ability {0}", ability.Name);
                foreach (var language in _sourceLocaleMappings.Keys)
                {
                    //_abilities.Add(CreateAbility(language, ability));
                }
            }

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

            _logger.LogInformation("Finished setting entities");
        }

        private Ability CreateAbility(string language, KVObject kvAbility)
        {
            var ability = new Ability();

            ability.Id           = (int)kvAbility.Children.First(x => x.Name == "ID").Value!;
            ability.InternalName = kvAbility.Name;
            ability.Language     = language;
            ability.LocalName    = GetAbilityValue(language, ability.InternalName);
            ability.LocalDesc    = GetAbilityValue(language, ability.InternalName, "Description");
            ability.LocalLore    = GetAbilityValue(language, ability.InternalName, "Lore");
            ability.LocalNotes   = GetAbilityNoteValues(language, ability.InternalName);

            ability.AbilityType           = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityType").ParseEnum<AbilityType>();
            ability.AbilityBehavior       = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityBehavior").ParseEnum<AbilityBehavior>();
            ability.AbilityUnitTargetTeam = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityUnitTargetTeam").ParseEnum<AbilityUnitTargetTeam>();
            ability.AbilityUnitTargetType = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityUnitTargetType").ParseEnum<AbilityUnitTargetType>();
            ability.AbilityUnitDamageType = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityUnitDamageType").ParseEnum<AbilityUnitDamageType>();
            ability.AbilityUnitTargetTeam = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityType").ParseEnum<AbilityUnitTargetTeam>();
            ability.SpellImmunityType     = kvAbility.Children.FirstOrDefault(x => x.Name == "SpellImmunityType").ParseEnum<SpellImmunityType>();
            ability.SpellDispellableType  = kvAbility.Children.FirstOrDefault(x => x.Name == "SpellDispellableType").ParseEnum<SpellDispellableType>();

            ability.AbilityCastRange   = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityCastRange").ParseList<float>();
            ability.AbilityCastPoint   = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityCastPoint").ParseList<float>();
            ability.AbilityChannelTime = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityChannelTime").ParseList<float>();
            ability.AbilityCooldown    = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityCooldown").ParseList<float>();
            ability.AbilityDuration    = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityDuration").ParseList<float>();
            ability.AbilityDamage      = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityDamage").ParseList<float>();
            ability.AbilityManaCost    = kvAbility.Children.FirstOrDefault(x => x.Name == "AbilityManaCost").ParseList<float>();

            //ability.AbilityValues = DO THIS

            _logger.LogTrace("Processed ability {0,-64} in {1}", ability.InternalName, language);
            return ability;
        }

        private Hero CreateHero(string language, KVObject kvhero)
        {
            var hero = new Hero();

            hero.InternalName = kvhero.Name;
            hero.Language     = language;
            hero.Id           = kvhero.ParseChildValue<int>("HeroID");
            hero.Name         = GetHeroValue(language, hero.InternalName);
            hero.NameAliases  = kvhero.ParseChildList<string>("NameAliases");
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
            hero.AttributePrimary          = kvhero.ParseChildValue<AttributePrimary>("AttributePrimary");

            hero.Complexity = kvhero.ParseChildValue<byte>("Complexity");
            hero.Role       = kvhero.ParseChildList<Role>("Role").ToArray();
            hero.Rolelevels = kvhero.ParseChildList<byte>("Rolelevels").ToArray();

            hero.AttackCapabilities   = kvhero.ParseChildValue<AttackCapabilities>("AttackCapabilities");
            hero.AttackDamageMin      = kvhero.ParseChildValue<short>("AttackDamageMin");
            hero.AttackDamageMax      = kvhero.ParseChildValue<short>("AttackDamageMax");
            hero.AttackRate           = kvhero.ParseChildValue<float>("AttackRate", 1.7F);
            hero.BaseAttackSpeed      = kvhero.ParseChildValue<short>("BaseAttackSpeed", 100);
            hero.AttackAnimationPoint = kvhero.ParseChildValue<float>("AttackAnimationPoint");
            hero.AttackRange          = kvhero.ParseChildValue<float>("AttackRange");
            hero.ProjectileSpeed      = kvhero.ParseChildValue<float>("ProjectileSpeed", 900);
            hero.ArmorPhysical        = kvhero.ParseChildValue<short>("ArmorPhysical", -1);
            hero.MagicalResistance    = kvhero.ParseChildValue<short>("MagicalResistance", 25);
            hero.MovementSpeed        = kvhero.ParseChildValue<short>("MovementSpeed");
            hero.MovementTurnRate     = kvhero.ParseChildValue<float>("MovementTurnRate", 0.6F);
            hero.VisionDaytimeRange   = kvhero.ParseChildValue<short>("VisionDaytimeRange", 1800);
            hero.VisionNighttimeRange = kvhero.ParseChildValue<short>("VisionNighttimeRange", 800);
            hero.StatusHealth         = kvhero.ParseChildValue<short>("StatusHealth", 200);
            hero.StatusHealthRegen    = kvhero.ParseChildValue<float>("StatusHealthRegen", 0.25F);
            hero.StatusMana           = kvhero.ParseChildValue<short>("StatusMana", 75);
            hero.StatusManaRegen      = kvhero.ParseChildValue<float>("StatusManaRegen", 0);

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
            var notesCount = _abilityValues.Keys.Where(x => noteRegex.IsMatch(x.Key)).Distinct().Count(); 
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
    }
}