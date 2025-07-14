window.pwaUpdate = {
    checkForUpdate: async function () {
        if (!navigator.serviceWorker) return false;

        const registration = await navigator.serviceWorker.ready;
        if (registration.waiting) {
            console.log("🟡 Nouvelle version en attente");
            return true;
        }

        navigator.serviceWorker.addEventListener('updatefound', () => {
            console.log("🔄 Nouvelle version détectée");
        });

        return false;
    },

    reload: function () {
        navigator.serviceWorker.ready.then(reg => {
            if (reg.waiting) {
                reg.waiting.postMessage({ type: 'SKIP_WAITING' });
                reg.waiting.addEventListener('statechange', (e) => {
                    if (e.target.state === 'activated') {
                        window.location.reload();
                    }
                });
            }
        });
    }
};
