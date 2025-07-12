namespace HotByteProject.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveImageAsync(IFormFile file, string folderName = "images")
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{folderName}/{fileName}";
        }
    }
}
