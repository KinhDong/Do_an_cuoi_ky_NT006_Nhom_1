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
		
		// URL avatar mặc định của bạn
		private const string DEFAULT_AVATAR_URL = "https://res.cloudinary.com/dwy5bb7gl/image/upload/v1761364135/AccountIcon_vv8dje.jpg";


		/// Xóa ảnh cũ dựa trên userId
		private static void DeleteOldImageBeforeUpload(string Uid)
		{
			try
			{
				// Đã đúng: Sử dụng publicId là "avatar/Uid"
				var deletionParams = new DeletionParams($"avatar/{Uid}")
				{
					ResourceType = ResourceType.Image
				};
				_cloudinary.Destroy(deletionParams);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Cloudinary Delete Lỗi: {ex.Message}");
			}
		}

		// Up ảnh mới
		public static string UploadImage(string filePath, string Uid)
		{
			// Xóa ảnh cũ
			DeleteOldImageBeforeUpload(Uid);

			var uploadParams = new ImageUploadParams()
			{
				File = new FileDescription(filePath),
				PublicId = $"avatar/{Uid}",  // SỬA LẠI: PublicId bao gồm cả folder
				Overwrite = true,
				Invalidate = true
				// (Xóa 'Folder' property)
			};

			var uploadResult = _cloudinary.Upload(uploadParams);
			return uploadResult.SecureUrl.ToString();
		}

		// HÀM MỚI ĐƯỢC SỬA LẠI TỪ 'CopyImage'
		/// Gán avatar mặc định cho user mới.
		/// <param name="Uid">ID của user mới</param>
		public static async Task CopyDefaultAvatarAsync(string Uid)
		{
			var uploadParams = new ImageUploadParams()
			{
				File = new FileDescription(DEFAULT_AVATAR_URL),
				PublicId = $"avatar/{Uid}", // SỬA LẠI: Giống như các hàm khác
				Overwrite = true
				// (Xóa 'Folder' property)
			};

			// Chạy bất đồng bộ để không block luồng chính
			await Task.Run(() =>
			{
				try
				{
					_cloudinary.Upload(uploadParams);
				}
				catch (Exception ex)
				{
					GD.PrintErr($"Lỗi Cloudinary CopyDefaultAvatarAsync: {ex.Message}");
				}
			});
		}


		// Lấy ảnh
		public static async Task<Texture2D> GetImageAsync(string publicId)
		{
			try
			{
				var result = await _cloudinary.GetResourceAsync(new GetResourceParams($"avatar/{publicId}"));
				string imageUrl = result.Url;

				using var http = new System.Net.Http.HttpClient();
				using var res = await http.GetAsync(imageUrl);

				var stream = await res.Content.ReadAsStreamAsync();
				var tex = await ConvertStreamToTexture(stream);

				if (tex != null)
					return tex;

				GD.PrintErr("Ảnh Cloudinary hỏng → dùng default");
			}
			catch { }

			// Fallback default
			try
			{
				using var http = new System.Net.Http.HttpClient();
				using var res = await http.GetAsync(DEFAULT_AVATAR_URL);

				var stream = await res.Content.ReadAsStreamAsync();
				return await ConvertStreamToTexture(stream);
			}
			catch
			{
				GD.PrintErr("Ảnh default cũng lỗi");
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

				var err =
					image.LoadPngFromBuffer(imageData) != Godot.Error.Ok &&
					image.LoadJpgFromBuffer(imageData) != Godot.Error.Ok &&
					image.LoadWebpFromBuffer(imageData) != Godot.Error.Ok;

				if (!err)
					return ImageTexture.CreateFromImage(image);

				GD.PrintErr("Ảnh không hợp lệ — dùng ảnh default.");
				return null;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"ConvertStreamToTexture EXCEPTION: {ex.Message}");
				return null;  // quan trọng: KHÔNG throw
			}
		}
		
		
	}
}