namespace ShopNetApi.Services.Interfaces
{
    public interface IOtpService
    {
        /// <summary>
        /// Tạo mã OTP ngẫu nhiên, lưu vào Redis và trả về mã OTP (để gửi mail)
        /// </summary>
        Task<string> GenerateAndStoreAsync(string email, string fullName);

        /// <summary>
        /// Kiểm tra mã OTP người dùng nhập vào có khớp với cache không
        /// </summary>
        Task<OtpVerifyResult?> VerifyAsync(string email, string otp);
    }

    public class OtpVerifyResult
    {
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
    }
}
