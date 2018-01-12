function JsFileRead(ev)
{
    var reader = new FileReader();
    reader.onloadend = function (evt) {
        YataOnline.MainClass.LoadFile(new Uint8Array(evt.target.result));
    }
	reader.readAsArrayBuffer(ev[0]);
	
}

function JsImageRead(ev) {
    if (!(ev[0].name.endsWith('png') || ev[0].name.endsWith('jpg') || ev[0].name.endsWith('jpeg')))
    {
        alert('Only png and jpg files are supported');
        return;
    }
    var reader = new FileReader();
    reader.onloadend = function (evt) {
        YataOnline.MainClass.LoadImage(new Uint8Array(evt.target.result));
    }
    reader.readAsArrayBuffer(ev[0]);
}

downloadBlob = function (data, fileName, mimeType) {
    var blob, url;
    blob = new Blob([data], {
        type: mimeType
    });
    url = window.URL.createObjectURL(blob);
    downloadURL(url, fileName, mimeType);
    setTimeout(function () {
        return window.URL.revokeObjectURL(url);
    }, 1000);
};

downloadURL = function (data, fileName) {
    var a;
    a = document.createElement('a');
    a.href = data;
    a.download = fileName;
    document.body.appendChild(a);
    a.style = 'display: none';
    a.click();
    a.remove();
};