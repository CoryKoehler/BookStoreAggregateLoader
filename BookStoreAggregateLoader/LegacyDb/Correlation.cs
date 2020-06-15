using System;

namespace BookStoreAggregateLoader.LegacyDb
{
    public class Correlation
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int LegacyId { get; set; }
        public Guid AggregateId { get; set; }
    }
}
