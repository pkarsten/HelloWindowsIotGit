using SQLite.Net.Attributes;

namespace UwpSqLiteDal
{
    public sealed class PurchTask
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Subject { get; set; }
        public string BodyText{ get; set; }
    }
}
