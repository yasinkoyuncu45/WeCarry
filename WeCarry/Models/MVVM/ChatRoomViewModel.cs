namespace WeCarry.Models.MVVM
{
    public class MessageItemVM
    {
        public int MessageID { get; set; }
        public int SenderUserID { get; set; }
        public string SenderName { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class ChatRoomViewModel
    {
        public int ConversationID { get; set; }
        public int MeUserID { get; set; }
        public int OtherUserID { get; set; }
        public string OtherUserName { get; set; } = "";
        public int AdID { get; set; }
        public string AdTitle { get; set; } = ""; // Ads’te Title yoksa kısa metin
        public bool IsEnded { get; set; }
        public List<MessageItemVM> Messages { get; set; } = new();
    }

   
}
