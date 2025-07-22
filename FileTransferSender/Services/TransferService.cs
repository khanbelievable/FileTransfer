using System.Net.Http.Headers;


public class TransferProgressTracker
{
    public int TotalChunks { get; set; }
    public int UploadedChunks { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}


namespace FileTransferSender.Services
{
    public class TransferService
    {
        public static Dictionary<string, TransferProgressTracker> TransferProgress = new();

        private readonly HttpClient _httpClient;

        public TransferService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> SendFileAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("Dosya bulunamadı", filePath);

            var fileInfo = new FileInfo(filePath);
            long fileSizeBytes = fileInfo.Length;
            long fileSizeMB = fileSizeBytes / (1024 * 1024);
            int maxConcurrency, chunkSize;
            int mb = 1024 * 1024;

            switch (fileSizeMB)
            {
                case < 5: chunkSize = 1 * mb; maxConcurrency = 1; break;
                case < 20: chunkSize = 2 * mb; maxConcurrency = 2; break;
                case < 100: chunkSize = 5 * mb; maxConcurrency = 4; break;
                case < 500: chunkSize = 8 * mb; maxConcurrency = 5; break;
                case < 1024: chunkSize = 10 * mb; maxConcurrency = 6; break;
                default: chunkSize = 15 * mb; maxConcurrency = 8; break;
            }

            string receiverUrl = "http://192.168.228.16:5000/api/upload/chunk";
            var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);
            string fileKey = Path.GetFileName(filePath);
            TransferProgress[fileKey] = new TransferProgressTracker
            {
                TotalChunks = totalChunks,
                UploadedChunks = 0
            };
            var uploadTasks = new List<Task>();

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < totalChunks; i++)
            {
                var buffer = new byte[chunkSize];
                int bytesRead = await fileStream.ReadAsync(buffer, 0, chunkSize);
                if (bytesRead == 0) break;

                int chunkIndex = i;
                var chunkData = new byte[bytesRead];
                Array.Copy(buffer, chunkData, bytesRead);

                await semaphore.WaitAsync();
                uploadTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var content = new MultipartFormDataContent();
                        var byteContent = new ByteArrayContent(chunkData);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                        content.Add(byteContent, "chunk", $"chunk_{chunkIndex}");
                        content.Add(new StringContent(chunkIndex.ToString()), "index");
                        content.Add(new StringContent(totalChunks.ToString()), "total");
                        content.Add(new StringContent(Path.GetFileName(filePath)), "filename");

                        var response = await _httpClient.PostAsync(receiverUrl, content);
                        response.EnsureSuccessStatusCode();
                        TransferProgress[fileKey].UploadedChunks++;
                        TransferProgress[fileKey].LastUpdated = DateTime.UtcNow;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(uploadTasks);
            stopwatch.Stop();
            Console.WriteLine($"chunkSize: {chunkSize / mb}MB concurrency: {maxConcurrency}");
            return $"Gönderim tamamlandı. Süre: {stopwatch.Elapsed.TotalSeconds:F2} saniye.";
        }
    }
}