﻿window.blazorCulture = {
    get: () => window.localStorage['BlazorCulture'],
    set: (value) => window.localStorage['BlazorCulture'] = value
};

function getFileUrl(fileContent) {
    var data = new Blob([fileContent], { type: 'text/plain' });
    return window.URL.createObjectURL(data);
}

function downloadFileResult(fileUrl,fileName) {
    let link = document.createElement("a");
    link.download = fileName;
    link.href = fileUrl;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}


// URL pointing to the Blob with the file contents
var objUrl = null;
// create the blob with file content, and attach the URL to the downloadlink;
// NB: link must have the download attribute
// this method can go to your library
function exportFile(fileContent, fileName, downloadLinkId) {
    // revoke the old object URL to avoid memory leaks.
    if (objUrl !== null) {
        window.URL.revokeObjectURL(objUrl);
    }
    // create the object that contains the file data and that can be referred to with a URL
    var data = new Blob([fileContent], { type: 'text/plain' });
    objUrl = window.URL.createObjectURL(data);
    // attach the object to the download link (styled as button)
    var downloadLinkButton = document.getElementById(downloadLinkId);
    downloadLinkButton.download = fileName;
    downloadLinkButton.href = objUrl;
};

Blazor.start({
    loadBootResource: function (type, name, defaultUri, integrity) {
        // For framework resources, use the precompressed .br files for faster downloads
        // This is needed only because GitHub pages doesn't natively support Brotli (or even gzip for .dll files)
        if (type !== 'dotnetjs' && location.hostname !== 'localhost') {
            return (async function () {
                const response = await fetch(defaultUri + '.br', { cache: 'no-cache' });
                if (!response.ok) {
                    throw new Error(response.statusText);
                }
                const originalResponseBuffer = await response.arrayBuffer();
                const originalResponseArray = new Int8Array(originalResponseBuffer);
                const decompressedResponseArray = BrotliDecode(originalResponseArray);
                const contentType = type === 'dotnetwasm' ? 'application/wasm' : 'application/octet-stream';
                return new Response(decompressedResponseArray, { headers: { 'content-type': contentType } });
            })();
        }
    }
});

// Single Page Apps for GitHub Pages
// https://github.com/rafrex/spa-github-pages
// Copyright (c) 2016 Rafael Pedicini, licensed under the MIT License
// ----------------------------------------------------------------------
// This script checks to see if a redirect is present in the query string
// and converts it back into the correct url and adds it to the
// browser's history using window.history.replaceState(...),
// which won't cause the browser to attempt to load the new url.
// When the single page app is loaded further down in this file,
// the correct url will be waiting in the browser's history for
// the single page app to route accordingly.
(function (l) {
    if (l.search) {
        var q = {};
        l.search.slice(1).split('&').forEach(function (v) {
            var a = v.split('=');
            q[a[0]] = a.slice(1).join('=').replace(/~and~/g, '&');
        });
        if (q.p !== undefined) {
            window.history.replaceState(null, null,
                l.pathname.slice(0, -1) + (q.p || '') +
                (q.q ? ('?' + q.q) : '') +
                l.hash
            );
        }
    }
}(window.location))