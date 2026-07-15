self.addEventListener('install', () => {
    console.log('Service worker installed');
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    console.log('Service worker activated');
    event.waitUntil(clients.claim());
});

self.addEventListener('message', event => {
    if (event.data?.type === 'SKIP_WAITING') {
        console.log('Service worker skip waiting');
        self.skipWaiting();
    }
});
