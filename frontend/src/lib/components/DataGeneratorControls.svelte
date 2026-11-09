<script lang="ts">
    import { type ModalComponent, type ModalProps, modals } from "svelte-modals";
    import DataGeneratorModal from "$lib/components/DataGeneratorModal.svelte";
    import { generatorState, refreshStatus, restart, start, stop } from "$lib/dataGenerator.svelte";

    const STATUS_POLL_INTERVAL_MS = 2000;

    let busy = $state(false);
    let commandError = $state<string | null>(null);

    const status = $derived(generatorState.status);
    const running = $derived(status?.status === "running");
    const error = $derived(commandError ?? generatorState.error);

    $effect(() => {
        void refreshStatus();
        // The stream also stops on its own once its duration is over.
        const interval = setInterval(() => void refreshStatus(), STATUS_POLL_INTERVAL_MS);
        return () => clearInterval(interval);
    });

    async function run(action: typeof start): Promise<void> {
        busy = true;
        const result = await action();
        busy = false;
        commandError = result.ok ? null : result.error;
    }

    async function openConfiguration(): Promise<void> {
        commandError = null;
        await modals.open(
            DataGeneratorModal as unknown as ModalComponent<ModalProps<unknown>, object, "">,
        );
    }

    const buttonClass =
        "cursor-pointer rounded-lg px-4 py-2 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50";
</script>

<div class="flex flex-col items-end gap-1">
    <div class="flex flex-wrap items-center justify-end gap-2">
        {#if status}
            <span
                class="rounded-full px-3 py-1 text-xs font-medium {running
                    ? 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300'
                    : 'bg-gray-200 text-gray-600 dark:bg-gray-700 dark:text-gray-300'}"
                title={status.generatorId ? `Generator: ${status.generatorId}` : undefined}
            >
                {running ? "Generating" : "Stopped"}
            </span>
        {/if}

        {#if running}
            {#if status?.hasPendingConfiguration}
                <button
                    type="button"
                    disabled={busy}
                    onclick={() => run(restart)}
                    class="{buttonClass} bg-amber-500 text-white hover:bg-amber-600"
                    title="A new configuration was saved while the stream was running."
                >
                    Restart to Apply
                </button>
            {/if}
            <button
                type="button"
                disabled={busy}
                onclick={() => run(stop)}
                class="{buttonClass} bg-red-600 text-white hover:bg-red-700"
            >
                Stop
            </button>
        {:else}
            <button
                type="button"
                disabled={busy || status === null}
                onclick={() => run(start)}
                class="{buttonClass} bg-green-600 text-white hover:bg-green-700"
            >
                Start
            </button>
        {/if}

        <button
            type="button"
            onclick={openConfiguration}
            class="{buttonClass} border border-gray-300 text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-200 dark:hover:bg-gray-700"
        >
            Configure
        </button>
    </div>

    {#if error}
        <p class="text-sm text-red-600 dark:text-red-400">{error}</p>
    {/if}
</div>
