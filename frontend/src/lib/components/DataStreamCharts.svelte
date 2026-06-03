<script lang="ts">
    import { Line } from "svelte-chartjs";
    import { type ChartData, type ChartOptions } from "chart.js";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { themeState } from "$lib/theme.svelte";

    let chartLabels = $state<string[]>([]);
    let featureData = $state<Record<number, number[]>>({});
    let numFeatures = $state<number>(0);

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
        let eventSource = new ReconnectingEventSource(`https://localhost:5297/data-stream`);

        eventSource.onmessage = (event: MessageEvent): void => {
            const dataArray = JSON.parse(event.data as string) as number[];

            if (numFeatures === 0 && dataArray.length > 0) {
                numFeatures = dataArray.length;
            }

            const now = new Date();
            const timeLabel =
                `${now.getHours()}` +
                `:${now.getMinutes().toString().padStart(2, "0")}` +
                `:${now.getSeconds().toString().padStart(2, "0")}`;

            chartLabels = [...chartLabels, timeLabel];

            let newFeatureData = { ...featureData };

            for (let i = 0; i < dataArray.length; i++) {
                if (!newFeatureData[i]) {
                    newFeatureData[i] = new Array(chartLabels.length - 1).fill(0);
                }
                newFeatureData[i].push(dataArray[i]);
            }

            if (chartLabels.length > 50) {
                chartLabels = chartLabels.slice(-50);
                for (let i = 0; i < dataArray.length; i++) {
                    newFeatureData[i] = newFeatureData[i].slice(-50);
                }
            }

            featureData = newFeatureData;
        };

        return () => {
            eventSource.close();
        };
    });

    function getChartData(index: number, data: number[]): ChartData<"line"> {
        const color = colors[index % colors.length];
        return {
            labels: chartLabels,
            datasets: [
                {
                    label: index === numFeatures - 1 ? "y" : `x${index + 1}`,
                    data: data,
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
    {#each Array(numFeatures) as _, i}
        <div
            class="mb-4 h-64 min-w-0 flex-1 rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900"
        >
            <Line data={getChartData(i, featureData[i] || [])} options={getChartOptions(i)} />
        </div>
    {/each}
</div>
