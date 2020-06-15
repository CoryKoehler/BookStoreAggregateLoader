using System.Collections.Generic;

namespace BookStoreAggregateLoader.LegacyDb
{
    public class Book
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public int Edition { get; set; }

        public int AuthorId { get; set; }
        public Author Author { get; set; }

        public int PublisherId { get; set; }
        public Publisher Publisher { get; set; }
        public List<OrderedItem> OrderedItems { get; set; }
    }
}
