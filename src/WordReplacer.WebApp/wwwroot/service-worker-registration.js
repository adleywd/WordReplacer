window.updateAvailable = new Promise((resolve, reject) => {
    if (!('serviceWorker' in navigator)) {
        const errorMessage = `This browser doesn't support service workers`;
        console.error(errorMessage);
        reject(errorMessage);
        return;
    }

    const hadControllerBeforeRegister = !!navigator.serviceWorker.controller;

    navigator.serviceWorker.register('./service-worker.js', { updateViaCache: 'none' })
        .then(registration => {
            console.info(`Service worker registration successful (scope: ${registration.scope})`);

            registration.onupdatefound = () => {
                const installingServiceWorker = registration.installing;
                installingServiceWorker.onstatechange = () => {
                    if (installingServiceWorker.state === 'installed') {
                        resolve(hadControllerBeforeRegister && !!navigator.serviceWorker.controller);
                    }
                }
            };
        })
        .catch(error => {
            console.error('Service worker registration failed with error:', error);
            reject(error);
        });

    // Handle the case where skipWaiting() causes a new SW to take control
    // while the page is already open. This fires after the promise above
    // may have already resolved, so it acts as a fallback notification.
    navigator.serviceWorker.addEventListener('controllerchange', () => {
        if (hadControllerBeforeRegister) {
            resolve(true);
        }
    });
});

window.registerForUpdateAvailableNotification = (applicationUpdateObjRef, methodName) => {
    window.updateAvailable.then(isUpdateAvailable => {
        if (isUpdateAvailable) {
            applicationUpdateObjRef.invokeMethodAsync(methodName).then();
        }
    });
};
