<script lang="ts">
    import QualityChart from "$lib/components/QualityChart.svelte";

    const {
        selectedCharts,
        onRemoveChart,
    }: {
        selectedCharts: (number | null)[];
        onRemoveChart: (modelId: number | null) => void;
    } = $props();
</script>

<div class="grid grid-cols-1 gap-4 3xl:grid-cols-2">
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
