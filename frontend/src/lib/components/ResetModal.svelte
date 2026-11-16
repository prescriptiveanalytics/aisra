<script lang="ts">
    import { apiFetch } from "$lib/auth.svelte";
    import { isReloading, reloadPage } from "$lib/reload";

    const {
        isOpen,
        close,
    }: {
        isOpen: boolean;
        close: () => void;
    } = $props();

    let busy = $state(false);
    let error = $state<string | null>(null);

    async function handleReset(): Promise<void> {
        busy = true;
        error = null;

        try {
            const res = await apiFetch("/api/reset", { method: "POST" });
            if (!res.ok) {
                throw new Error();
            }
            // The backend also broadcasts a "reset" event, but this page may not be connected to the event stream.
            reloadPage();
        } catch {
            // The "reset" event may have arrived first and reloaded the page, which cancels this request.
            if (isReloading()) {
                return;
            }
            error = "The application could not be reset.";
            busy = false;
        }
    }
</script>

{#if isOpen}
    <div role="dialog" class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
        <div
            class="relative z-50 flex w-full max-w-md flex-col gap-4 rounded-xl bg-white p-6 shadow-2xl dark:bg-gray-800"
        >
            <h2 class="text-2xl font-semibold text-gray-800 dark:text-gray-100">
                Reset Application?
            </h2>
            <p class="text-sm text-gray-700 dark:text-gray-300">
                This stops the agent and the data stream, clears the message history, deletes all
                recorded data and trained models, and restores the default settings. This cannot be
                undone.
            </p>

            {#if error}
                <p class="text-sm text-red-600 dark:text-red-400">{error}</p>
            {/if}

            <div class="flex justify-end gap-2">
                <button
                    type="button"
                    disabled={busy}
                    onclick={() => close()}
                    class="cursor-pointer rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:text-gray-200 dark:hover:bg-gray-700"
                >
                    Cancel
                </button>
                <button
                    type="button"
                    disabled={busy}
                    onclick={handleReset}
                    class="cursor-pointer rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50"
                >
                    {busy ? "Resetting…" : "Reset"}
                </button>
            </div>
        </div>
    </div>
{/if}
