using Discord;
using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.DataBuild.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Magus.DataBuilder
{
    class Program
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "MAGUS_")
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .Build();

        private static readonly ServiceProvider services = ConfigureServices();
        private static readonly IDatabaseService db = services.GetRequiredService<IDatabaseService>();

        private static HttpClient client = new()
        {
            BaseAddress = new Uri("https://www.dota2.com/datafeed/"),
        };

        private static string language = "english";
        private static Regex xmlTag = new Regex(@"<[^>]*>");

        public static async Task Main()
        {
            var patchNoteUpdater = services.GetRequiredService<PatchNoteUpdater>();
            await patchNoteUpdater.Update();
            //UpdatePatchNotes();

            //UpdateHeroes();
            //UpdateItems();

            db.Dispose();
        }

        static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton<IDatabaseService>(x => new LiteDBService(configuration.GetSection("DatabaseService")))
                .AddSingleton(x => new HttpClient())
                .AddSingleton(x => new PatchNoteUpdater(x.GetRequiredService<IDatabaseService>(), x.GetRequiredService<IConfiguration>(), x.GetRequiredService<HttpClient>()))
                .BuildServiceProvider();

        public static void UpdatePatchNotes()
        {

            db.DeleteCollection<Patch>();
            db.DeleteCollection<Data.Models.Embeds.GeneralPatchNoteEmbed>();
            db.DeleteCollection<Data.Models.Embeds.HeroPatchNoteEmbed>();
            db.DeleteCollection<Data.Models.Embeds.ItemPatchNoteEmbed>();

            var patchList = client
                .GetFromJsonAsync<JsonElement>($"patchnoteslist?language={language}")
                .Result
                .GetProperty("patches");

            var generalPatchNotes = new List<Data.Models.Embeds.GeneralPatchNoteEmbed>();
            var heroPatchNotes = new List<Data.Models.Embeds.HeroPatchNoteEmbed>();
            var itemPatchNotes = new List<Data.Models.Embeds.ItemPatchNoteEmbed>();


            var heroList = client.GetFromJsonAsync<JsonElement>($"herolist?language={language}").Result;
            var heroData = heroList.GetProperty("result").GetProperty("data").GetProperty("heroes").Deserialize<IEnumerable<Hero>>();
            var abilityList = client.GetFromJsonAsync<JsonElement>($"abilitylist?language={language}").Result;
            var abilityData = abilityList.GetProperty("result").GetProperty("data").GetProperty("itemabilities").Deserialize<IEnumerable<Ability>>();
            var itemList = client.GetFromJsonAsync<JsonElement>($"itemlist?language={language}").Result;
            var itemData = itemList.GetProperty("result").GetProperty("data").GetProperty("itemabilities").Deserialize<IEnumerable<Item>>();

            foreach (var patch in patchList.EnumerateArray())
            {
                var patchData = patch.Deserialize<Patch>();
                if (patchData == null)
                {
                    Console.WriteLine("Error deserializing patch: {}", patch);
                    return;
                }
                patchData.Id = (ulong)patchData.PatchTimestamp;
                db.InsertRecord(patchData);

                var patchNoteData = client
                    .GetFromJsonAsync<RawPatchNote>($"patchnotes?language={language}&version={patchData.PatchNumber}")
                    .Result;
                if (patchNoteData == null || patchNoteData.success == false)
                {
                    Console.WriteLine("Error retrieving RawPatchNote: {}", patchData.PatchNumber);
                    return;
                }

                generalPatchNotes.Add(PatchNoteUtils.GetGeneralPatchNote(patchData, patchNoteData));
                heroPatchNotes.AddRange(PatchNoteUtils.GetHeroPatchNotes(patchData, patchNoteData, heroData, abilityData));
                itemPatchNotes.AddRange(PatchNoteUtils.GetItemPatchNotes(patchData, patchNoteData, itemData));

                Console.Write("#");
            }
            db.InsertRecords(generalPatchNotes);
            db.InsertRecords(heroPatchNotes);
            db.InsertRecords(itemPatchNotes);

            db.EnsureIndex<Patch>("PatchNumber");
            db.EnsureIndex<Data.Models.Embeds.GeneralPatchNoteEmbed>("PatchNumber");
            db.EnsureIndex<Data.Models.Embeds.HeroPatchNoteEmbed>("EntityId");
            db.EnsureIndex<Data.Models.Embeds.HeroPatchNoteEmbed>("PatchNumber");
            db.EnsureIndex<Data.Models.Embeds.ItemPatchNoteEmbed>("EntityId");
            db.EnsureIndex<Data.Models.Embeds.ItemPatchNoteEmbed>("PatchNumber");

            Console.WriteLine("\nFinished Patches");
        }

        public static void UpdateHeroes()
        {
            db.DeleteCollection<HeroInfo>();
            db.DeleteCollection<AbilityInfo>();
            db.DeleteCollection<TalentInfo>();

            var heroList = client
                .GetFromJsonAsync<JsonElement>($"herolist?language={language}")
                .Result
                .GetProperty("result")
                .GetProperty("data")
                .GetProperty("heroes");

            var heroes = new List<HeroInfo>();
            var abilities = new List<AbilityInfo>();
            var talents = new List<TalentInfo>();

            foreach (var hero in heroList.EnumerateArray())
            {
                var heroData = client
                    .GetFromJsonAsync<JsonElement>($"herodata?language={language}&hero_id={hero.GetProperty("id")}")
                    .Result
                    .GetProperty("result")
                    .GetProperty("data")
                    .GetProperty("heroes")[0]
                    .Deserialize<Hero>();
                if (heroData == null)
                    return;

                var latestPatch = db.GetLatestPatch();

                var urlRegex = new Regex(@"[^a-zA-Z0-9-']");
                var heroUrl = $"https://www.dota2.com/hero/{urlRegex.Replace(heroData.LocalName.ToLower(), "")}";

                var heroInfoEmbed = new Data.Models.Embeds.Embed()
                {
                    Title = heroData.LocalName,
                    Description = heroData.LocalNpeDesc,
                    Url = heroUrl,
                    ColorRaw = 0X00A84300,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.PatchTimestamp),
                    ThumbnailUrl = $"{DotaUrls.Hero}{heroData.InternalName.Substring(14)}.png",
                    Footer = new() { Text = $"Patch {latestPatch.PatchNumber}" },
                };

                List<Field> heroInfoFields = new();

                // Attributes
                heroInfoFields.Add(new()
                {
                    Name = $"{Emotes.StrengthIcon} Strength {(heroData.PrimaryAttribute == PrimaryAttribute.Strength ? "⭐" : "")}",
                    Value = $"{heroData.StrengthBase} +{heroData.StrengthGain}",
                    IsInline = true
                });

                heroInfoFields.Add(new()
                {
                    Name = $"{Emotes.AgilityIcon} Agility {(heroData.PrimaryAttribute == PrimaryAttribute.Agility ? "⭐" : "")}",
                    Value = $" {heroData.AgilityBase} +{heroData.AgilityGain}",
                    IsInline = true
                });
                heroInfoFields.Add(new()
                {
                    Name = $"{Emotes.IntelligenceIcon} Intelligence {(heroData.PrimaryAttribute == PrimaryAttribute.Intelligence ? "⭐" : "")}",
                    Value = $"{heroData.IntelligenceBase} +{heroData.IntelligenceGain}",
                    IsInline = true
                });

                //Stats
                heroInfoFields.Add(new()
                {
                    Name = "Attack",
                    Value = $"{Emotes.DamageIcon} {heroData.DamageMin}-{heroData.DamageMax}\n{Emotes.AttackTimeIcon} {heroData.AttackRate}\n{Emotes.AttackRangeIcon} {heroData.AttackRange}\n{heroData.GetAttackType().GetAttackTypeIcon()} {heroData.GetAttackType()}",
                    IsInline = true
                });
                heroInfoFields.Add(new()
                {
                    Name = "Defence",
                    Value = $"{Emotes.ArmourIcon} {heroData.Armour.ToString("n1")}\n{Emotes.MagicResistIcon} {heroData.MagicResistance}",
                    IsInline = true
                });
                heroInfoFields.Add(new()
                {
                    Name = "Mobility",
                    Value = $"{Emotes.MoveSpeedIcon} {heroData.MoveSpeed}\n{Emotes.TurnRateIcon} {heroData.TurnRate}\n{Emotes.VisionIcon} {heroData.ViewRangeDay} / {heroData.ViewRangeNight}",
                    IsInline = true
                });

                var abilityValue = "";
                foreach (var ability in heroData.Abilities)
                {
                    if (ability.AbilityIsGrantedByScepter)
                    {
                        abilityValue += ability.LocalName + Emotes.ScepterIcon + "   ";
                    }
                    else if (ability.AbilityIsGrantedByShard)
                    {
                        abilityValue += ability.LocalName + Emotes.ShardIcon + "   ";
                    }
                    else
                    {
                        abilityValue += ability.LocalName + "   ";
                    }
                }
                // Break hero abilities across new lines, except for certain heroes
                if (heroData.LocalName == "Invoker")
                {
                    abilityValue = abilityValue.Trim().Replace("   ", " | ");
                }
                else
                {
                    abilityValue = abilityValue.Trim().Replace("   ", "\n");
                }
                heroInfoFields.Add(new()
                {
                    Name = "Abilities",
                    Value = abilityValue,
                    IsInline = true
                });

                heroInfoFields.Add(new()
                {
                    Name = "Complexity",
                    Value = $"{new string('\u25c6', heroData.Complexity)}{new string('\u25c7', 3 - heroData.Complexity)}",
                    IsInline = true
                });
                var roleValue = "";
                foreach (var role in heroData.GetAllRoles())
                {
                    if (heroData.GetHightestRoles().Contains(role))
                        roleValue += Format.Bold(role.ToString()) + " ";
                    else
                        roleValue += role + " ";
                }
                roleValue = roleValue.Trim().Replace(" ", " | ");
                heroInfoFields.Add(new()
                {
                    Name = "Roles",
                    Value = roleValue,
                    IsInline = true
                });

                heroInfoFields.Add(new()
                {
                    Name = "About",
                    Value = xmlTag.Replace(heroData.LocalHype, "")
                });

                heroInfoEmbed.Fields = heroInfoFields;
                var heroInfo = new HeroInfo()
                {
                    Id = (ulong)heroData.Id,
                    InternalName = heroData.InternalName,
                    LocalName = heroData.LocalName,
                    Embed = heroInfoEmbed,
                };
                heroes.Add(heroInfo);

                //

                foreach (var ability in heroData.Abilities)
                {
                    ability.FormatSpell();
                    // TODO Make ability embed
                    var embed = new Data.Models.Embeds.Embed()
                    {
                        Title = $"{ability.LocalName}",
                        Description = ability.LocalDesc + "\n",
                        ColorRaw = Color.Orange,
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.PatchTimestamp),
                        Footer = new() { Text = $"Patch {latestPatch.PatchNumber}" },
                        ThumbnailUrl = $"{DotaUrls.Ability}{ability.InternalName}.png",
                    };
                    var abilityInfo = new AbilityInfo()
                    {
                        Id = (ulong)ability.Id,
                        InternalName = ability.InternalName,
                        LocalName = ability.LocalName,
                        Embed = embed,
                    };
                    abilities.Add(abilityInfo);
                }

                // TODO Make talent embed?
                foreach (var talent in heroData.Talents)
                {
                    talent.FormatSpell();
                }
                Console.Write("#");
            }

            db.InsertRecords(heroes);
            db.InsertRecords(abilities);
            db.InsertRecords(talents);

            db.EnsureIndex<HeroInfo>("LocalName");
            db.EnsureIndex<HeroInfo>("InternalName");
            db.EnsureIndex<HeroInfo>("RealName");
            db.EnsureIndex<HeroInfo>("Aliases");
            db.EnsureIndex<AbilityInfo>("LocalName");
            db.EnsureIndex<AbilityInfo>("InternalName");

            Console.WriteLine("\nFinished Heroes");
        }

        public static void UpdateItems()
        {
            db.DeleteCollection<ItemInfo>();
            var itemList = client
                .GetFromJsonAsync<JsonElement>($"itemlist?language={language}")
                .Result
                .GetProperty("result")
                .GetProperty("data")
                .GetProperty("itemabilities");

            var items = new List<ItemInfo>();
            var latestPatch = db.GetLatestPatch();

            foreach (var item in itemList.EnumerateArray())
            {
                var itemData = client
                    .GetFromJsonAsync<JsonElement>($"itemdata?language={language}&item_id={item.GetProperty("id")}")
                    .Result
                    .GetProperty("result")
                    .GetProperty("data")
                    .GetProperty("items")[0]
                    .Deserialize<Item>();
                if (itemData == null)
                    return;

                if (itemData.InternalName.StartsWith("item_dagon"))
                {
                    var level = itemData.InternalName == "item_dagon" ? 1 : int.Parse(itemData.InternalName.Substring(itemData.InternalName.Length - 1));
                    itemData.LocalName += $" (Level {level})";

                    foreach (var value in itemData.SpecialValues)
                    {
                        if (value.ValuesFloat != null && value.ValuesFloat.Length == 5) value.ValuesFloat = new float[] { value.ValuesFloat[level - 1] };
                    }
                }
                itemData.FormatSpell();

                var bonuses = "";
                foreach (var bonus in itemData.GetItemBonusValues())
                {
                    bonuses += bonus.LocalHeading + "\n";
                }

                var costs = "";
                if (itemData.ManaCosts[0] != 0)
                {
                    costs += $"{Emotes.ManaIcon} {itemData.ManaCosts[0]}{Emotes.Spacer}";
                }
                if (itemData.Cooldowns[0] != 0)
                {
                    costs += $"{Emotes.CooldownIcon} {itemData.Cooldowns[0]}{Emotes.Spacer}";
                }
                if (itemData.ItemCost != 0)
                {
                    costs += $"{Emotes.GoldIcon} {itemData.ItemCost}";
                }

                var localDesc = "\n" + costs + "\n\n" + xmlTag.Replace(itemData.LocalDesc ?? "", "") + "\n";

                var spellValues = "";
                foreach (var spellValue in itemData.GetSpellValues())
                {
                    spellValues += $"\n{spellValue.LocalHeading}";
                }

                var itemInfoEmbed = new Data.Models.Embeds.Embed()
                {
                    Title = itemData.LocalName,
                    Description = bonuses + localDesc + spellValues,
                    ColorRaw = Color.DarkBlue,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.PatchTimestamp),
                    Footer = new() { Text = $"Patch {latestPatch.PatchNumber}" },
                    ThumbnailUrl = $"{DotaUrls.Item}{itemData.InternalName.Substring(5)}.png",
                };
                // Notes
                var fields = new List<Field>();
                if (itemData.LocalNotes != null && itemData.LocalNotes.Any())
                {
                    var notes = "";
                    foreach (var note in itemData.LocalNotes)
                    {
                        notes += note + "\n";
                    }
                    fields.Add(new() { Name = "Notes", Value = xmlTag.Replace(notes, "") });
                }
                // About Row
                if (itemData.LocalLore != null)
                {
                    fields.Add(new() { Name = "About", Value = xmlTag.Replace(itemData.LocalLore, "") });
                }
                itemInfoEmbed.Fields = fields;
                var itemInfo = new ItemInfo()
                {
                    Id = (ulong)itemData.Id,
                    InternalName = itemData.InternalName,
                    LocalName = itemData.LocalName,
                    Embed = itemInfoEmbed,
                };
                items.Add(itemInfo);
                Console.Write("#");
            }

            db.InsertRecords(items);

            db.EnsureIndex<ItemInfo>("LocalName");
            db.EnsureIndex<ItemInfo>("InternalName");
            Console.WriteLine("\nFinished Items");
        }
    }
}