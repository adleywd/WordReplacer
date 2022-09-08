//async function downloadImage(filename, fileStream) {
//    const arrayBuffer = await fileStream.arrayBuffer();
//    const blob = new Blob([arrayBuffer]);

//    const url = URL.createObjectURL(blob);

//    triggerDownload(filename, url);

//    URL.revokeObjectURL(url);
//}

//function triggerDownload(filename, url) {
//    const anchorElement = document.createElement("a");
//    anchorElement.href = url;

//    anchorElement.download = filename;

//    anchorElement.click();
//    anchorElement.remove();
//}

//window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    //const arrayBuffer = await contentStreamReference.arrayBuffer();
    //const blob = new Blob([arrayBuffer]);
    //const url = URL.createObjectURL(blob);
    //const anchorElement = document.createElement('a');
    //anchorElement.href = url;
    //anchorElement.download = fileName ?? '';
    //anchorElement.click();
    //anchorElement.remove();
    //URL.revokeObjectURL(url);
//}

window.downloadFileFromStream = function (fileName, bytesBase64) {
    var fileUrl = "data:" + ";base64," + bytesBase64;
    fetch(fileUrl)
        .then(response => response.blob())
        .then(blob => {
            var link = window.document.createElement("a");
            link.href = window.URL.createObjectURL(blob);
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        });
}