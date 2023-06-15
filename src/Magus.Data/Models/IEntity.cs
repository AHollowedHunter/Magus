namespace Magus.Data.Models
{
    internal interface IEntity
    {
        public int EntityId { get; set; }
        public string InternalName { get; set; }

    }
}
