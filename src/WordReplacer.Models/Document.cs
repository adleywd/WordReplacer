using Microsoft.AspNetCore.Components.Forms;
using WordReplacer.Dto;

namespace WordReplacer.Models;

public class Document
{
    public List<KeyValuePair<DocumentValue, DocumentValue>> DocumentValues { get; set; } = [];
    public IDictionary<string, FileUploadDto> Files { get; set; } = new Dictionary<string, FileUploadDto>();
    public List<IBrowserFile> FilesBrowser { get; } = [];
}