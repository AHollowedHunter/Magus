namespace Magus.Data.Models.Dota
{
    public record Talent
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public string InternalName { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public AbilityType AbilityType { get; set; }
        public AbilityBehavior AbilityBehavior { get; set; }
        /// <summary>
        /// Level is which XP level the talent is for
        /// Side is the side it appears, 0 is right, 1 is left (Thanks volvo for right talent first)
        /// </summary>
        public (byte Level, byte Side) Position { get; set; }

        public string? AdLinkedAbilities { get; set; } // Sometimes set here...

        public IEnumerable<TalentValue> TalentValues { get; set; }

        public record TalentValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string? AdLinkedAbilities { get; set; }
        }


        // for empty talents, in abilities scan for talent name, then add the value if missing
    }
}
