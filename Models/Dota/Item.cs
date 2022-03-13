using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota
{
    public record Item : BaseSpell
    {
        //[JsonPropertyName("id")]
        //public int Id { get; set; }
        //[JsonPropertyName("name_loc")]
        //public string LocalName { get; set; }
        //[JsonPropertyName("name")]
        //public string InternalName { get; set; }
        //[JsonPropertyName("neutral_item_tier")]
        //public sbyte NeutralItemTier { get; set; }

        public Item()
        {

        }
    }
}
