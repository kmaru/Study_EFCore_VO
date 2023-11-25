using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DddValueObjectEF.Infra;

namespace DddValueObjectEF
{
    public class SampleDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public DbSet<UserExtAttr> UserAttrs => Set<UserExtAttr>();

        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
              .HasKey(x => x.Id);

            modelBuilder.Entity<User>()
              .Property(x => x.Id);

            modelBuilder.Entity<User>()
              .Property(x => x.Name)
              .HasMaxLength(100);

            modelBuilder.Entity<User>()
              .Property(x => x.Age);

            modelBuilder.Entity<User>()
              .Property(x => x.Email);

            modelBuilder.Entity<UserExtAttr>()
                .HasKey(x => x.UserId);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<UserId>()
                .HaveStronglyTypedValueConversion();
            configurationBuilder
                .Properties<UserName>()
                .HaveStronglyTypedValueConversion();
            configurationBuilder
                .Properties<UserAge>()
                .HaveStronglyTypedValueConversion();
            configurationBuilder
                .Properties<Email>()
                .HaveStronglyTypedValueConversion();
        }
    }
}
