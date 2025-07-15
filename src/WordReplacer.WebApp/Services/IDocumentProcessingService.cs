using MatBlazor;
using WordReplacer.Dto;
using WordReplacer.Models;

namespace WordReplacer.WebApp.Services;

/// <summary>
/// Document Processing Service Interface
/// </summary>
public interface IDocumentProcessingService
{
    /// <summary>
    /// Add Values for replacing in document
    /// </summary>
    /// <param name="doc">The document.</param>
    void AddValues(Document doc, DocumentParamsDto docParamsDto);
    
    /// <summary>
    /// Add Values for replacing in document
    /// </summary>
    /// <param name="doc">The document.</param>
    void AddValues(Document doc, string text);
    
    /// <summary>
    /// Handles the submit async.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="prepareUIToReplaceAndDownload"></param>
    /// <param name="prepareDownloadUI"></param>
    /// <param name="delayDotNetToUpdateUIAsync"></param>
    /// <param name="toasterAction"></param>
    /// <param name="openDownloadPopup"></param>
    /// <param name="setDefaultUIAfterDownload"></param>
    /// <param name="setDefaultUIAfterError"></param>
    /// <param name="updateProgressBar"></param>
    /// <param name="onDownloadSuccess"></param>
    /// <param name="onDownloadError"></param>
    /// <returns></returns>
    Task HandleSubmitAsync(
        Document doc,
        Func<Task> prepareUIToReplaceAndDownload,
        Func<List<Dictionary<string, string>>, bool, List<FileUploadDto>, Task> prepareDownloadUI,
        Func<Task> delayDotNetToUpdateUIAsync,
        Action openDownloadPopup,
        Func<Task> setDefaultUIAfterDownload,
        Func<Task> setDefaultUIAfterError,
        Action<double> updateProgressBar,
        Action<string> onDownloadSuccess,
        Action<string> onDownloadError
    );
}
