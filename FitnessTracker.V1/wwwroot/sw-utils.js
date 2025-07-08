
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
    window.location.reload(true);
};
