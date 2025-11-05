using Microsoft.EntityFrameworkCore;
using WeCarry.Models.MVVM;

namespace WeCarry.Models
{
    public class CityRepository : ICityRepository
    {
        private readonly Context _con;

        public CityRepository(Context con)
        {
            _con = con ?? throw new ArgumentNullException(nameof(con));
        }

        // ------------------------------------------------------------
        // 1️⃣ Tüm şehirleri getirir (A'dan Z'ye sıralı) — Asenkron versiyon
        // ------------------------------------------------------------
        public async Task<List<City>> GetAllAsync()
        {
            try
            {
                // Veritabanından şehirleri çekip alfabetik sırala
                return await _con.City
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
            }
            catch (Exception ex)
            {
                // Hata loglama (ileride ILogger entegre edilebilir)
                Console.WriteLine("❌ CityRepository.GetAllAsync hatası: " + ex.Message);

                // Hata durumunda boş liste döndür
                return new List<City>();
            }
        }
    }
}
