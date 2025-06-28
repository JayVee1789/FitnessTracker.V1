/* Manifest version: 5A6Eqd1r */
// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });
self.addEventListener('install', () => self.skipWaiting());

self.addEventListener('activate', (event) => {
    event.waitUntil(clients.claim());
});

//self.addEventListener('message', async event => {
//    if (event.data === 'check-for-update') {
//        const clients = await self.clients.matchAll({ type: 'window' });
//        for (const client of clients) {
//            client.postMessage({ type: 'update-available' });
//        }
//    }
//});

