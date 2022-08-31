namespace Magus.Data.Models.Dota
{
    public record Ability : BaseSpell
    {
        public bool AbilityHasScepter { get; set; }
        public bool AbilityHasShard { get; set; }
        public bool AbilityIsGrantedByScepter { get; set; }
        public bool AbilityIsGrantedByShard { get; set; }

        public string? ScepterDescription { get; set; }
        public string? ShardDescription { get; set; }

        public IDictionary<string, Talent>? LinkedTalents { get; set; } = new Dictionary<string, Talent>();

        public IEnumerable<AbilityValue> GetAbilityValues()
        {
            var spellValues = new List<AbilityValue>();

            foreach (var value in AbilityValues.Where(x => !x.RequiresShard == true && !x.Name.StartsWith("shard_") && !x.RequiresScepter == true && !x.Name.StartsWith("scepter_")))
            {
                if (!string.IsNullOrEmpty(value.Description) && !(value.Description.StartsWith('+') || value.Description.StartsWith('-')))
                {
                    spellValues.Add(value);
                }
            }

            return spellValues;
        }

        public IEnumerable<AbilityValue> GetShardValues()
        {
            var upgradeValues = new List<AbilityValue>();
            if (!AbilityHasShard)
                return upgradeValues;

            return AbilityValues.Where(x => x.RequiresShard == true || x.Name.StartsWith("shard_"));
        }

        public IEnumerable<AbilityValue> GetScepterValues()
        {
            var upgradeValues = new List<AbilityValue>();
            if (!AbilityHasScepter)
                return upgradeValues;

            return AbilityValues.Where(x => x.RequiresScepter == true || x.Name.StartsWith("scepter_"));
        }
    }
}
