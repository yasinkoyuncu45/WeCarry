using WeCarry.Models.MVVM;

namespace WeCarry.Models
{
    public interface ICityRepository
    {
        Task<List<City>> GetAllAsync();
    }
}
