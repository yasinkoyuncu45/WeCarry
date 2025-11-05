using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeCarry.Models.MVVM
{
    public class Message
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageID { get; set; }

        [Required]
        public int ConversationID { get; set; }

        [Required]
        public int SenderUserID { get; set; }

        [ForeignKey(nameof(SenderUserID))]
        public User? SenderUser { get; set; }


        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false;

        // Nav prop
        public Conversation? Conversation { get; set; }
    }
}
