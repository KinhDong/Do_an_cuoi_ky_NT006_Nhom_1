using Godot;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Threading.Tasks;
using System.IO;
using System;

namespace NT106.Scripts.Services
{
    public static class CloudinaryService
    {

        private readonly static System.Net.Http.HttpClient http = new();

        static Account account = new Account(
            "dwy5bb7gl", // Cloud name
            "784683322925125", // Cloud API Key
            "EJyM1WzLBImknGan3OsAsmw7-Ns" // Cloud API Secret
        );

        private readonly static Cloudinary _cloudinary = new(account);        

        /// Xóa ảnh cũ dựa trên userId
        private static void DeleteOldImageBeforeUpload(string Uid)
        {
            try
            {
                var deletionParams = new DeletionParams($"avatar/{Uid}")
                {
                    ResourceType = ResourceType.Image
                };
                _cloudinary.Destroy(deletionParams);
            }
            catch
            {                
            }
        }

        // Up ảnh mới
        public static string UploadImage(string filePath, string Uid)
        {
            // Xóa ảnh cũ
            DeleteOldImageBeforeUpload(Uid);

            // Upload ảnh mới (ghi đè nếu trùng)
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(filePath),
                Folder = "avatar",       // Lưu vào thư mục avatar
                PublicId = Uid,        // Đặt tên ảnh theo userId
                Overwrite = true,         // Ghi đè nếu trùng
                Invalidate = true         // Làm mới cache CDN
            };

            var uploadResult = _cloudinary.Upload(uploadParams);

            // Trả về URL HTTPS an toàn
            return uploadResult.SecureUrl.ToString();
        }

        // Copy ảnh
        public static void CopyImage(string publicId)
        {
            string sourceUrl = "https://res.cloudinary.com/dwy5bb7gl/image/upload/v1761364135/AccountIcon_vv8dje.jpg"; // Avatar mặc định có sẵn trên Cloudinary

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(sourceUrl),
                PublicId = publicId,
                Folder = "Avatar" // Thư mục trên cloudinary
            };

            _cloudinary.Upload(uploadParams);
        }

        // Lấy ảnh
        public static async Task<Texture2D> GetImageAsync(string publicId)
        {
            try
            {
                // Sử dụng code Cloudinary của bạn
                var getResourceParams = new GetResourceParams(publicId);
                var result = await _cloudinary.GetResourceAsync(getResourceParams);

                // Lấy URL ảnh
                string imageUrl = result.Url;

                // Tải ảnh từ URL
                using System.Net.Http.HttpClient httpClient = new();
                using var response = await httpClient.GetAsync(imageUrl);
                using var stream = await response.Content.ReadAsStreamAsync();

                // Chuyển đổi stream thành Texture2D
                return await ConvertStreamToTexture(stream);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Lỗi tải ảnh từ Cloudinary: {ex.Message}");
                return null;
            }
        }
        
        private static async Task<Texture2D> ConvertStreamToTexture(Stream stream)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                var image = new Image();
                
                // Thử các định dạng ảnh phổ biến
                if (image.LoadPngFromBuffer(imageData) == Godot.Error.Ok ||
                    image.LoadJpgFromBuffer(imageData) == Godot.Error.Ok ||
                    image.LoadWebpFromBuffer(imageData) == Godot.Error.Ok)
                {
                    return ImageTexture.CreateFromImage(image);
                }
                
                GD.PrintErr("Không thể decode ảnh từ stream");
                return null;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Lỗi chuyển đổi stream: {ex.Message}");
                return null;
            }
        }
    }
}
