using MatBlazor;
using Microsoft.Extensions.Localization;
using WordReplacer.Common;
using WordReplacer.Dto;
using WordReplacer.Models;
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
            doc.DocumentValues.SanitizeValues();

            var combinations = _documentService.GetAllCombinations(doc.DocumentValues);

            // TODO MOVE IT OUT OF HERE

            var progressSizePerFile = 1.0 / (combinations.Count * doc.Files.Count);

            foreach (var file in doc.Files)
            {
                MemoryStream originalFileInMemoryStream = await _documentService.GetMemoryStream(file.Value).ConfigureAwait(false);

                foreach (var combination in combinations)
                {
                    var fileName = GetFileName(combination.Values, file.Value.Name);
                    try
                    {
                        Stream docReplaced = _documentService.Replace(combination, originalFileInMemoryStream, IsMultipleWordsAtOnce);
                        await _documentService.DownloadFile(
                                fileName,
                                docReplaced,
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                            .ConfigureAwait(false);
                        await docReplaced.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        // updateProgressBar(progressSizePerFile);
                        // await delayDotNetToUpdateUIAsync().ConfigureAwait(false);
                    }
                }

                await originalFileInMemoryStream.DisposeAsync().ConfigureAwait(false);
            }

            // await setDefaultUIAfterDownload().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // await setDefaultUIAfterError().ConfigureAwait(false);
        }
    }


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