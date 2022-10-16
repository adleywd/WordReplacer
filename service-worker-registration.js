﻿window.updateAvailable = new Promise((resolve, reject) => {
    if (!('serviceWorker' in navigator)) {
        const errorMessage = `This browser doesn't support service workers`;
        console.error(errorMessage);
        reject(errorMessage);
        return;
    }

    navigator.serviceWorker.register('./service-worker.js', { updateViaCache: 'none' })
        .then(registration => {
            console.info(`Service worker registration successful (scope: ${registration.scope})`);

            registration.onupdatefound = () => {
                const installingServiceWorker = registration.installing;
                installingServiceWorker.onstatechange = () => {
                    if (installingServiceWorker.state === 'installed') {
                        resolve(!!navigator.serviceWorker.controller);
                    }
                }
            };
        })
        .catch(error => {
            console.error('Service worker registration failed with error:', error);
            reject(error);
        });
});

window.registerForUpdateAvailableNotification = () => {
    window.updateAvailable.then(isUpdateAvailable => {
        if (isUpdateAvailable) {
            document.getElementById('snackbar_app_update').classList.add('mdc-snackbar--open');
        }
    });
};
