const isDebug = self.location.hostname === 'localhost' || self.location.hostname === '127.0.0.1';
const debugLog = isDebug ? console.log.bind(console) : () => { };

debugLog('[Worker] Starting DocumentReplaceWorker...');

let assemblyExports;
let startupError;

try {
    debugLog('[Worker] Importing dotnet.js...');
    const { dotnet } = await import('../_framework/dotnet.js');
    debugLog('[Worker] dotnet imported, calling dotnet.create()...');
    const runtime = await dotnet.create();
    debugLog('[Worker] Runtime created, getting config...');
    const config = runtime.getConfig();
    debugLog('[Worker] Config mainAssemblyName:', config.mainAssemblyName);
    assemblyExports = await runtime.getAssemblyExports(config.mainAssemblyName);
    debugLog('[Worker] Assembly exports loaded. Available keys:', Object.keys(assemblyExports));
} catch (err) {
    startupError = err.message;
    console.error('[Worker] Startup error:', err);
}

self.postMessage({ command: 'ready' });
debugLog('[Worker] Ready message sent');

function base64ToBytes(base64) {
    const binaryString = atob(base64);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes;
}

function bytesToBase64(bytes) {
    let binaryString = '';
    for (let i = 0; i < bytes.length; i++) {
        binaryString += String.fromCharCode(bytes[i]);
    }
    return btoa(binaryString);
}

self.addEventListener('message', async e => {
    debugLog('[Worker] Received message:', e.data.command, 'requestId:', e.data.requestId);
    try {
        if (!assemblyExports) {
            throw new Error(startupError || 'Worker exports not loaded');
        }

        let result;
        switch (e.data.command) {
            case 'replace':
                debugLog('[Worker] Decoding base64 input, length:', e.data.fileBytesBase64?.length);
                const fileBytes = base64ToBytes(e.data.fileBytesBase64);
                debugLog('[Worker] Decoded to', fileBytes.length, 'bytes. Calling Replace...');
                const resultBytes = assemblyExports.WordReplacer.WebApp.Workers.DocumentReplaceWorker.Replace(
                    fileBytes,
                    e.data.replacementsJson
                );
                debugLog('[Worker] Replace returned', resultBytes?.length, 'bytes. Encoding to base64...');
                result = bytesToBase64(resultBytes);
                debugLog('[Worker] Base64 result length:', result?.length);
                break;
            default:
                throw new Error(`Unknown command: ${e.data.command}`);
        }

        debugLog('[Worker] Sending response for requestId:', e.data.requestId);
        self.postMessage({
            command: 'response',
            requestId: e.data.requestId,
            result
        });
    } catch (err) {
        console.error('[Worker] Error processing message:', err);
        self.postMessage({
            command: 'response',
            requestId: e.data.requestId,
            error: err.message
        });
    }
});
