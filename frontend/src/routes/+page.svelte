<script lang="ts">
    import { Line } from "svelte-chartjs";
    import {
        Chart as ChartJS,
        Title,
        Tooltip,
        Legend,
        LineElement,
        LinearScale,
        PointElement,
        CategoryScale,
        type ChartData,
        type ChartOptions,
    } from "chart.js";
    import SvelteMarkdown from "@humanspeak/svelte-markdown";
    import ServerEventNotification from "$lib/components/ServerEventNotification.svelte";
    import type { ServerEvent } from "$lib/types/serverEvents";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { themeState } from "$lib/theme.svelte";

    ChartJS.register(Title, Tooltip, Legend, LineElement, LinearScale, PointElement, CategoryScale);

    type ServerEventData = { message: string };

    let serverEvents = $state([] as ServerEvent[]);

    let chartLabels = $state<string[]>([]);
    let chartValues = $state<number[]>([]);

    let chartData = $derived<ChartData<"line">>({
        labels: chartLabels,
        datasets: [
            {
                label: "Quality (%)",
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

    function getMessage(data: string): string {
        return (JSON.parse(data) as ServerEventData).message as string;
    }

    $effect(() => {
        let eventSource = new ReconnectingEventSource("https://localhost:5297/ai-stream");

        eventSource.addEventListener("tool", (event) => {
            serverEvents = [
                { type: "tool", message: getMessage(event.data as string) },
                ...serverEvents,
            ];
        });

        eventSource.addEventListener("fragment", (event: MessageEvent) => {
            const fragment = getMessage(event.data as string);

            if (serverEvents.length > 0 && serverEvents[0].type === "fragment") {
                serverEvents[0].message += fragment;
            } else {
                serverEvents = [{ type: "fragment", message: fragment }, ...serverEvents];
            }
        });

        eventSource.addEventListener("qualitydrop", () => {
            serverEvents = [{ type: "qualitydrop" }, ...serverEvents];
        });

        return () => {
            eventSource.close();
        };
    });

    $effect(() => {
        let eventSource = new ReconnectingEventSource("https://localhost:5297/quality-stream");

        eventSource.onmessage = (event) => {
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

<div class="mx-auto max-w-6xl p-8 font-sans">
    <h1 class="mb-6 text-3xl font-bold text-gray-800 dark:text-gray-100">
        HeuristicAgent Dashboard
    </h1>

    <div class="grid grid-cols-1 gap-8 lg:grid-cols-2">
        <div class="flex flex-col">
            <h2 class="mb-4 text-xl font-semibold text-gray-700 dark:text-gray-200">Agent</h2>

            <div
                class={"flex max-h-100 flex-1 flex-col-reverse gap-4 overflow-y-scroll rounded-xl" +
                    " border-gray-200 bg-gray-50 p-6 text-sm leading-relaxed text-gray-800 shadow-sm dark:border-gray-700 dark:bg-gray-900 dark:text-gray-300"}
            >
                {#each serverEvents as event, idx (idx)}
                    {#if event.type === "fragment"}
                        <div class="prose **:whitespace-normal dark:prose-invert">
                            <SvelteMarkdown source={event.message ?? ""} />
                        </div>
                    {:else}
                        <ServerEventNotification {event} />
                    {/if}
                {/each}
            </div>
        </div>

        <div>
            <h2 class="mb-4 text-xl font-semibold text-gray-700 dark:text-gray-200">
                Model Quality
            </h2>

            <div
                class="h-100 flex-1 rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900"
            >
                <Line data={chartData} options={chartOptions} />
            </div>
        </div>
    </div>
</div>
