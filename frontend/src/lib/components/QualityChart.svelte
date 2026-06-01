<script lang="ts">
    import { Line } from "svelte-chartjs";
    import { type ChartData, type ChartOptions } from "chart.js";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { themeState } from "$lib/theme.svelte";

    let { modelId }: { modelId: number | null } = $props();

    let chartLabels = $state<string[]>([]);
    let chartValues = $state<number[]>([]);
    let featureData = $state<Record<string, (number | null)[]>>({});

    let hasFeatureData = $derived(
        Object.values(featureData).some((data) => data.some((val) => val !== null))
    );

    const colors = [
        "rgb(239, 68, 68)",
        "rgb(34, 197, 94)",
        "rgb(234, 179, 8)",
        "rgb(168, 85, 247)",
        "rgb(236, 72, 153)",
        "rgb(20, 184, 166)",
        "rgb(249, 115, 22)",
    ];

    let chartData = $derived<ChartData<"line">>({
        labels: chartLabels,
        datasets: [
            {
                label: `Quality (%) - ${modelId == null ? "Active Model" : `Model ${modelId}`}`,
                data: chartValues,
                borderColor: "rgb(59, 130, 246)",
                backgroundColor: "rgba(59, 130, 246, 0.5)",
                tension: 0.3,
                pointRadius: 2,
                yAxisID: "y",
            },
            ...Object.entries(featureData).map(([featureName, data], index) => {
                const color = colors[index % colors.length];
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

    let chartOptions = $derived<ChartOptions<"line">>({
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
        featureImportances: {
            feature: string;
            importance: number;
        }[] | null;
    };

    $effect(() => {
        let eventSource = new ReconnectingEventSource(
            `https://localhost:5297/metrics-stream${modelId != null ? `?modelId=${modelId}` : ""}`,
        );

        eventSource.onmessage = (event: MessageEvent): void => {
            const metrics = JSON.parse(event.data as string) as ModelMetrics;

            const now = new Date();
            const timeLabel =
                `${now.getHours()}` +
                `:${now.getMinutes().toString().padStart(2, "0")}` +
                `:${now.getSeconds().toString().padStart(2, "0")}`;

            chartLabels = [...chartLabels, timeLabel];
            chartValues = [...chartValues, metrics.quality * 100];

            let newFeatureData = { ...featureData };

            if (metrics.featureImportances != null) {
                metrics.featureImportances.forEach((f) => {
                    if (!newFeatureData[f.feature]) {
                        newFeatureData[f.feature] = new Array(chartLabels.length - 1).fill(null);
                    }
                });
            }

            for (const key of Object.keys(newFeatureData)) {
                const feature = metrics.featureImportances?.find((f) => f.feature === key);
                newFeatureData[key].push(feature ? feature.importance : null);
            }

            if (chartLabels.length > 50) {
                chartLabels = chartLabels.slice(-50);
                chartValues = chartValues.slice(-50);
                for (const key of Object.keys(newFeatureData)) {
                    newFeatureData[key] = newFeatureData[key].slice(-50);
                }
            }

            featureData = newFeatureData;
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
