using Microsoft.AspNetCore.Mvc;

namespace FileTransferReceiver.Controllers
{
    
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private static readonly SemaphoreSlim _mergeLock = new(1, 1); // class seviyesinde

        [HttpPost("chunk")]
        public async Task<IActionResult> UploadChunk(
            [FromForm] IFormFile chunk,
            [FromForm] int index,
            [FromForm] int total,
            [FromForm] string filename)
        {
            try
            {
                if (chunk == null || chunk.Length == 0)
                    return BadRequest("Chunk eksik.");

                var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "TempChunks", filename);
                Directory.CreateDirectory(tempFolder);

                var chunkPath = Path.Combine(tempFolder, $"chunk_{index}");
                using (var stream = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await chunk.CopyToAsync(stream);
                }

                // Merge kilidini al ve tüm chunklar var mı kontrol et
                await _mergeLock.WaitAsync();
                try
                {
                    bool allChunksExist = true;
                    for (int i = 0; i < total; i++)
                    {
                        string partPath = Path.Combine(tempFolder, $"chunk_{i}");
                        if (!System.IO.File.Exists(partPath))
                        {
                            allChunksExist = false;
                            break;
                        }
                    }

                    if (allChunksExist)
                    {
                        var finalPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", filename);
                        Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);

                        using var finalStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None);
                        for (int i = 0; i < total; i++)
                        {
                            var partPath = Path.Combine(tempFolder, $"chunk_{i}");
                            var bytes = await System.IO.File.ReadAllBytesAsync(partPath);
                            await finalStream.WriteAsync(bytes, 0, bytes.Length);
                        }

                        Directory.Delete(tempFolder, true);
                        Console.WriteLine($"[OK] {filename} birleştirildi.");
                    }
                }
                finally
                {
                    _mergeLock.Release();
                }

                return Ok("Chunk kaydedildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Receiver ERROR] Chunk {index}/{total} - {ex.Message}");
                return StatusCode(500, $"HATA: {ex.Message}");
            }
        }
    }
}
