(function () {
    const state = {
        installPrompt: null,
        registration: null,
        updateWorker: null,
        refreshing: false
    };

    const selectors = {
        install: 'pwa-install-banner',
        update: 'pwa-update-banner',
        offline: 'pwa-offline-chip'
    };

    function isStandalone() {
        return window.matchMedia('(display-mode: standalone)').matches
            || window.navigator.standalone === true;
    }

    function isIos() {
        return /iphone|ipad|ipod/i.test(navigator.userAgent)
            || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
    }

    function show(id) {
        document.getElementById(id)?.classList.add('is-visible');
    }

    function hide(id) {
        document.getElementById(id)?.classList.remove('is-visible');
    }

    function updateOfflineChip() {
        if (navigator.onLine) {
            hide(selectors.offline);
        } else {
            show(selectors.offline);
        }
    }

    function createPwaUi() {
        if (document.getElementById(selectors.install))
            return;

        document.body.insertAdjacentHTML('beforeend', `
            <div id="${selectors.offline}" class="pwa-offline-chip">Hors ligne</div>

            <div id="${selectors.install}" class="pwa-banner" role="status" aria-live="polite">
                <div>
                    <strong>Installer FitnessTracker</strong>
                    <span>Accès rapide, plein écran et meilleure expérience mobile.</span>
                </div>
                <div class="pwa-banner-actions">
                    <button type="button" class="pwa-banner-secondary" data-pwa-dismiss-install>Plus tard</button>
                    <button type="button" class="pwa-banner-primary" data-pwa-install>Installer</button>
                </div>
            </div>

            <div id="${selectors.update}" class="pwa-banner" role="status" aria-live="polite">
                <div>
                    <strong>Nouvelle version disponible</strong>
                    <span>Recharge pour appliquer les dernières corrections.</span>
                </div>
                <div class="pwa-banner-actions">
                    <button type="button" class="pwa-banner-secondary" data-pwa-dismiss-update>Plus tard</button>
                    <button type="button" class="pwa-banner-primary" data-pwa-update>Mettre à jour</button>
                </div>
            </div>
        `);

        document.querySelector('[data-pwa-install]')?.addEventListener('click', install);
        document.querySelector('[data-pwa-update]')?.addEventListener('click', applyUpdate);
        document.querySelector('[data-pwa-dismiss-install]')?.addEventListener('click', () => {
            sessionStorage.setItem('pwa-install-dismissed', 'true');
            hide(selectors.install);
        });
        document.querySelector('[data-pwa-dismiss-update]')?.addEventListener('click', () => hide(selectors.update));
        updateOfflineChip();
    }

    function showIosInstallHint() {
        if (!isIos() || isStandalone() || sessionStorage.getItem('pwa-install-dismissed') === 'true')
            return;

        const installBanner = document.getElementById(selectors.install);
        const primaryButton = installBanner?.querySelector('[data-pwa-install]');
        const text = installBanner?.querySelector('span');

        if (text)
            text.textContent = 'Sur iPhone: Partager puis Ajouter à l’écran d’accueil.';

        if (primaryButton)
            primaryButton.textContent = 'OK';

        show(selectors.install);
    }

    async function install() {
        if (!state.installPrompt) {
            if (isIos()) {
                sessionStorage.setItem('pwa-install-dismissed', 'true');
                hide(selectors.install);
            }
            return;
        }

        hide(selectors.install);
        state.installPrompt.prompt();
        await state.installPrompt.userChoice.catch(() => null);
        state.installPrompt = null;
    }

    function trackInstalling(worker) {
        if (!worker)
            return;

        worker.addEventListener('statechange', () => {
            if (worker.state === 'installed' && navigator.serviceWorker.controller) {
                state.updateWorker = worker;
                show(selectors.update);
            }
        });
    }

    async function registerServiceWorker() {
        if (!('serviceWorker' in navigator))
            return null;

        const isLocalhost = ['localhost', '127.0.0.1', '[::1]'].includes(location.hostname);
        if (isLocalhost)
            return null;

        state.registration = await navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' });

        if (state.registration.waiting) {
            state.updateWorker = state.registration.waiting;
            show(selectors.update);
        }

        trackInstalling(state.registration.installing);
        state.registration.addEventListener('updatefound', () => trackInstalling(state.registration.installing));

        setInterval(() => state.registration?.update(), 60 * 60 * 1000);
        return state.registration;
    }

    function applyUpdate() {
        const worker = state.updateWorker || state.registration?.waiting;
        if (!worker)
            return;

        worker.postMessage({ type: 'SKIP_WAITING' });
    }

    window.pwaUpdate = {
        checkForUpdate: async function () {
            if (!state.registration)
                state.registration = await navigator.serviceWorker?.ready?.catch(() => null);

            await state.registration?.update?.();
            const hasUpdate = !!(state.registration?.waiting || state.updateWorker);
            if (hasUpdate)
                show(selectors.update);

            return hasUpdate;
        },
        reload: applyUpdate,
        install: install,
        isStandalone: isStandalone
    };

    window.addEventListener('beforeinstallprompt', event => {
        event.preventDefault();
        state.installPrompt = event;

        if (!isStandalone() && sessionStorage.getItem('pwa-install-dismissed') !== 'true')
            show(selectors.install);
    });

    window.addEventListener('appinstalled', () => {
        state.installPrompt = null;
        hide(selectors.install);
    });

    window.addEventListener('online', updateOfflineChip);
    window.addEventListener('offline', updateOfflineChip);

    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (state.refreshing)
                return;

            state.refreshing = true;
            window.location.reload();
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        createPwaUi();
        showIosInstallHint();
        registerServiceWorker().catch(error => console.warn('Service worker registration failed', error));
    });
})();
