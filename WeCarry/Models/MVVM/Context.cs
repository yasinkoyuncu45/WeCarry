using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace WeCarry.Models.MVVM
{
    public class Context:DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
        public DbSet<User> User { get; set; }
        public DbSet<UserType> UserType { get; set; }
        public DbSet<ServiceType> ServiceType { get; set; }
        public DbSet<Ads> Ads { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ads → User ilişkisinde Cascade kapat
            modelBuilder.Entity<Ads>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Ads → ServiceType ilişkisinde Cascade kapat
            modelBuilder.Entity<Ads>()
                .HasOne(a => a.ServiceType)
                .WithMany()
                .HasForeignKey(a => a.ServiceTypeID)
                .OnDelete(DeleteBehavior.Restrict);
            // Aynı ilan + aynı iki kullanıcı için yalnızca 1 sohbet
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => new { c.AdID, c.StarterUserID, c.OwnerUserID })
                .IsUnique();

            // Mesajları tarihe göre hızlı çekmek için
            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ConversationID, m.CreatedAt });

            // (Opsiyonel) İlişkiler:
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationID)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }


}

