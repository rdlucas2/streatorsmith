using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace compUpload.Controllers
{
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;

        public UploadController(ILogger<UploadController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [RequestSizeLimit(1610612736)]
        [Route("api/[controller]")]
        public async Task<ContentResult> Upload([FromForm] FileInputModel model)
        {
            string errorMsg = string.Empty;
            try
            {
                if (!modelIsValid(model, out errorMsg))
                    throw new Exception(errorMsg);

                await UploadBlob(model);

                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "<p>Your submission was succesful, thank you!</p>"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File Upload Error");
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = $"<p>Error uploading your submission. <a href=\"/\">Try again</a> - making sure your contestant number was entered and the code was correct, or contact <a href=\"mailto:cmm_piano@hotmail.com?Subject=Streator%20Smith%20Competition%20Video%20Upload%20Error&body={ex}\" target=\"_top\">cmm_piano@hotmail.com</a> - please send the following error message along with the email.</p><br><p>Error Message:<br>{ex.ToString().Replace("\r\n", "<br>")}</p>"
                };
            }
        }

        private async Task UploadBlob(FileInputModel model)
        {
                string connString = Environment.GetEnvironmentVariable("connString");

                string untrustedFileName = Path.GetFileName(model.FileToUpload.FileName);
                string blobName = $"{DateTime.Now.Ticks}_streator_smith_contestant_{model.ContestantNumber}_{untrustedFileName}";

                string containerName = "competition";
                BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
                BlobContainerClient containerClient = null;
                try 
                {
                    containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
                } 
                catch (RequestFailedException ex)
                {
                    if(ex.ErrorCode != "ContainerAlreadyExists")
                        throw;
                    else
                        containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                }
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                using (Stream uploadFileStream = model.FileToUpload.OpenReadStream())
                {
                    await blobClient.UploadAsync(uploadFileStream, true);
                }
        }

        //https://blog.filestack.com/thoughts-and-knowledge/complete-list-audio-video-file-formats/
        static string[] mediaExtensions = {
            ".WEBM", ".MPG", ".MP2", ".MPEG", ".MPE", ".MPV", ".OGG", ".MP4", ".M4P", ".M4V", ".AVI", ".WMV", ".MOV", ".QT", ".FLV", ".SWF", "AVCHD"
        };

        static bool IsMediaFile(string path)
        {
            return -1 != Array.IndexOf(mediaExtensions, Path.GetExtension(path).ToUpperInvariant());
        }

        private bool modelIsValid(FileInputModel model, out string errorMsg)
        {
            errorMsg = string.Empty;
            string secretcode = Environment.GetEnvironmentVariable("secretcode");
            
            if (model.Code != secretcode)
                errorMsg += "Invalid Code.\r\n";

            if (string.IsNullOrWhiteSpace(model.ContestantNumber))
                errorMsg += "Contestant Number is required.\r\n";

            if (model.FileToUpload == null || model.FileToUpload.Length <= 0 || string.IsNullOrWhiteSpace(model.FileToUpload.FileName))
            {
                errorMsg += "Must Include a file to upload.\r\n";
            }
            else
            {
                if (!IsMediaFile(model.FileToUpload.FileName))
                    errorMsg += "Invalid File Format.\r\n";

                if (model.FileToUpload.Length > 1610612736)
                    errorMsg += "File too big.\r\n";
            }

            if (!string.IsNullOrWhiteSpace(errorMsg))
                return false;

            return true;
        }
    }

    public class FileInputModel
    {
        public string ContestantNumber { get; set; }
        public string Code { get; set; }
        public IFormFile FileToUpload { get; set; }
    }

}
