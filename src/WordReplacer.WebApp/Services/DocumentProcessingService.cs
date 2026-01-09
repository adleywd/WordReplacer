using Microsoft.Extensions.Localization;
using WordReplacer.Common;
using WordReplacer.Dto;
using WordReplacer.Models;
using WordReplacer.Models.Enums;
using WordReplacer.Services;
using WordReplacer.Enums;
using WordReplacer.WebApp.Resources;

namespace WordReplacer.WebApp.Services;

/// <summary>
/// Document processing service
/// </summary>
public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDocumentService _documentService;
    private readonly IStringLocalizer<GeneralResource> _generalLocalizer;
    private const bool IsMultipleWordsAtOnce = true;

    /// <summary>
    /// Initializes a new instance of <see cref="DocumentProcessingService"/>.
    /// </summary>
    /// <param name="documentService">The document service.</param>
    /// <param name="generalLocalizer">The localizer.</param>
    /// <exception cref="ArgumentNullException">documentService</exception>
    public DocumentProcessingService(IDocumentService documentService, IStringLocalizer<GeneralResource> generalLocalizer)
    {
        ArgumentNullException.ThrowIfNull(documentService);
        ArgumentNullException.ThrowIfNull(generalLocalizer);
        _documentService = documentService;
        _generalLocalizer = generalLocalizer;
    }

    /// <inheritdoc />
    public void AddValues(
        Document doc,
        DocumentParamsDto docParamsDto)
    {
        if (doc.DocumentValues.Select(d => d.Key.Text).Any(text => text == docParamsDto.Text))
        {
            // toasterAction(
            //     string.Format(localizer("valueAlreadyAddedError"), docParamsDto.Text),
            //     MatToastType.Danger,
            //     localizer("valueAlreadyAddedErrorTitle"));
            return;
        }

        var oldValue = new DocumentValue()
        {
            Label = _generalLocalizer["originalDocumentLabel"],
            HtmlId = Guid.NewGuid().ToString(),
            IsOldValue = true,
            Text = docParamsDto.Text,
            HelperText = _generalLocalizer["originalDocumentHelperText"],
            Type = InputType.Text,
            IsAccordionOpen = true
        };

        var newValue = new DocumentValue()
        {
            Label = string.Format(_generalLocalizer["newDocumentLabel"], docParamsDto.Text),
            HtmlId = Guid.NewGuid().ToString(),
            IsOldValue = false,
            Text = string.Empty,
            HelperText = _generalLocalizer["newDocumentHelperText"],
            Type = docParamsDto.Type,
            IsAccordionOpen = true,
            ShouldReplaceForEachLine = docParamsDto.Type == InputType.List
        };

        doc.DocumentValues.Add(new KeyValuePair<DocumentValue, DocumentValue>(oldValue, newValue));
        docParamsDto.Text = string.Empty; // Clear add values text
    }

    public void AddValues(Document doc, string text)
    {
        var delimiter = DelimiterType.None;
        var customDelimiter = string.Empty;
        
        if (doc.DocumentValues.Count > 0)
        {
            delimiter = doc.DocumentValues.Last().Value.Delimiter;
            customDelimiter = doc.DocumentValues.Last().Value.CustomDelimiter;
        }
        
        var oldValue = new DocumentValue()
        {
            Label = _generalLocalizer["originalDocumentLabel"],
            HtmlId = Guid.NewGuid().ToString(),
            IsOldValue = true,
            Text = text,
            HelperText = _generalLocalizer["originalDocumentHelperText"],
            Type = InputType.Text,
            IsAccordionOpen = true
        };

        var newValue = new DocumentValue()
        {
            Label = string.Format(_generalLocalizer["newDocumentLabel"], text),
            HtmlId = Guid.NewGuid().ToString(),
            IsOldValue = false,
            Text = string.Empty,
            HelperText = _generalLocalizer["newDocumentHelperText"],
            Type = InputType.List,
            Delimiter = delimiter,
            CustomDelimiter = customDelimiter,
            IsAccordionOpen = true,
            ShouldReplaceForEachLine = true
        };

        doc.DocumentValues.Add(new KeyValuePair<DocumentValue, DocumentValue>(oldValue, newValue));
    }

    /// <inheritdoc />
    public async Task HandleSubmitAsync(
        Document doc,
        Func<Task> prepareUIToReplaceAndDownload,
        Func<List<Dictionary<string, string>>, bool, List<FileUploadDto>, Task> prepareDownloadUI,
        Func<Task> delayDotNetToUpdateUIAsync,
        Action openDownloadPopup,
        Func<Task> setDefaultUIAfterDownload,
        Func<Task> setDefaultUIAfterError,
        Action<double> updateProgressBar,
        Action<string> onDownloadSuccess,
        Action<string> onDownloadError)
    {
        try
        {
            await prepareUIToReplaceAndDownload().ConfigureAwait(false);

            doc.DocumentValues.SanitizeValues();

            var combinations = new List<Dictionary<string, string>>();
            var isThereAnyReplaceForMultipleLine = doc.DocumentValues.Any(d => d.Value.ShouldReplaceForEachLine);

            if (isThereAnyReplaceForMultipleLine)
            {
                combinations = _documentService.GetAllCombinations(doc.DocumentValues);
            }
            else
            {
                combinations.Add(doc.DocumentValues.ToDictionary(d => d.Key.Text!, d => d.Value.Text!));
            }

            if (isThereAnyReplaceForMultipleLine && combinations.Count == 0)
            {
                // toasterAction("The list of values to be replaced cannot be empty.", MatToastType.Danger);
                return;
            }

            // TODO MOVE IT OUT OF HERE
            var files = new List<FileUploadDto>();
            // files.AddRange(doc.Files.Select(f => new FileUploadDto { Name = f.Name, Size = f.Size, Type = f.ContentType, LastModified = f.LastModified}));

            await prepareDownloadUI(combinations, isThereAnyReplaceForMultipleLine, files).ConfigureAwait(false);
            await delayDotNetToUpdateUIAsync().ConfigureAwait(false);

            // toasterAction("The site may freeze for a few moments.", MatToastType.Primary);

            var progressSizePerFile = 1.0 / (combinations.Count * doc.FilesBrowser.Count);

            openDownloadPopup();
            await delayDotNetToUpdateUIAsync().ConfigureAwait(false);

            foreach (var file in files)
            {
                MemoryStream originalFileInMemoryStream = await _documentService.GetMemoryStream(file).ConfigureAwait(false);

                foreach (var combination in combinations)
                {
                    var fileName = GetFileName(
                        isThereAnyReplaceForMultipleLine || doc.FilesBrowser.Count > 1,
                        combination.Values,
                        file.Name);
                    try
                    {
                        Stream docReplaced = _documentService.Replace(combination, originalFileInMemoryStream, IsMultipleWordsAtOnce);
                        await _documentService.DownloadFile(
                                fileName,
                                docReplaced,
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                            .ConfigureAwait(false);
                        await docReplaced.DisposeAsync().ConfigureAwait(false);
                        onDownloadSuccess(fileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        // toasterAction($"An error occurred while processing the file {fileName}.", MatToastType.Danger);
                        onDownloadError(fileName);
                    }
                    finally
                    {
                        updateProgressBar(progressSizePerFile);
                        await delayDotNetToUpdateUIAsync().ConfigureAwait(false);
                    }
                }

                await originalFileInMemoryStream.DisposeAsync().ConfigureAwait(false);
            }

            await setDefaultUIAfterDownload().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // toasterAction("An unexpected error occurred.", MatToastType.Danger);
            await setDefaultUIAfterError().ConfigureAwait(false);
        }
    }

    public async Task ReplaceWordsAsync(Document doc)
    {
        try
        {
            var combinations = PrepareDocumentCombinations(doc);
            await ProcessAllFilesAsync(doc, combinations).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleProcessingErrorAsync(ex).ConfigureAwait(false);
        }
    }

    public async Task ReplaceWordsAsync(
        Document doc, 
        Action<Dictionary<string, Download>> onDownloadsInitialized,
        Action<string, double> onProgressUpdate,
        Action<string, DownloadStatus> onStatusUpdate,
        Action onCompleted,
        bool shouldAddPrefixToFileName)
    {
        try
        {
            var combinations = PrepareDocumentCombinations(doc);
            var downloads = InitializeDownloads(doc, combinations, shouldAddPrefixToFileName);
            
            onDownloadsInitialized(downloads);
            
            await ProcessAllFilesWithProgressAsync(doc, combinations, onProgressUpdate, onStatusUpdate, shouldAddPrefixToFileName).ConfigureAwait(false);
            
            onCompleted();
        }
        catch (Exception ex)
        {
            await HandleProcessingErrorAsync(ex).ConfigureAwait(false);
            onCompleted();
        }
    }

    private List<Dictionary<string, string>> PrepareDocumentCombinations(Document doc)
    {
        doc.DocumentValues.SanitizeValues();
        return _documentService.GetAllCombinations(doc.DocumentValues);
    }

    private async Task ProcessAllFilesAsync(Document doc, List<Dictionary<string, string>> combinations)
    {
        var progressSizePerFile = CalculateProgressSizePerFile(combinations.Count, doc.Files.Count);

        foreach (var file in doc.Files)
        {
            await ProcessSingleFileAsync(file, combinations, progressSizePerFile).ConfigureAwait(false);
        }
    }

    private double CalculateProgressSizePerFile(int combinationsCount, int filesCount)
    {
        return 1.0 / (combinationsCount * filesCount);
    }

    private async Task ProcessSingleFileAsync(KeyValuePair<string, FileUploadDto> file, List<Dictionary<string, string>> combinations, double progressSizePerFile)
    {
        using var originalFileInMemoryStream = await _documentService.GetMemoryStream(file.Value).ConfigureAwait(false);

        foreach (var combination in combinations)
        {
            await ProcessCombinationAsync(file, combination, originalFileInMemoryStream, progressSizePerFile).ConfigureAwait(false);
        }
    }

    private async Task ProcessCombinationAsync(KeyValuePair<string, FileUploadDto> file, Dictionary<string, string> combination, MemoryStream originalFileInMemoryStream, double progressSizePerFile)
    {
        var fileName = GetFileName(combination.Values, file.Value.Name);
        
        try
        {
            await ReplaceAndDownloadFileAsync(combination, originalFileInMemoryStream, fileName).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            await UpdateProgressAsync(progressSizePerFile).ConfigureAwait(false);
        }
    }

    private async Task ReplaceAndDownloadFileAsync(Dictionary<string, string> combination, MemoryStream originalFileInMemoryStream, string fileName)
    {
        using var docReplaced = _documentService.Replace(combination, originalFileInMemoryStream, IsMultipleWordsAtOnce);
        
        await _documentService.DownloadFile(
            fileName,
            docReplaced,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            .ConfigureAwait(false);
    }

    private async Task UpdateProgressAsync(double progressSizePerFile)
    {
        // updateProgressBar(progressSizePerFile);
        // await delayDotNetToUpdateUIAsync().ConfigureAwait(false);
        await Task.CompletedTask;
    }

    private async Task HandleProcessingErrorAsync(Exception ex)
    {
        Console.WriteLine(ex.Message);
        // await setDefaultUIAfterError().ConfigureAwait(false);
        await Task.CompletedTask;
    }

    private Dictionary<string, Download> InitializeDownloads(Document doc, List<Dictionary<string, string>> combinations, bool shouldAddPrefix)
    {
        var downloads = new Dictionary<string, Download>();
        foreach (var file in doc.Files)
        {
            foreach (var combination in combinations)
            {
                var fileName = GetFileName(combination.Values, file.Value.Name, shouldAddPrefix);
                
                if(downloads.ContainsKey(fileName))
                {
                    // increase count to reflect multiple real files mapping to same display name
                    downloads[fileName].Count++;
                    continue;
                }
                
                downloads.Add(fileName, new Download 
                { 
                    FileName = fileName,
                    Status = DownloadStatus.InProgress,
                    Progress = 0.0,
                    IsProgressIndeterminate = true,
                    Count = 1
                });
            }
        }
        return downloads;
    }

    private async Task ProcessAllFilesWithProgressAsync(Document doc, List<Dictionary<string, string>> combinations, Action<string, double> onProgressUpdate, Action<string, DownloadStatus> onStatusUpdate, bool shouldAddPrefix)
    {
        var progressSizePerFile = CalculateProgressSizePerFile(combinations.Count, doc.Files.Count);

        foreach (var file in doc.Files)
        {
            await ProcessSingleFileWithProgressAsync(file, combinations, progressSizePerFile, onProgressUpdate, onStatusUpdate, shouldAddPrefix).ConfigureAwait(false);
        }
    }

    private async Task ProcessSingleFileWithProgressAsync(KeyValuePair<string, FileUploadDto> file, List<Dictionary<string, string>> combinations, double progressSizePerFile, Action<string, double> onProgressUpdate, Action<string, DownloadStatus> onStatusUpdate, bool shouldAddPrefix)
    {
        using var originalFileInMemoryStream = await _documentService.GetMemoryStream(file.Value).ConfigureAwait(false);

        foreach (var combination in combinations)
        {
            await ProcessCombinationWithProgressAsync(file, combination, originalFileInMemoryStream, progressSizePerFile, onProgressUpdate, onStatusUpdate, shouldAddPrefix).ConfigureAwait(false);
        }
    }

    private async Task ProcessCombinationWithProgressAsync(KeyValuePair<string, FileUploadDto> file, Dictionary<string, string> combination, MemoryStream originalFileInMemoryStream, double progressSizePerFile, Action<string, double> onProgressUpdate, Action<string, DownloadStatus> onStatusUpdate, bool shouldAddPrefix)
    {
        var fileName = GetFileName(combination.Values, file.Value.Name, shouldAddPrefix);
        
        try
        {
            onStatusUpdate(fileName, DownloadStatus.InProgress);
            await ReplaceAndDownloadFileAsync(combination, originalFileInMemoryStream, fileName).ConfigureAwait(false);
            onStatusUpdate(fileName, DownloadStatus.Success);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            onStatusUpdate(fileName, DownloadStatus.Error);
        }
        finally
        {
            onProgressUpdate(fileName, progressSizePerFile);
        }
    }

    private string GetFileName(IEnumerable<string> combinationsValues, string inputFileName, bool shouldAddPrefix)
    {
        var sanitizedPart = Helper.SanitizeFileName(string.Join("_", combinationsValues));

        if (shouldAddPrefix)
        {
            return $"{GetFileNameWithoutExtension(inputFileName)}_{sanitizedPart}.docx";
        }

        return $"{sanitizedPart}.docx";
    }

    // existing overloads kept for other code paths
    private string GetFileName(IEnumerable<string> combinationsValues, string inputFileName)
    {
        return $"{GetFileNameWithoutExtension(inputFileName)}_{Helper.SanitizeFileName(string.Join("_", combinationsValues))}.docx";
    }

    private string GetFileName(bool hasMultipleFiles, IEnumerable<string> combinationsValues, string inputFileName)
    {
        var fileName = inputFileName;
        if (hasMultipleFiles)
        {
            fileName = $"{GetFileNameWithoutExtension(fileName)}_{Helper.SanitizeFileName(string.Join("_", combinationsValues))}.docx";
        }

        return fileName;
    }

    private string GetFileNameWithoutExtension(string fileName)
    {
        return Path.GetFileNameWithoutExtension(fileName);
    }
}

