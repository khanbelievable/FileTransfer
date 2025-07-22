using Microsoft.AspNetCore.Mvc;
using static FileTransferSender.Services.TransferService;

namespace FileTransferSender.Controllers
{
    [ApiController]
    [Route("api/progress")]
    public class ProgressController : ControllerBase
    {
        [HttpGet("{filename}")]
        public IActionResult GetProgress(string filename)
        {
            if (TransferProgress.TryGetValue(filename, out var progress))
            {
                return Ok(new
                {
                    total = progress.TotalChunks,
                    uploaded = progress.UploadedChunks
                });
            }
            return NotFound();
        }
    }
}
