using ForestTails.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForestTails.Server.Data
{
    public class ForestTailsDbContext : DbContext
    {
        public ForestTailsDbContext(DbContextOptions<ForestTailsDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<PlayerStatistics> PlayerStatistics { get; set; }
        public DbSet<SocialNetwork> SocialNetworks { get; set; }
        public DbSet<Sanction> Sanctions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        public DbSet<GameItem> GameItems { get; set; }
        public DbSet<InventorySlot> InventorySlots { get; set; }
        public DbSet<Cosmetic> Cosmetics { get; set; }
        public DbSet<UnlockedCosmetic> UnlockedCosmetics { get; set; }

        public DbSet<VerificationCode> VerificationCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e => {
                e.HasIndex(u => u.Username).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<User>()
                .HasOne(u => u.Statistics).WithOne(s => s.User)
                .HasForeignKey<PlayerStatistics>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friendship>(e => {
                e.HasKey(f => new { f.RequesterId, f.AddresseeId });
                e.HasOne(f => f.Requester).WithMany(u => u.SentFriendRequests)
                    .HasForeignKey(f => f.RequesterId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(f => f.Addressee).WithMany(u => u.ReceivedFriendRequests)
                    .HasForeignKey(f => f.AddresseeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Report>(e => {
                e.HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterUserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Reported).WithMany().HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InventorySlot>()
                .HasIndex(i => new { i.UserId, i.SlotPosition }).IsUnique();

            modelBuilder.Entity<UnlockedCosmetic>(e => {
                e.HasIndex(c => new { c.UserId, c.CosmeticId }).IsUnique();
                e.HasOne(c => c.Cosmetic).WithMany().HasForeignKey(c => c.CosmeticId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VerificationCode>().HasIndex(v => v.Email);
        }
    }
}
