//window.listenForServiceWorkerUpdates = function (dotNetHelper) {
//    if ('serviceWorker' in navigator) {
//        navigator.serviceWorker.addEventListener('controllerchange', () => {
//            const justReloaded = sessionStorage.getItem('justReloaded');
//            if (justReloaded === 'true') {
//                console.log("🔁 Reload détecté, pas d'alerte");
//                sessionStorage.removeItem('justReloaded');
//                return;
//            }

//            console.log("🆕 MAJ détectée, alerte activée");
//            dotNetHelper.invokeMethodAsync('NotifyUpdateAvailable');
//        });
//    }
//};

//window.checkForSWUpdate = function () {
//    if (navigator.serviceWorker.controller) {
//        navigator.serviceWorker.controller.postMessage("check-for-update");
//    } else {
//        console.warn("⚠️ Aucun SW contrôleur actif");
//    }
//};

//window.forceFullReload = function () {
//    sessionStorage.setItem('justReloaded', 'true');
//    window.location.reload(true);
//};
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
