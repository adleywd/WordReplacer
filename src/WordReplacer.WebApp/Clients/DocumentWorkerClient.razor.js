const isDebug = location.hostname === 'localhost' || location.hostname === '127.0.0.1';
const debugLog = isDebug ? console.log.bind(console) : () => { };

const pendingRequests = {};
let pendingRequestId = 0;
let workerReady = false;
let workerReadyPromiseResolve;
const workerReadyPromise = new Promise(resolve => {
    workerReadyPromiseResolve = resolve;
});

debugLog('[WorkerClient] Creating worker...');

const dotnetWorker = new Worker(
    './Workers/DocumentReplaceWorker.razor.js',
    { type: 'module' }
);

dotnetWorker.addEventListener('message', e => {
    debugLog('[WorkerClient] Received message from worker:', e.data.command, 'requestId:', e.data.requestId);
    switch (e.data.command) {
        case 'ready':
            debugLog('[WorkerClient] Worker is ready!');
            workerReady = true;
            workerReadyPromiseResolve();
            break;
        case 'response':
            const request = pendingRequests[e.data.requestId];
            if (!request) {
                console.error('[WorkerClient] No pending request for id:', e.data.requestId);
                break;
            }
            delete pendingRequests[e.data.requestId];
            if (e.data.error) {
                console.error('[WorkerClient] Worker returned error:', e.data.error);
                request.reject(new Error(e.data.error));
            } else {
                debugLog('[WorkerClient] Worker returned success, result length:', e.data.result?.length);
                request.resolve(e.data.result);
            }
            break;
        default:
            debugLog('[WorkerClient] Worker said:', e.data);
    }
});

dotnetWorker.addEventListener('error', e => {
    console.error('[WorkerClient] Worker error event:', e.message, e.filename, e.lineno);
    for (const id of Object.keys(pendingRequests)) {
        pendingRequests[id].reject(new Error(`Worker error: ${e.message}`));
        delete pendingRequests[id];
    }
});

dotnetWorker.addEventListener('messageerror', e => {
    console.error('[WorkerClient] Worker messageerror event:', e);
});

async function sendRequestToWorker(request) {
    if (!workerReady) {
        debugLog('[WorkerClient] Worker not ready yet, waiting...');
        await workerReadyPromise;
        debugLog('[WorkerClient] Worker is now ready, proceeding.');
    }
    pendingRequestId++;
    const currentId = pendingRequestId;
    debugLog('[WorkerClient] Sending request to worker, id:', currentId, 'command:', request.command);
    const promise = new Promise((resolve, reject) => {
        pendingRequests[currentId] = { resolve, reject };
    });
    dotnetWorker.postMessage({ ...request, requestId: currentId });
    return promise;
}

export async function replaceDocument(fileBytesBase64, replacementsJson) {
    debugLog('[WorkerClient] replaceDocument called, base64 length:', fileBytesBase64?.length, 'json length:', replacementsJson?.length);
    const result = await sendRequestToWorker({
        command: 'replace',
        fileBytesBase64,
        replacementsJson
    });
    debugLog('[WorkerClient] replaceDocument completed, result length:', result?.length);
    return result;
}
