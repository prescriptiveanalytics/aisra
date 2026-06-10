<script lang="ts">
    import { Line } from "svelte-chartjs";
    import { type ChartData, type ChartOptions } from "chart.js";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { themeState } from "$lib/theme.svelte";
    import { apiBase } from "$lib/config";
    import { CHART_COLORS, MAX_DATA_POINTS } from "$lib/utils/chart";
    import { formatTimeLabel } from "$lib/utils/time";

    type DataPoint = {
        id: string;
        value: number;
    };

    type Series = {
        id: string;
        labels: string[];
        data: number[];
    };

    let series = $state<Series[]>([]);

    $effect(() => {
        const eventSource = new ReconnectingEventSource(`${apiBase}/data-stream`);

        eventSource.onmessage = (event: MessageEvent): void => {
            const dataPoint = JSON.parse(event.data as string) as DataPoint;

            const timeLabel = formatTimeLabel();

            let found = false;
            const newSeries = series.map((s) => {
                if (s.id === dataPoint.id) {
                    found = true;
                    let newLabels = [...s.labels, timeLabel];
                    let newData = [...s.data, dataPoint.value];
                    if (newLabels.length > MAX_DATA_POINTS) {
                        newLabels = newLabels.slice(-MAX_DATA_POINTS);
                        newData = newData.slice(-MAX_DATA_POINTS);
                    }
                    return { ...s, labels: newLabels, data: newData };
                }
                return s;
            });

            if (!found) {
                series = [
                    ...newSeries,
                    { id: dataPoint.id, labels: [timeLabel], data: [dataPoint.value] },
                ];
            } else {
                series = newSeries;
            }
        };

        return () => {
            eventSource.close();
        };
    });

    function getChartData(s: Series, colorIndex: number): ChartData<"line"> {
        const color = CHART_COLORS[colorIndex % CHART_COLORS.length];
        return {
            labels: s.labels,
            datasets: [
                {
                    label: s.id,
                    data: s.data,
                    borderColor: color,
                    backgroundColor: color.replace("rgb", "rgba").replace(")", ", 0.5)"),
                    tension: 0.3,
                    pointRadius: 2,
                    yAxisID: "y",
                },
            ],
        };
    }

    function getChartOptions(): ChartOptions<"line"> {
        return {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    title: {
                        display: true,
                        text: "Value",
                        color: themeState.current === "dark" ? "#e5e7eb" : "#374151",
                    },
                    grid: {
                        color: themeState.current === "dark" ? "#374151" : "#e5e7eb",
                    },
                    ticks: {
                        color: themeState.current === "dark" ? "#9ca3af" : "#6b7280",
                    },
                },
                x: {
                    title: {
                        display: true,
                        text: "Time",
                        color: themeState.current === "dark" ? "#e5e7eb" : "#374151",
                    },
                    grid: {
                        color: themeState.current === "dark" ? "#374151" : "#e5e7eb",
                    },
                    ticks: {
                        color: themeState.current === "dark" ? "#9ca3af" : "#6b7280",
                    },
                },
            },
            plugins: {
                legend: {
                    labels: {
                        color: themeState.current === "dark" ? "#e5e7eb" : "#374151",
                    },
                },
            },
            animation: {
                duration: 0,
            },
        };
    }
</script>

{#if series.length > 0}
    <div class="grid grid-cols-1 gap-4 2xl:grid-cols-2">
        {#each series as s, _index (s.id)}
            <div
                class="mb-4 h-64 min-w-0 flex-1 rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900"
            >
                <Line data={getChartData(s, _index)} options={getChartOptions()} />
            </div>
        {/each}
    </div>
{/if}
