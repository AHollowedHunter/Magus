namespace Magus.DataBuild.Models
{
    public record RawPatchNote
    {
        public string patch_number { get; set; }
        public string patch_name { get; set; }
        public int patch_timestamp { get; set; }
        public IList<Generic> generic { get; set; }
        public IList<Items> items { get; set; }
        public IList<Items> neutral_items { get; set; }
        public IList<Heroes> heroes { get; set; }
        public bool success { get; set; }

        public record Generic
        {
            public int indent_level { get; set; }
            public string note { get; set; }

        }

        public record Ability_notes
        {
            public int indent_level { get; set; }
            public string note { get; set; }

        }

        public record Items
        {
            public int ability_id { get; set; }
            public IList<Ability_notes> ability_notes { get; set; }

        }

        public record Abilities
        {
            public int ability_id { get; set; }
            public IList<Ability_notes> ability_notes { get; set; }

        }

        public record Heroes
        {
            public int hero_id { get; set; }
            public IList<Abilities>? abilities { get; set; }
            public IList<Hero_notes>? hero_notes { get; set; }
            public IList<Talent_notes>? talent_notes { get; set; }

        }

        public record Hero_notes
        {
            public int indent_level { get; set; }
            public string note { get; set; }

        }

        public record Talent_notes
        {
            public int indent_level { get; set; }
            public string note { get; set; }

        }
    }
}
