/* Manifest version: 5o6lzgo7 */
// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });
self.addEventListener('install', () => self.skipWaiting());

self.addEventListener('activate', (event) => {
    event.waitUntil(clients.claim());
});

self.addEventListener('message', async event => {
    if (event.data === 'check-for-update') {
        const clients = await self.clients.matchAll({ type: 'window' });
        for (const client of clients) {
            client.postMessage({ type: 'update-available' });
        }
    }
});
self.addEventListener('message', async event => {
    if (event.data === 'check-for-update') {
        const allClients = await self.clients.matchAll({ type: 'window' });
        for (const client of allClients) {
            client.postMessage({ type: 'update-available' });
        }
    }
});
