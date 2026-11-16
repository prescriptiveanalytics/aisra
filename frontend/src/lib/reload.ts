let reloading = false;

/**
 * Reloads the page so that all state shown in the frontend is fetched again.
 */
export function reloadPage(): void {
    reloading = true;
    window.location.reload();
}

/**
 * Whether the page is about to reload. Requests that are still pending are then cancelled by the browser, so their
 * failure should not be reported.
 */
export function isReloading(): boolean {
    return reloading;
}
