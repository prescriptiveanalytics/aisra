<script lang="ts">
    import { Line } from "svelte-chartjs";
    import { type ChartData, type ChartOptions } from "chart.js";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { themeState } from "$lib/theme.svelte";
    import { apiBase } from "$lib/config";

    type DataPoint = {
        id: string;
        value: number;
    }

    let series = $state<{ id: string; labels: string[]; data: number[] }[]>([]);

    const colors = [
        "rgb(239, 68, 68)",
        "rgb(34, 197, 94)",
        "rgb(234, 179, 8)",
        "rgb(168, 85, 247)",
        "rgb(236, 72, 153)",
        "rgb(20, 184, 166)",
        "rgb(249, 115, 22)",
    ];

    $effect(() => {
        let eventSource = new ReconnectingEventSource(`${apiBase}/data-stream`);

        eventSource.onmessage = (event: MessageEvent): void => {
            const dataPoint = JSON.parse(event.data as string) as DataPoint;

            const now = new Date();
            const timeLabel =
                `${now.getHours()}` +
                `:${now.getMinutes().toString().padStart(2, "0")}` +
                `:${now.getSeconds().toString().padStart(2, "0")}`;

            let found = false;
            let newSeries = series.map((s) => {
                if (s.id === dataPoint.id) {
                    found = true;
                    let newLabels = [...s.labels, timeLabel];
                    let newData = [...s.data, dataPoint.value];
                    if (newLabels.length > 50) {
                        newLabels = newLabels.slice(-50);
                        newData = newData.slice(-50);
                    }
                    return { ...s, labels: newLabels, data: newData };
                }
                return s;
            });

            if (!found) {
                newSeries = [
                    ...newSeries,
                    { id: dataPoint.id, labels: [timeLabel], data: [dataPoint.value] },
                ];
            }

            series = newSeries;
        };

        return () => {
            eventSource.close();
        };
    });

    function getChartData(s: { id: string; labels: string[]; data: number[] }, colorIndex: number): ChartData<"line"> {
        const color = colors[colorIndex % colors.length];
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

    function getChartOptions(index: number): ChartOptions<"line"> {
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

<div class="grid grid-cols-1 gap-4 2xl:grid-cols-2">
    {#each series as s, i}
        <div
            class="mb-4 h-64 min-w-0 flex-1 rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900"
        >
            <Line data={getChartData(s, i)} options={getChartOptions(i)} />
        </div>
    {/each}
</div>
