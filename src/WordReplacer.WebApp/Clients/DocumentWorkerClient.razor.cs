using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace WordReplacer.WebApp.Clients;

[SupportedOSPlatform("browser")]
public partial class DocumentWorkerClient
{
    private static bool _workerStarted;

    public static async Task InitClient()
    {
        if (_workerStarted)
        {
            Debug.WriteLine("[DocumentWorkerClient] InitClient already done, skipping.");
            return;
        }
        _workerStarted = true;

        Debug.WriteLine("[DocumentWorkerClient] Calling JSHost.ImportAsync...");
        await JSHost.ImportAsync(
            moduleName: nameof(DocumentWorkerClient),
            moduleUrl: "../Clients/DocumentWorkerClient.razor.js").ConfigureAwait(false);
        Debug.WriteLine("[DocumentWorkerClient] JSHost.ImportAsync completed.");
    }

    [JSImport("replaceDocument", nameof(DocumentWorkerClient))]
    public static partial Task<string> ReplaceDocumentBase64Async(
        string fileBytesBase64, string replacementsJson);

    public static async Task<byte[]> ReplaceDocumentAsync(byte[] fileBytes, string replacementsJson)
    {
        Debug.WriteLine($"[DocumentWorkerClient] ReplaceDocumentAsync: converting {fileBytes.Length} bytes to base64...");
        var base64Input = Convert.ToBase64String(fileBytes);
        Debug.WriteLine($"[DocumentWorkerClient] Base64 input length: {base64Input.Length}. Calling ReplaceDocumentBase64Async...");
        var base64Result = await ReplaceDocumentBase64Async(base64Input, replacementsJson).ConfigureAwait(false);
        Debug.WriteLine($"[DocumentWorkerClient] ReplaceDocumentBase64Async returned, base64 result length: {base64Result?.Length}");
        return Convert.FromBase64String(base64Result);
    }
}
