<script lang="ts">
    import QualityChart from "$lib/components/QualityChart.svelte";

    const {
        selectedCharts,
        onAddChart,
        onRemoveChart,
    }: {
        selectedCharts: (number | null)[];
        onAddChart: () => void;
        onRemoveChart: (modelId: number | null) => void;
    } = $props();
</script>

<div class="mt-8 mb-4 flex items-center justify-between">
    <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">Models</h2>

    <button
        onclick={onAddChart}
        class="cursor-pointer rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
    >
        Add Chart
    </button>
</div>

<div class="grid grid-cols-1 gap-4 2xl:grid-cols-2">
    {#each selectedCharts as chartModelId, index (chartModelId)}
        <div class="mb-4 h-full min-w-0 flex-1 content-end">
            {#if index > 0}
                <div class="flex justify-end">
                    <button
                        onclick={() => onRemoveChart(chartModelId)}
                        class="mb-1 cursor-pointer rounded-lg bg-red-100 px-3 py-1 text-sm text-red-600 hover:bg-red-200 dark:bg-red-900/30 dark:text-red-400 dark:hover:bg-red-900/50"
                        aria-label="Remove chart"
                    >
                        Remove
                    </button>
                </div>
            {/if}
            <QualityChart modelId={chartModelId} />
        </div>
    {/each}
</div>
