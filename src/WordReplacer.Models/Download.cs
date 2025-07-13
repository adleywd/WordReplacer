using WordReplacer.Models.Enums;

namespace WordReplacer.Models;

public class Download
{
    public string FileName { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DownloadStatus Status { get; set; }
    public bool IsProgressIndeterminate { get; set; }
}