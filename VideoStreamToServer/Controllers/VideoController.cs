using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using SampleApp.Utilities;
using System.Net;
using VideoStreamToServer.Configurations;
using VideoStreamToServer.Utilities;

namespace VideoStreamToServer.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class VideoController : Controller
    {

        private readonly IConfiguration Configuration;
        public FileUploadSettings? UploadSettings { get; private set; }


        public VideoController(IConfiguration configuration)
        {
            Configuration = configuration;
            UploadSettings = configuration.GetSection("FileUpload").Get<FileUploadSettings>();
        }
        [HttpPost]
        [DisableRequestSizeLimit]

        public async Task<IActionResult> UploadPhysical()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                UploadSettings.MaximumFileSizeInByte);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                       

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            UploadSettings.AllowedFileExtensions, UploadSettings.MaximumFileSizeInByte);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(UploadSettings.UploadPath, trustedFileNameForFileStorage.Remove(trustedFileNameForFileStorage.Length-3)+ Path.GetExtension(contentDisposition.FileName.Value))))
                        {
                            await targetStream.WriteAsync(streamedFileContent);
 
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(VideoController), null);
        }
    }
}
