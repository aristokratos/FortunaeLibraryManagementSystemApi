

namespace FortunaeLibraryManagementSystem.Infrastructure.Data
{
    using FortunaeLibraryManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;



    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.Book)
                .WithMany()
                .HasForeignKey(b => b.BookId);

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.Penalty)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Ratings)
                .HasForeignKey(r => r.BookId);
            modelBuilder.Entity<Book>()
                .Property(b => b.AverageRating)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Rating>()
                .Property(r => r.Value)
                .IsRequired();

            modelBuilder.Entity<Rating>()
                .HasCheckConstraint("CHK_Rating_Value", "[Value] BETWEEN 1 AND 5");

            modelBuilder.Entity<Rating>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }

}
