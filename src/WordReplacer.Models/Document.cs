using Microsoft.AspNetCore.Components.Forms;
using WordReplacer.Dto;

namespace WordReplacer.Models;

public class Document
{
    public List<KeyValuePair<DocumentValue, DocumentValue>> DocumentValues { get; set; } = [];
    public Dictionary<string, FileUploadDto> FilesDto { get; set; } = [];
    public List<IBrowserFile> FilesBrowser { get; } = [];
}