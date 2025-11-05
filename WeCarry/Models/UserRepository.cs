using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeCarry.Models.MVVM;

namespace WeCarry.Models
{
    public class UserRepository : IUserRepository
    {
        private readonly Context _context; // Veritabanı bağlantısı (EF Core)
        private readonly IPasswordHasher<User> _hasher; // Şifre güvenliği için

        // Constructor — DI (Dependency Injection) ile context ve hasher alınır
        public UserRepository(Context context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // -------------------------------------------------------
        // TÜM KULLANICILARI GETİR (Admin paneli için)
        // -------------------------------------------------------
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.User.ToListAsync();
        }

        // -------------------------------------------------------
        // TEK KULLANICI GETİR (ID'ye göre)
        // -------------------------------------------------------
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.User.FindAsync(id);
        }

        // -------------------------------------------------------
        // KAYIT KONTROLÜ (Email veya Telefon zaten var mı?)
        // -------------------------------------------------------
        public async Task<bool> RegistrationCheck(string email, string telephone)
        {
            email = email.Trim().ToLowerInvariant(); // normalize et
            return await _context.User
                .AnyAsync(u => u.Email == email || u.Telephone == telephone);
        }

        // -------------------------------------------------------
        // GİRİŞ KONTROLÜ (Email + Şifre)
        // -------------------------------------------------------
        public async Task<(int UserId, string Name, string UserType, bool IsActive)> LoginCheckAsync(string email, string password)
        {
            var normalized = email.Trim().ToLower();

            // Email'e göre kullanıcıyı bul
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);
            if (user is null)
                return (0, null, null, false);

            // Şifreyi kontrol et
            var result = _hasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
                return (0, null, null, false);

            // Kullanıcı tipi belirle
            string role = user.UserTypeID switch
            {
                1 => "Admin",
                2 => "Tır Sürücüsü",
                3 => "Çekici Sürücüsü",
                4 => "Kullanıcı",
                _ => "Kullanıcı"
            };

            // Gerekirse şifreyi yeniden hashle (güvenlik için)
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var tracked = await _context.User.FirstAsync(x => x.UserID == user.UserID);
                tracked.Password = _hasher.HashPassword(tracked, password);
                await _context.SaveChangesAsync();
            }

            return (user.UserID, user.Name, role, user.Active);
        }

        // -------------------------------------------------------
        // YENİ KULLANICI EKLE (Kayıt Ol)
        // -------------------------------------------------------
        public bool AddUser(User user)
        {
            try
            {
                // Email formatını normalize et
                user.Email = user.Email?.Trim().ToLowerInvariant();

                // Kullanıcı tipi belirle (taşıyıcı mı, normal kullanıcı mı)
                user.UserTypeID = user.ServiceTypeID switch
                {
                    1 => 2, // Tır Sürücüsü
                    2 => 3, // Çekici Sürücüsü
                    _ => 4  // Normal kullanıcı
                };

                // Şifreyi hashle (güvenli kayıt)
                user.Password = _hasher.HashPassword(user, user.Password);

                _context.User.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                // Veritabanı ekleme hatası (örnek: duplicate key)
                return false;
            }
            catch
            {
                // Beklenmeyen hata
                return false;
            }
        }

        // -------------------------------------------------------
        // KULLANICIYI PASİF HALE GETİR (Silmek yerine)
        // -------------------------------------------------------
        public async Task<bool> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return false;

            user.Active = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        // -------------------------------------------------------
        // KULLANICIYI GÜNCELLE (Admin paneli)
        // -------------------------------------------------------
        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                var dbUser = await _context.User.FindAsync(user.UserID);
                if (dbUser == null) return false;

                // Şifre hariç tüm bilgileri güncelle
                dbUser.Name = user.Name;
                dbUser.Surname = user.Surname;
                dbUser.Email = user.Email;
                dbUser.Telephone = user.Telephone;
                dbUser.Address = user.Address;
                dbUser.Firm = user.Firm;
                dbUser.UserTypeID = user.UserTypeID;
                dbUser.ServiceTypeID = user.ServiceTypeID;
                dbUser.Active = user.Active;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // -------------------------------------------------------
        // PROFİL GÜNCELLE (Kullanıcı kendi sayfasından)
        // -------------------------------------------------------
        public async Task<bool> UpdateProfileAsync(int userId, string firm, string telephone, string oldPassword, string? newPassword)
        {
            try
            {
                var dbUser = await _context.User.FindAsync(userId);
                if (dbUser == null) return false;

                dbUser.Firm = firm;
                dbUser.Telephone = telephone;

                // Eğer yeni şifre girilmişse kontrol et
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    // Eski şifre doğru mu?
                    var result = _hasher.VerifyHashedPassword(dbUser, dbUser.Password, oldPassword);
                    if (result == PasswordVerificationResult.Failed)
                        return false;

                    // Yeni şifreyi hashle
                    dbUser.Password = _hasher.HashPassword(dbUser, newPassword);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // -------------------------------------------------------
        // AKTİVASYON KODUYLA KULLANICI GETİR
        // -------------------------------------------------------
        public User? GetByActivationCode(string code)
        {
            return _context.User.FirstOrDefault(u => u.ActivationCode == code);
        }

        // -------------------------------------------------------
        // KULLANICIYI GÜNCELLE (Direkt Update metodu)
        // -------------------------------------------------------
        public void Update(User user)
        {
            _context.User.Update(user);
            _context.SaveChanges();
        }
    }
}
