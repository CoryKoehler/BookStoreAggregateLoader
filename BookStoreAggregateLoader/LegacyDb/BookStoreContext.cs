using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace BookStoreAggregateLoader.LegacyDb
{
    public class BookStoreContext : DbContext
    {
        public BookStoreContext(DbContextOptions<BookStoreContext> options) : base(options) { }

        public DbSet<Inventory> StoreInventory { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedItem> OrderedItems { get; set; }
        public DbSet<Correlation> Correlation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>(i =>
            {
                i.HasKey(k => k.Id);
                i.HasOne<Book>().WithMany().HasForeignKey(k => k.BookId);
            });

            modelBuilder.Entity<Author>(a =>
            {
                a.HasKey(k => k.Id);
            });

            modelBuilder.Entity<Publisher>(p =>
            {
                p.HasKey(k => k.Id);
            });

            modelBuilder.Entity<Book>(b =>
            {
                b.HasKey(k => k.Id);
                b.HasOne(a => a.Author).WithMany(a => a.Books).HasForeignKey(k => k.AuthorId);
                b.HasOne(p => p.Publisher).WithMany(p => p.Books).HasForeignKey(k => k.PublisherId);
            });

            modelBuilder.Entity<Customer>(c =>
            {
                c.HasKey(k => k.Id);
            });


            modelBuilder.Entity<Order>(o =>
            {
                o.HasKey(k => k.Id);
                o.HasOne(_ => _.Customer).WithMany(_ => _.Orders).HasForeignKey(k => k.CustomerId);
            });

            modelBuilder.Entity<OrderedItem>(oi =>
            {
                oi.HasKey(k => new { k.OrderId, k.BookId });
                oi.HasOne(_ => _.Order).WithMany(_ => _.OrderedItems).HasForeignKey(o => o.OrderId);
                oi.HasOne(_ => _.Book).WithMany(_ => _.OrderedItems).HasForeignKey(b => b.BookId);
            });

            modelBuilder.Entity<Correlation>(c =>
            {
                c.HasKey(k => k.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BookStoreContext>
    {
        public BookStoreContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("pocConnectionString");

            var builder = new DbContextOptionsBuilder<BookStoreContext>();

            if (connectionString != null)
            {
                builder.UseSqlServer(connectionString);
            }
            else
            {
                builder.UseSqlServer("Server=tcp:concept-proofing-sqlserver.database.windows.net,1433;Initial Catalog=concept-proofing-sql;Persist Security Info=False;User ID=cory;Password=qucgA7eNBiMn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                //builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=concept-proofing-sql;Trusted_Connection=true;MultipleActiveResultSets=true");
            }

            return new BookStoreContext(builder.Options);
        }
    }
}
