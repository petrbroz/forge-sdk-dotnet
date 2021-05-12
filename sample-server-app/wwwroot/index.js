window.addEventListener('DOMContentLoaded', async function () {
    const viewer = await initViewer(document.getElementById('viewer'));
    updateBucketList(viewer);
});

async function initViewer(container) {
    async function getAccessToken(callback) {
        const resp = await fetch('/api/auth/token');
        if (resp.ok) {
            const { access_token, expires_in } = await resp.json();
            callback(access_token, expires_in);
        } else {
            console.error(await resp.text());
            alert('Could not obtain access token. See the console for more details.');
        }
    }
    return new Promise(function (resolve, reject) {
        Autodesk.Viewing.Initializer({ getAccessToken }, async function () {
            const config = { extensions: [] };
            const viewer = new Autodesk.Viewing.GuiViewer3D(container, config);
            viewer.start();
            resolve(viewer);
        });
    });
}

function loadModel(viewer, urn) {
    function onDocumentLoadSuccess(doc) {
        viewer.loadDocumentNode(doc, doc.getRoot().getDefaultGeometry());
    }
    function onDocumentLoadFailure(code, message) {
        console.error(message);
        alert('Could not load model. See the console for more details.');
    }
    if (urn) {
        Autodesk.Viewing.Document.load('urn:' + urn, onDocumentLoadSuccess, onDocumentLoadFailure);
    }
}

async function updateBucketList(viewer) {
    const el = document.getElementById('buckets');
    const resp = await fetch('/api/buckets');
    if (resp.ok) {
        const buckets = await resp.json();
        el.innerHTML = buckets.map(b => `<option value="${b.name}">${b.name}</option>`).join('\n');
    } else {
        consle.error(await resp.text());
        alert('Could not retrievee buckets. See the console for more details.');
    }
    el.onchange = () => updateObjectList(el.value, viewer);
    updateObjectList(el.value, viewer);
}

async function updateObjectList(bucketKey, viewer) {
    const el = document.getElementById('objects');
    if (!bucketKey) {
        el.innerHTML = '';
        return;
    }
    const resp = await fetch(`/api/buckets/${bucketKey}/objects`);
    if (resp.ok) {
        const objects = await resp.json();
        el.innerHTML = objects.map(o => `<option value="${o.urn}">${o.name}</option>`).join('\n');
    } else {
        consle.error(await resp.text());
        alert('Could not retrievee objects. See the console for more details.');
    }
    el.onchange = () => loadModel(viewer, el.value);
    loadModel(viewer, el.value);
}
