using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;
using WordReplacer.WebApp.Clients;

namespace WordReplacer.WebApp.Services;

[SupportedOSPlatform("browser")]
public class WorkerDocumentService
{
    public async Task<byte[]> ReplaceAsync(byte[] fileBytes, Dictionary<string, string> values)
    {
        Debug.WriteLine($"[WorkerDocumentService] ReplaceAsync called, fileBytes length: {fileBytes.Length}, values count: {values.Count}");
        Debug.WriteLine("[WorkerDocumentService] Calling InitClient...");
        await DocumentWorkerClient.InitClient().ConfigureAwait(false);
        Debug.WriteLine("[WorkerDocumentService] InitClient done. Serializing values...");
        var json = JsonSerializer.Serialize(values);
        Debug.WriteLine($"[WorkerDocumentService] JSON length: {json.Length}. Calling ReplaceDocumentAsync...");
        var result = await DocumentWorkerClient.ReplaceDocumentAsync(fileBytes, json).ConfigureAwait(false);
        Debug.WriteLine($"[WorkerDocumentService] ReplaceDocumentAsync returned {result.Length} bytes");
        return result;
    }
}
