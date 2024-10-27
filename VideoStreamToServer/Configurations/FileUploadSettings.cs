namespace VideoStreamToServer.Configurations
{
    public record FileUploadSettings
    {
        public int MaximumFileSizeInByte { get; set; }
        public string UploadPath { get; set; }
        public string[] AllowedFileExtensions { get; set; }

    };
    
}
