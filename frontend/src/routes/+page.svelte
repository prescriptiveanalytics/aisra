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
    import ChatPanel from "$lib/components/ChatPanel.svelte";
    import DataStreamCharts from "$lib/components/DataStreamCharts.svelte";
    import ModelSection from "../lib/components/ModelSection.svelte";
    import type { ServerEvent } from "$lib/types/serverEvents";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { type ModalComponent, type ModalProps, modals } from "svelte-modals";
    import AddChartModal from "$lib/components/AddChartModal.svelte";
    import { apiBase } from "$lib/config";

    ChartJS.register(Title, Tooltip, Legend, LineElement, LinearScale, PointElement, CategoryScale);

    type ServerEventData = { message: string };

    let serverEvents = $state<ServerEvent[]>([]);
    let isDone = $state(true);

    let selectedCharts = $state<(number | null)[]>([null]);

    async function handleAddChart(): Promise<void> {
        const res = await fetch(`${apiBase}/api/models`);
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
        const eventSource = new ReconnectingEventSource(`${apiBase}/api/token-stream`);

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

    function handleSendMessage(message: string): void {
        isDone = false;
        serverEvents = [{ type: "user_message", message }, ...serverEvents];

        fetch(`${apiBase}/api/chat`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ message }),
        }).catch(() => {
            isDone = true;
        });
    }
</script>

<div class="mx-auto max-w-500 p-8 font-sans">
    <h1 class="mb-6 text-3xl font-bold text-gray-800 dark:text-gray-100">Dashboard</h1>

    <div class="flex flex-col gap-8 lg:flex-row">
        <ChatPanel {serverEvents} {isDone} onSendMessage={handleSendMessage} />

        <div class="flex-1">
            <div class="mb-4 flex items-center justify-between">
                <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">Data Stream</h2>
            </div>

            <DataStreamCharts />

            <ModelSection
                {selectedCharts}
                onAddChart={handleAddChart}
                onRemoveChart={removeChart}
            />
        </div>
    </div>
</div>
