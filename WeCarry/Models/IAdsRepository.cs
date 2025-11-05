using WeCarry.Models.MVVM;
namespace WeCarry.Models
{
    public interface IAdsRepository
    {
        Task<Ads> GetByIdAsync(int id);
        public IQueryable<Ads> GetAll();
        void AdsCreate(Ads ads, string userType, int userId);

        Task<bool> UpdateAsync(Ads ad);
        Task<bool> DeleteAsync(int id);
        Task<List<Ads>> GetByUserIdAsync(int userId);// ilanlarım için tek kullanıcı


    }
}
