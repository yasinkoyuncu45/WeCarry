using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeCarry.Models.MVVM
{
    public enum ConversationStatus
    {
        Active = 0,   // Devam eden sohbet
        Ended = 1     // Sonlandırılmış sohbet (arşiv)
    }
   
    public class Conversation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConversationID { get; set; }

        [Required]
        public int AdID { get; set; }            // Hangi ilan için konuşuluyor

        [Required]
        public int StarterUserID { get; set; }   // Sohbeti başlatan kullanıcı

        [Required]
        public int OwnerUserID { get; set; }     // İlan sahibi

        [Required]
        public ConversationStatus Status { get; set; } = ConversationStatus.Active;

        public int? EndedByUserID { get; set; }  // Kim sonlandırdı (isteğe bağlı)

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Ads? Ad { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        [ForeignKey(nameof(StarterUserID))]
        public User StarterUser { get; set; }

        [ForeignKey(nameof(OwnerUserID))]
        public User OwnerUser { get; set; }
    }
}

