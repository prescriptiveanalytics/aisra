<script lang="ts">
    import { Line } from "svelte-chartjs";
    import { type ChartData, type ChartOptions } from "chart.js";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { themeState } from "$lib/theme.svelte";
    import { apiBase } from "$lib/config";
    import { CHART_COLORS, MAX_DATA_POINTS } from "$lib/utils/chart";
    import { formatTimeLabel } from "$lib/utils/time";

    const { modelId }: { modelId: number | null } = $props();

    let activeModelId = $state<number | null>(null);
    let chartLabels = $state<string[]>([]);
    let chartValues = $state<number[]>([]);
    let featureData = $state<Record<string, (number | null)[]>>({});

    const hasFeatureData = $derived(
        Object.values(featureData).some((data) => data.some((val) => val !== null)),
    );

    const chartData = $derived<ChartData<"line">>({
        labels: chartLabels,
        datasets: [
            {
                label: `Quality (%) - ${modelId == null ? (activeModelId == null ? "Active Model (none)" : `Active Model (ID: ${activeModelId})`) : `Model ${modelId}`}`,
                data: chartValues,
                borderColor: "rgb(59, 130, 246)",
                backgroundColor: "rgba(59, 130, 246, 0.5)",
                tension: 0.3,
                pointRadius: 2,
                yAxisID: "y",
            },
            ...Object.entries(featureData).map(([featureName, data], index) => {
                const color = CHART_COLORS[index % CHART_COLORS.length];
                return {
                    label: `Importance: ${featureName}`,
                    data: data,
                    borderColor: color,
                    backgroundColor: color.replace("rgb", "rgba").replace(")", ", 0.5)"),
                    spanGaps: false,
                    tension: 0.3,
                    pointRadius: 2,
                    yAxisID: "y1",
                    borderDash: [5, 5],
                };
            }),
        ],
    });

    const chartOptions = $derived<ChartOptions<"line">>({
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            y: {
                min: 0,
                max: 100,
                title: {
                    display: true,
                    text: "Quality %",
                    color: themeState.current === "dark" ? "#e5e7eb" : "#374151",
                },
                grid: {
                    color: themeState.current === "dark" ? "#374151" : "#e5e7eb",
                },
                ticks: {
                    color: themeState.current === "dark" ? "#9ca3af" : "#6b7280",
                },
            },
            y1: {
                type: "linear",
                display: hasFeatureData,
                position: "right",
                min: 0,
                title: {
                    display: true,
                    text: "Importance",
                    color: themeState.current === "dark" ? "#e5e7eb" : "#374151",
                },
                grid: {
                    drawOnChartArea: false,
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
    });

    type ModelMetrics = {
        quality: number;
        featureImportances:
            | {
                  feature: string;
                  importance: number;
              }[]
            | null;
    };

    $effect(() => {
        const eventSource = new ReconnectingEventSource(
            `${apiBase}/api/metrics-stream${modelId != null ? `?modelId=${modelId}` : ""}`,
        );

        eventSource.onmessage = (event: MessageEvent): void => {
            const metrics = JSON.parse(event.data as string) as ModelMetrics;
            const timeLabel = formatTimeLabel();

            chartLabels = [...chartLabels, timeLabel];
            chartValues = [...chartValues, metrics.quality * 100];

            const newFeatureData = { ...featureData };

            if (metrics.featureImportances != null) {
                for (const f of metrics.featureImportances) {
                    if (newFeatureData[f.feature] == null) {
                        newFeatureData[f.feature] = new Array(chartLabels.length - 1).fill(
                            null,
                        ) as (number | null)[];
                    }
                }
            }

            for (const key of Object.keys(newFeatureData)) {
                const feature = metrics.featureImportances?.find((f) => f.feature === key);
                newFeatureData[key].push(feature ? feature.importance : null);
            }

            if (chartLabels.length > MAX_DATA_POINTS) {
                chartLabels = chartLabels.slice(-MAX_DATA_POINTS);
                chartValues = chartValues.slice(-MAX_DATA_POINTS);
                for (const key of Object.keys(newFeatureData)) {
                    newFeatureData[key] = newFeatureData[key].slice(-MAX_DATA_POINTS);
                }
            }

            featureData = newFeatureData;
        };

        return () => {
            eventSource.close();
        };
    });

    $effect(() => {
        if (modelId != null) {
            return;
        }

        const eventSource = new ReconnectingEventSource(`${apiBase}/api/current-model`);

        eventSource.onmessage = (event: MessageEvent): void => {
            const data = event.data as string;
            activeModelId = data === "null" ? null : Number.parseInt(data);
        };

        return () => {
            eventSource.close();
        };
    });
</script>

<div
    class="h-100 w-full rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900"
>
    <Line data={chartData} options={chartOptions} />
</div>
