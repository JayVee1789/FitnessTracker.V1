self.addEventListener('install', () => {
    console.log("📦 Service worker installé");
    self.skipWaiting(); // active immédiatement
});

self.addEventListener('activate', (event) => {
    console.log("✅ Service worker activé");
    event.waitUntil(clients.claim());
});

// Pour recevoir un message depuis l’app Blazor et forcer l’activation
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        console.log("🚀 Forçage de mise à jour via message");
        self.skipWaiting();
    }
});


