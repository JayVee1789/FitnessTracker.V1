
window.listenForServiceWorkerUpdates = function (dotNetHelper) {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            const justReloaded = sessionStorage.getItem('justReloaded');
            if (justReloaded === 'true') {
                sessionStorage.removeItem('justReloaded');
                return;
            }

            dotNetHelper.invokeMethodAsync('NotifyUpdateAvailable');
        });
    }
};

window.forceFullReload = function () {
    sessionStorage.setItem('justReloaded', 'true');
    window.location.reload();
};

window.clearPwaCaches = async function () {
    if (!('caches' in window)) return;

    const keys = await caches.keys();
    await Promise.all(keys
        .filter(key => key.startsWith('fitnesstracker-') || key.startsWith('offline-cache-'))
        .map(key => caches.delete(key)));
};
