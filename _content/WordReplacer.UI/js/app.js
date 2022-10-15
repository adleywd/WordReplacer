window.downloadFileFromStream = function (fileName, bytesBase64, mimiType) {
    var fileUrl = "data:" + mimiType + ";base64," + bytesBase64;
    fetch(fileUrl)
        .then(response => response.blob())
        .then(blob => {
            let link = window.document.createElement("a");
            link.href = window.URL.createObjectURL(blob);
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        });
}

window.reloadPage = function() {
    window.location.reload();
}