using Microsoft.AspNetCore.Mvc;
using FileTransferSender.Services;

namespace FileTransferSender.Controllers
{
    [ApiController]
    [Route("api/uploads")] // REST: Kaynak bazlı URL
    public class UploadController : ControllerBase
    {
        private readonly TransferService _transferService;
        private readonly IWebHostEnvironment _env;

        public UploadController(TransferService transferService, IWebHostEnvironment env)
        {
            _transferService = transferService;
            _env = env;
        }

        /// <summary>
        /// Bir dosya yükler ve alıcı cihaza parça parça gönderir.
        /// </summary>
        /// <param name="file">Yüklenecek dosya (multipart/form-data)</param>
        /// <returns>Gönderim sonucu mesajı</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAndSend([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            var savePath = Path.Combine(_env.ContentRootPath, "kargo", file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

            // Geçici olarak dosyayı diske yaz
            using (var stream = new FileStream(savePath, FileMode.Create))
                await file.CopyToAsync(stream);

            // Chunked olarak gönder
            var resultMessage = await _transferService.SendFileAsync(savePath);
            return Ok(resultMessage);
        }
    }
}
