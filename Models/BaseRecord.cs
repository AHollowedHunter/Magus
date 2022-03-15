namespace Magus.Data.Models
{
    public abstract record BaseRecord
    {
        public Guid Id { get; set; }
    }
}
