using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using FileTransferSender.Services;

namespace FileTransferSender.Controllers
{
    [ApiController]
    [Route("api/send")]
    public class SendController : ControllerBase
    {
        private readonly TransferService _transferService;
        private readonly IWebHostEnvironment _env;
        public SendController(TransferService transferService, IWebHostEnvironment env)
        {
            _transferService = transferService;
            _env = env;
        }

        [HttpPost("manual")]

        public async Task<IActionResult> SendManual()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "kargo", "400mb.rar");
            if (!System.IO.File.Exists(path))
                return NotFound("Dosya bulunamadı");

            var result = await _transferService.SendFileAsync(path);
            return Ok(result);
        }

    }
}