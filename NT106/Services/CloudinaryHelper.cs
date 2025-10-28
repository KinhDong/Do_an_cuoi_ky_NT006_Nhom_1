using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NT106.Services
{
    public class CloudinaryHelper
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryHelper()
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
                var result = _cloudinary.Destroy(deletionParams);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa ảnh cũ: " + ex.Message);
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
                Folder = "Avatar" // Thư mục mới 
            };

            _cloudinary.Upload(uploadParams);
        }

        // Lấy ảnh
        public async Task<Image> GetImageAsync(string publicId)
        {
            var getResourceParams = new GetResourceParams(publicId);
            var result = await _cloudinary.GetResourceAsync(getResourceParams);

            // Lấy URL ảnh
            string imageUrl = result.Url;

            // Tải ảnh từ URL
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(imageUrl);
            using var stream = await response.Content.ReadAsStreamAsync();

            return Image.FromStream(stream);
        }
    }
}
