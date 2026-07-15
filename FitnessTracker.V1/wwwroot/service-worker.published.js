// Production service worker for FitnessTracker.
// Blazor replaces service-worker.js with this file during publish.

self.importScripts('./service-worker-assets.js');

const cacheNamePrefix = 'fitnesstracker-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineUrl = 'offline.html';
const base = '/';
const baseUrl = new URL(base, self.origin);

const offlineAssetsInclude = [
    /\.wasm$/,
    /\.dll$/,
    /\.html$/,
    /\.js$/,
    /\.json$/,
    /\.css$/,
    /\.woff2?$/,
    /\.png$/,
    /\.jpe?g$/,
    /\.gif$/,
    /\.ico$/,
    /\.svg$/,
    /\.dat$/,
    /\.blat$/
];

const offlineAssetsExclude = [
    /^service-worker\.js$/,
    /^service-worker\.published\.js$/,
    /^service-worker-assets\.js$/,
    /\.pdb$/,
    /\.map$/
];

const manifestUrlList = new Set(
    self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href)
);

self.addEventListener('install', event => {
    event.waitUntil(onInstall());
});

self.addEventListener('activate', event => {
    event.waitUntil(onActivate());
});

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET')
        return;

    event.respondWith(onFetch(event));
});

self.addEventListener('message', event => {
    if (event.data?.type === 'SKIP_WAITING')
        self.skipWaiting();
});

async function onInstall() {
    const cache = await caches.open(cacheName);

    const assetRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, {
            integrity: asset.hash,
            cache: 'no-cache'
        }));

    await addAllSettled(cache, [
        new Request('index.html', { cache: 'no-cache' }),
        new Request(offlineUrl, { cache: 'no-cache' }),
        ...assetRequests
    ]);

    await self.skipWaiting();
}

async function onActivate() {
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => (key.startsWith(cacheNamePrefix) || key.startsWith('offline-cache-')) && key !== cacheName)
        .map(key => caches.delete(key)));

    await self.clients.claim();
}

async function onFetch(event) {
    const request = event.request;
    const url = new URL(request.url);

    if (url.origin !== self.location.origin)
        return fetch(request);

    if (request.mode === 'navigate')
        return navigationHandler(request);

    if (manifestUrlList.has(request.url))
        return cacheFirst(request);

    return networkFirst(request);
}

async function navigationHandler(request) {
    try {
        const networkResponse = await fetch(request);
        const cache = await caches.open(cacheName);
        await cache.put('index.html', networkResponse.clone());
        return networkResponse;
    } catch {
        const cache = await caches.open(cacheName);
        return await cache.match('index.html')
            || await cache.match(offlineUrl)
            || Response.error();
    }
}

async function cacheFirst(request) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);

    if (cachedResponse)
        return cachedResponse;

    const networkResponse = await fetch(request);
    if (networkResponse.ok)
        await cache.put(request, networkResponse.clone());

    return networkResponse;
}

async function networkFirst(request) {
    try {
        const networkResponse = await fetch(request);

        if (networkResponse.ok && request.url.startsWith(self.location.origin)) {
            const cache = await caches.open(cacheName);
            await cache.put(request, networkResponse.clone());
        }

        return networkResponse;
    } catch {
        const cache = await caches.open(cacheName);
        return await cache.match(request)
            || await cache.match(offlineUrl)
            || Response.error();
    }
}

async function addAllSettled(cache, requests) {
    const results = await Promise.allSettled(
        requests.map(async request => {
            const response = await fetch(request);
            if (response.ok)
                await cache.put(request, response);
        })
    );

    const rejected = results.filter(result => result.status === 'rejected').length;
    if (rejected > 0)
        console.warn(`Service worker: ${rejected} assets were not cached.`);
}
