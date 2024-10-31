 using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using SampleApp.Utilities;
using System.IO;
using System.Net;
using VideoStreamToServer.Configurations;
using VideoStreamToServer.Services;
using VideoStreamToServer.Utilities;
 
namespace VideoStreamToServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : Controller
    {
        public readonly FileUploadSettings? UploadSettings;
        public readonly PDFGenerator PDFGenerator;
        public ReportController(FileUploadSettings uploadSettings, PDFGenerator pDFGenerator)
        {
            UploadSettings = uploadSettings;
            PDFGenerator = pDFGenerator;
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadPhysical()
        {
            string filepath="";

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
                            Path.Combine(UploadSettings.UploadPath, trustedFileNameForFileStorage.Remove(trustedFileNameForFileStorage.Length - 3) + Path.GetExtension(contentDisposition.FileName.Value))))
                        {
                            filepath = targetStream.Name;
                            await targetStream.WriteAsync(streamedFileContent);

                        }
                          
                    }
                }

              
                section = await reader.ReadNextSectionAsync();
            }
            PDFGenerator.ConvertHtmlToPdf(filepath, @"D:\File.pdf");

            var fileName = "test.pdf";
            var mimeType = "application/pdf";
            var stream = System.IO.File.OpenRead(@"D:\File.pdf");


            return new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = fileName
            };

 
            return Created(nameof(VideoController), null);
        }
    }
}
