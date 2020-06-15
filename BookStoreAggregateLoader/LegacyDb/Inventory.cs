namespace BookStoreAggregateLoader.LegacyDb
{
    public class Inventory
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int StockLevelUsed { get; set; }
        public int StockLevelNew { get; set; }
    }
}
