using Godot;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace NT106.Scripts.Services
{
    public class CloudinaryServices
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryServices()
        {
            var account = new CloudinaryDotNet.Account(
                "dwy5bb7gl", // Cloud name
                "784683322925125", // Cloud API Key
                "EJyM1WzLBImknGan3OsAsmw7-Ns" // Cloud API Secret
            );

            _cloudinary = new Cloudinary(account);
        }


        /// Xóa ảnh cũ dựa trên userId
        private void DeleteOldImageBeforeUpload(string Uid)
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
        public string UploadImage(string filePath, string Uid)
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
        public void CopyImage(string publicId)
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
        public async Task<ImageTexture> GetImageAsync(string publicId)
        {
            var getResourceParams = new GetResourceParams(publicId);
            var result = await _cloudinary.GetResourceAsync(getResourceParams);

            // Lấy URL ảnh
            string imageUrl = result.Url;

            // Tải ảnh từ URL
            using var httpClient = new System.Net.Http.HttpClient();
            using var response = await httpClient.GetAsync(imageUrl);
            using var stream = await response.Content.ReadAsStreamAsync();

            // Chuyển sang ImageTexture
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                byte[] data = ms.ToArray();

                Image image = new Image();
                
                return ImageTexture.CreateFromImage(image);                
            }
        }
    }
}
