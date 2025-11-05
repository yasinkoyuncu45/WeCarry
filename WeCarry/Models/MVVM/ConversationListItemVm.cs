namespace WeCarry.Models.MVVM
{
    public class ConversationListItemVm
    {
        public int ConversationID { get; set; }
        public int AdID { get; set; } // İlan ID'si

        public string StarterUserName { get; set; } // Sohbeti başlatan kullanıcının adı
        public string OwnerUserName { get; set; } // İlan sahibinin adı

        public DateTime CreatedAt { get; set; } // Konuşmanın başlama zamanı

        public DateTime? EndedAt { get; set; } // Konuşma sonlandıysa bitiş zamanı (opsiyonel)

        public string? EndedByUserName { get; set; } // Konuşmayı sonlandıran kullanıcının adı (opsiyonel)



    }
}
