<script lang="ts">
    import { Line } from "svelte-chartjs";
    import { type ChartData, type ChartOptions } from "chart.js";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { themeState } from "$lib/theme.svelte";

    let { modelId }: { modelId: number | null } = $props();

    let chartLabels = $state<string[]>([]);
    let chartValues = $state<number[]>([]);

    let chartData = $derived<ChartData<"line">>({
        labels: chartLabels,
        datasets: [
            {
                label: `Quality (%) - ${modelId == null ? "Current Model" : `Model ${modelId}`}`,
                data: chartValues,
                borderColor: "rgb(59, 130, 246)",
                backgroundColor: "rgba(59, 130, 246, 0.5)",
                tension: 0.3,
                pointRadius: 2,
            },
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

    $effect(() => {
        let eventSource = new ReconnectingEventSource(
            `https://localhost:5297/quality-stream${modelId != null ? `?modelId=${modelId}` : ""}`,
        );

        eventSource.onmessage = (event: MessageEvent): void => {
            const num = parseFloat(event.data as string);
            if (!isNaN(num)) {
                const percent = num * 100;
                const now = new Date();
                const timeLabel =
                    `${now.getHours()}` +
                    `:${now.getMinutes().toString().padStart(2, "0")}` +
                    `:${now.getSeconds().toString().padStart(2, "0")}`;

                chartLabels = [...chartLabels, timeLabel];
                chartValues = [...chartValues, percent];

                if (chartLabels.length > 50) {
                    chartLabels = chartLabels.slice(-50);
                    chartValues = chartValues.slice(-50);
                }
            }
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
