using tusdotnet;

namespace VideoStreamToServer.Extensions
{
    public static class ChunckUpload
    {
        public static void ConfigureTusChunkUpload(  this WebApplication app, string routPath ,string UploadPath) {

            app.MapTus(routPath, async httpContext => new()
            {
                // This method is called on each request so different configurations can be returned per user, domain, path etc.
                // Return null to disable tusdotnet for the current request.

                // Where to store data?
                Store = new tusdotnet.Stores.TusDiskStore(UploadPath),
                Events = new()
                {
                    // What to do when file is completely uploaded?
                    OnFileCompleteAsync = async eventContext =>
                    {
                        tusdotnet.Interfaces.ITusFile file = await eventContext.GetFileAsync();
                        Dictionary<string, tusdotnet.Models.Metadata> metadata = await file.GetMetadataAsync(eventContext.CancellationToken);
                        using Stream content = await file.GetContentAsync(eventContext.CancellationToken);

                     }
                }
            });
        }
    }
}
