<script lang="ts">
    import {
        Chart as ChartJS,
        Title,
        Tooltip,
        Legend,
        LineElement,
        LinearScale,
        PointElement,
        CategoryScale,
    } from "chart.js";
    import SvelteMarkdown from "@humanspeak/svelte-markdown";
    import ServerEventNotification from "$lib/components/ServerEventNotification.svelte";
    import QualityChart from "$lib/components/QualityChart.svelte";
    import DataStreamCharts from "$lib/components/DataStreamCharts.svelte";
    import type { ServerEvent } from "$lib/types/serverEvents";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { type ModalComponent, type ModalProps, modals } from "svelte-modals";
    import AddChartModal from "$lib/components/AddChartModal.svelte";

    ChartJS.register(Title, Tooltip, Legend, LineElement, LinearScale, PointElement, CategoryScale);

    type ServerEventData = { message: string };

    let serverEvents = $state([] as ServerEvent[]);
    let chatInput = $state("");
    let isDone = $state(true);

    let selectedCharts = $state<(number | null)[]>([null]);

    async function handleAddChart(): Promise<void> {
        const res = await fetch("https://localhost:5297/models");
        if (res.ok) {
            const fetchedModels = (await res.json()) as { id: number; model: string }[];
            await modals.open(
                AddChartModal as unknown as ModalComponent<ModalProps<unknown>, object, "">,
                {
                    models: fetchedModels,
                    selectedCharts,
                    onConfirm: (modelId: number) => {
                        if (!selectedCharts.includes(modelId)) {
                            selectedCharts = [...selectedCharts, modelId];
                        }
                    },
                },
            );
        }
    }

    function removeChart(modelIdToRemove: number | null): void {
        selectedCharts = selectedCharts.filter((id) => id !== modelIdToRemove);
    }

    function getMessage(data: string): string {
        return (JSON.parse(data) as ServerEventData).message as string;
    }

    $effect(() => {
        let eventSource = new ReconnectingEventSource("https://localhost:5297/ai-stream");

        eventSource.addEventListener("tool", (event) => {
            isDone = false;
            serverEvents = [
                { type: "tool", message: getMessage(event.data as string) },
                ...serverEvents,
            ];
        });

        eventSource.addEventListener("fragment", (event: MessageEvent) => {
            isDone = false;
            const fragment = getMessage(event.data as string);

            if (serverEvents.length > 0 && serverEvents[0].type === "fragment") {
                serverEvents[0].message += fragment;
            } else {
                serverEvents = [{ type: "fragment", message: fragment }, ...serverEvents];
            }
        });

        eventSource.addEventListener("qualitydrop", () => {
            isDone = false;
            serverEvents = [{ type: "qualitydrop" }, ...serverEvents];
        });

        eventSource.addEventListener("done", () => {
            isDone = true;
        });

        return () => {
            eventSource.close();
        };
    });

    async function handleChatSubmit(): Promise<void> {
        if (!chatInput.trim() || !isDone) {
            return;
        }

        const message = chatInput.trim();
        chatInput = "";
        isDone = false;

        serverEvents = [{ type: "user_message", message }, ...serverEvents];

        try {
            await fetch("https://localhost:5297/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ message }),
            });
        } catch {
            isDone = true;
        }
    }
</script>

<div class="mx-auto max-w-500 p-8 font-sans">
    <h1 class="mb-6 text-3xl font-bold text-gray-800 dark:text-gray-100">Dashboard</h1>

    <div class="flex flex-col gap-8 lg:flex-row">
        <div class="flex w-full max-w-lg flex-col lg:max-w-md">
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
                    {:else if event.type === "user_message"}
                        <div
                            class="max-w-[80%] self-end rounded-lg bg-blue-100 p-3 text-blue-900 dark:bg-blue-900 dark:text-blue-100"
                        >
                            {event.message}
                        </div>
                    {:else}
                        <ServerEventNotification {event} />
                    {/if}
                {/each}
            </div>

            <form
                onsubmit={(e) => {
                    e.preventDefault();
                    handleChatSubmit();
                }}
                class="mt-4 flex gap-2"
            >
                <input
                    type="text"
                    bind:value={chatInput}
                    disabled={!isDone}
                    placeholder={isDone ? "Type your message..." : "Waiting for agent..."}
                    class="flex-1 rounded-lg border border-gray-300 p-2 text-sm disabled:opacity-50 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                />
                <button
                    type="submit"
                    disabled={!isDone || !chatInput.trim()}
                    class="cursor-pointer rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                >
                    Send
                </button>
            </form>
        </div>

        <div class="flex-1">
            <div class="mb-4 flex items-center justify-between">
                <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">Data Stream</h2>
            </div>

            <DataStreamCharts />

            <div class="mt-8 mb-4 flex items-center justify-between">
                <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">
                    Model Quality
                </h2>

                <div class="flex gap-2">
                    <button
                        onclick={handleAddChart}
                        class="cursor-pointer rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
                    >
                        Add Chart
                    </button>
                </div>
            </div>

            <div class="grid grid-cols-1 gap-4 2xl:grid-cols-2">
                {#each selectedCharts as chartModelId, index (chartModelId)}
                    <div class="mb-4 h-full min-w-0 flex-1 content-end">
                        {#if index > 0}
                            <div class="flex justify-end">
                                <button
                                    onclick={() => removeChart(chartModelId)}
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
        </div>
    </div>
</div>
