using WordReplacer.Models.Enums;

namespace WordReplacer.Models;

public class Download
{
    public string FileName { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DownloadStatus Status { get; set; }
    public bool IsProgressIndeterminate { get; set; }

    // Number of actual files that share this displayed filename
    public int Count { get; set; } = 1;

    // How many of those actual files have completed (success/error)
    public int CompletedCount { get; set; } = 0;
}