using ShopNetApi.Services.Interfaces;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ShopNetApi.Services
{
    public class OtpService : IOtpService
    {
        private readonly IDatabase _db;

        public OtpService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // ===================== HASH OTP =====================
        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(input))
            );
        }

        // ===================== MODEL LƯU REDIS =====================
        private class RegisterOtpCache
        {
            public string OtpHash { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public DateTime CreatedAt { get; set; }
        }

        // ===================== GENERATE + STORE =====================
        public async Task<string> GenerateAndStoreAsync(
            string email,
            string fullName)
        {
            var otp = RandomNumberGenerator
                .GetInt32(100000, 999999)
                .ToString();

            var cache = new RegisterOtpCache
            {
                OtpHash = Hash(otp),
                Email = email,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow
            };

            await _db.StringSetAsync(
                GetKey(email),
                JsonSerializer.Serialize(cache),
                TimeSpan.FromMinutes(5)
            );

            return otp; // gửi email
        }

        // ===================== VERIFY OTP =====================
        public async Task<OtpVerifyResult?> VerifyAsync(
            string email,
            string otp)
        {
            var value = await _db.StringGetAsync(GetKey(email));
            if (value.IsNullOrEmpty)
                return null;

            var cache = JsonSerializer.Deserialize<RegisterOtpCache>(
                value.ToString()
            );

            if (cache == null)
                return null;

            if (cache.OtpHash != Hash(otp))
                return null;

            await _db.KeyDeleteAsync(GetKey(email));

            return new OtpVerifyResult
            {
                Email = cache.Email,
                FullName = cache.FullName
            };
        }

        private static string GetKey(string email)
            => $"otp:register:{email}";
    }


}
