namespace Extraordinary.WebApi.Controllers.Models
{
    public class FileItem
    {
        public string Name { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string Size { get; set; } = "0 B";
        public string Modified { get; set; } = string.Empty;
    }
}