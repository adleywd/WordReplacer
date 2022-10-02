namespace WordReplacer.Models;

public class Download
{
    public string FileName { get; set; } = string.Empty;
    public double Progress { get; set; }
    public bool IsDownloading { get; set; }
    public bool IsDownloadCompleted { get; set; }
    public bool IsProgressIndeterminate { get; set; }
}