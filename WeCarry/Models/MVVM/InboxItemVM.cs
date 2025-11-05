using System;

namespace WeCarry.Models.MVVM
{
    public class InboxItemVM
    {
        public int ConversationID { get; set; }
        public string OtherUserName { get; set; } = "";
        public string AdTitle { get; set; } = "";
        public DateTime LastMessageAt { get; set; }
        public int Unread { get; set; } // okunmamış sayısı

        // ✅ Sohbet bitmiş mi?
        public bool IsEnded { get; set; }
    }
}
