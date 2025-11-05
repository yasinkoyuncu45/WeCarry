using Microsoft.EntityFrameworkCore;
using WeCarry.Models.MVVM;

namespace WeCarry.Models
{
    public class AdsRepository : IAdsRepository
    {
        private readonly Context _context; // Veritabanı bağlantısı (EF Core Context)

        // Constructor — DI (Dependency Injection) ile context alınır
        public AdsRepository(Context context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ------------------------------------------------------------
        // KULLANICIYA AİT TÜM İLANLARI GETİR (İlanlarım sayfası)
        // ------------------------------------------------------------
        public async Task<List<Ads>> GetByUserIdAsync(int userId)
        {
            return await _context.Ads
                .Include(a => a.ServiceType)         // Hizmet türü bilgisiyle birlikte getir
                .Where(a => a.UserID == userId)      // Sadece bu kullanıcıya ait ilanlar
                .OrderByDescending(a => a.CreatedAt) // Yeni ilanlar en üstte
                .ToListAsync();
        }

        // ------------------------------------------------------------
        // TÜM İLANLARI GETİR (Ana sayfa veya admin paneli)
        // ------------------------------------------------------------
        public IQueryable<Ads> GetAll()
        {
            return _context.Ads
                .Include(a => a.User)         // İlan sahibini de dahil et
                .Include(a => a.ServiceType); // Hizmet türünü dahil et
        }

        // ------------------------------------------------------------
        // TEK İLAN GETİR (İlan Detay Sayfası)
        // ------------------------------------------------------------
        public async Task<Ads?> GetByIdAsync(int id)
        {
            return await _context.Ads
                .Include(a => a.User)
                .Include(a => a.ServiceType)
                .FirstOrDefaultAsync(a => a.AdsID == id); // ID'ye göre tek kayıt döndür
        }

        // ------------------------------------------------------------
        // YENİ İLAN OLUŞTUR (Taşıyıcı ilan ekler)
        // ------------------------------------------------------------
        public void AdsCreate(Ads ads, string userType, int userId)
        {
            ads.IsActive = true;              // Varsayılan olarak aktif
            ads.CreatedAt = DateTime.Now;     // İlan tarihi
            ads.UserID = userId;              // Giriş yapan kullanıcının ID’si atanır

            _context.Ads.Add(ads);
            _context.SaveChanges();           // Senkron, küçük işlemlerde sorun olmaz
        }

        // ------------------------------------------------------------
        // İLAN GÜNCELLEME (İlan Düzenleme Sayfası)
        // ------------------------------------------------------------
        public async Task<bool> UpdateAsync(Ads ad)
        {
            try
            {
                // Güncellenecek ilanı veritabanından bul
                var existingAd = await _context.Ads.FirstOrDefaultAsync(a => a.AdsID == ad.AdsID);
                if (existingAd == null)
                    return false;

                // Sadece izin verilen alanlar güncelleniyor
                existingAd.ServiceTypeID = ad.ServiceTypeID;
                existingAd.FoundCity = ad.FoundCity;
                existingAd.DestinationCity = ad.DestinationCity;
                existingAd.KmFee = ad.KmFee;
                existingAd.AdvertisementText = ad.AdvertisementText;
                existingAd.IsActive = ad.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // TODO: Logger eklenebilir (örneğin ILogger<AdsRepository>)
                throw; // hatayı kontrol katmanına fırlat
            }
        }

        // ------------------------------------------------------------
        // İLAN SİLME (Aslında pasifleştiriyor — Soft Delete)
        // ------------------------------------------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var ads = await _context.Ads.FindAsync(id);
                if (ads == null)
                    return false;

                ads.IsActive = false; // Gerçekten silmiyoruz, sadece pasif hale getiriyoruz
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw; // Hata üst katmanda yakalanabilir (Controller’da)
            }
        }
    }
}
