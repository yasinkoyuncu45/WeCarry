namespace WeCarry.Models.MVVM
{
    public class ServiceType
    {
        public int? ServiceTypeID { get; set; }
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
