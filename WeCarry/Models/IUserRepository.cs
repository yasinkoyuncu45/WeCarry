using WeCarry.Models.MVVM;

namespace WeCarry.Models
{
    public interface IUserRepository
    {
        Task<bool> RegistrationCheck(string email, string telephone);
      
        Task<(int UserId, string Name, string UserType, bool IsActive)> LoginCheckAsync(string email, string password);
        bool AddUser(User user); 
        Task<bool> DeleteUser(int id); // kullanıcıyı pasif yapar
        Task<List<User>> GetAllAsync();   // tüm kullanıcıları getir
        Task<User?> GetByIdAsync(int id); // tek kullanıcıyı getir
        Task<bool> UpdateAsync(User user);
        User GetByActivationCode(string code);
        void Update(User user);
        Task<bool> UpdateProfileAsync(int userId, string firm, string telephone, string oldPassword, string? newPassword);// profıl gunnceleme için
    }
}
