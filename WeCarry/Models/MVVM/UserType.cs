using System.ComponentModel.DataAnnotations;

namespace WeCarry.Models.MVVM
{
    public class UserType
    {
        [Key]
        public int UserTypeID { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = null!;  // Örn: "Taşıyıcı", "Müşteri", "Admin" vb.

        public ICollection<User> Users { get; set; }
    }
}
