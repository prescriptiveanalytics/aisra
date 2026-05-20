<script lang="ts">
    let {
        isOpen,
        close,
        models,
        selectedCharts,
        onConfirm,
    }: {
        isOpen: boolean;
        close: () => void;
        models: { id: number; model: string }[];
        selectedCharts: (number | null)[];
        onConfirm: (modelId: number) => void;
    } = $props();

    function handleConfirm(id: number): void {
        onConfirm(id);
        close();
    }
</script>

{#if isOpen}
    <div role="dialog" class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
        <div class="pointer-events-none fixed inset-0 bg-black/50 transition-opacity"></div>

        <div
            class="relative z-50 flex max-h-full w-full max-w-4xl flex-col rounded-xl bg-white shadow-2xl dark:bg-gray-800"
        >
            <div
                class="flex items-center justify-between border-b border-gray-200 p-6 dark:border-gray-700"
            >
                <h2 class="text-2xl font-semibold text-gray-800 dark:text-gray-100">
                    Select a Model to Add
                </h2>
                <button
                    onclick={() => close()}
                    class="cursor-pointer rounded-lg p-2 text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700"
                    aria-label="Close modal"
                >
                    <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M6 18L18 6M6 6l12 12"
                        />
                    </svg>
                </button>
            </div>

            <div class="flex-1 overflow-y-auto p-6">
                {#if models.length === 0}
                    <p class="text-gray-600 dark:text-gray-400">No models available.</p>
                {:else}
                    <div class="flex flex-col gap-4">
                        {#each models as model (model.id)}
                            {@const isSelected = selectedCharts.includes(model.id)}
                            <div
                                role="button"
                                tabindex={isSelected ? -1 : 0}
                                onclick={() => {
                                    if (!isSelected) {
                                        handleConfirm(model.id);
                                    }
                                }}
                                onkeydown={(e) => {
                                    if (!isSelected && (e.key === "Enter" || e.key === " ")) {
                                        e.preventDefault();
                                        handleConfirm(model.id);
                                    }
                                }}
                                class="flex cursor-pointer flex-col gap-2 rounded-lg border border-gray-200 p-4 transition-colors hover:border-blue-500 dark:border-gray-700 dark:hover:border-blue-400 {isSelected
                                    ? 'pointer-events-none opacity-50'
                                    : ''}"
                            >
                                <div class="flex items-center justify-between">
                                    <span class="font-medium text-gray-800 dark:text-gray-200"
                                        >Model {model.id}</span
                                    >
                                    {#if isSelected}
                                        <span
                                            class="rounded bg-gray-200 px-2 py-1 text-xs font-medium text-gray-600 dark:bg-gray-700 dark:text-gray-400"
                                            >Added</span
                                        >
                                    {/if}
                                </div>
                                <div class="rounded bg-gray-50 p-3 dark:bg-gray-900">
                                    <pre
                                        class="text-sm break-all whitespace-pre-wrap text-gray-700 dark:text-gray-300"><code
                                            >{model.model}</code
                                        ></pre>
                                </div>
                            </div>
                        {/each}
                    </div>
                {/if}
            </div>
        </div>
    </div>
{/if}
