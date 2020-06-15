using System;
using System.Collections.Generic;

namespace BookStoreAggregateLoader.LegacyDb
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public List<OrderedItem> OrderedItems { get; set; }
    }

    public class OrderedItem
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
