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
    import DataGeneratorControls from "$lib/components/DataGeneratorControls.svelte";
    import ModelSection from "../lib/components/ModelSection.svelte";
    import MonitorModelQualityToggle from "$lib/components/MonitorModelQualityToggle.svelte";
    import UserControls from "$lib/components/UserControls.svelte";
    import type { ServerEvent } from "$lib/types/serverEvents";
    import ReconnectingEventSource from "reconnecting-eventsource";
    import { type ModalComponent, type ModalProps, modals } from "svelte-modals";
    import AddChartModal from "$lib/components/AddChartModal.svelte";
    import ResetModal from "$lib/components/ResetModal.svelte";
    import { apiBase } from "$lib/config";
    import { apiFetch } from "$lib/auth.svelte";
    import { reloadPage } from "$lib/reload";

    ChartJS.register(Title, Tooltip, Legend, LineElement, LinearScale, PointElement, CategoryScale);

    type ServerEventData = { message: string };

    let serverEvents = $state<ServerEvent[]>([]);
    let isDone = $state(true);
    let isStopping = $state(false);
    let monitorModelQuality = $state<boolean | null>(null);

    type AgentSettings = { monitorModelQuality: boolean };

    async function loadAgentSettings(): Promise<void> {
        const res = await apiFetch("/api/chat/settings");
        if (res.ok) {
            monitorModelQuality = ((await res.json()) as AgentSettings).monitorModelQuality;
        }
    }

    async function handleMonitorModelQualityChange(enabled: boolean): Promise<void> {
        const previous = monitorModelQuality;
        monitorModelQuality = enabled;

        try {
            const res = await apiFetch("/api/chat/settings", {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ monitorModelQuality: enabled } satisfies AgentSettings),
            });
            if (!res.ok) {
                throw new Error(`Failed to update the agent settings: ${res.status}`);
            }
            monitorModelQuality = ((await res.json()) as AgentSettings).monitorModelQuality;
        } catch {
            monitorModelQuality = previous;
        }
    }

    $effect(() => {
        void loadAgentSettings();
    });

    let selectedCharts = $state<(number | null)[]>([null]);

    async function handleAddChart(): Promise<void> {
        const res = await apiFetch("/api/models");
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

    async function handleReset(): Promise<void> {
        await modals.open(ResetModal as unknown as ModalComponent<ModalProps<unknown>, object, "">);
    }

    function removeChart(modelIdToRemove: number | null): void {
        selectedCharts = selectedCharts.filter((id) => id !== modelIdToRemove);
    }

    function getMessage(data: string): string {
        return (JSON.parse(data) as ServerEventData).message as string;
    }

    $effect(() => {
        const eventSource = new ReconnectingEventSource(`${apiBase}/api/token-stream`, {
            withCredentials: true,
        });

        // Sent when the agent starts responding, and on (re)connecting while it is busy.
        eventSource.addEventListener("started", () => {
            isDone = false;
        });

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
            isStopping = false;
        });

        eventSource.addEventListener("interrupted", () => {
            isDone = true;
            isStopping = false;
            serverEvents = [
                { type: "interrupted", message: "The agent was stopped." },
                ...serverEvents,
            ];
        });

        // The application was reset (possibly from another client), so all state shown here is outdated.
        eventSource.addEventListener("reset", reloadPage);

        return () => {
            eventSource.close();
        };
    });

    function handleSendMessage(message: string): void {
        isDone = false;
        serverEvents = [{ type: "user_message", message }, ...serverEvents];

        apiFetch("/api/chat", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ message }),
        })
            .then((res) => {
                // On 409, the agent is already busy, so it can still be stopped.
                if (!res.ok && res.status !== 409) {
                    isDone = true;
                }
            })
            .catch(() => {
                isDone = true;
            });
    }

    async function handleStop(): Promise<void> {
        isStopping = true;

        try {
            const res = await apiFetch("/api/chat/stop", { method: "POST" });
            if (!res.ok) {
                isStopping = false;
            } else if (!((await res.json()) as boolean)) {
                // Nothing was running, so no "interrupted" event will follow.
                isDone = true;
                isStopping = false;
            }
        } catch {
            isStopping = false;
        }
    }
</script>

<!--
    From 2xl up, the page fills the viewport: the header rows and the chat stay in place and only the
    chart columns scroll. Below that, the sections are placed into one or two columns and the page
    scrolls as a whole.
-->
<div
    class="mx-auto grid max-w-640 grid-cols-1 gap-x-8 p-8 font-sans lg:grid-cols-[28rem_minmax(0,1fr)] 2xl:h-dvh 2xl:grid-cols-[28rem_minmax(0,1fr)_minmax(0,1fr)] 2xl:grid-rows-[auto_auto_minmax(0,1fr)]"
>
    <div class="col-span-full mb-6 flex items-center justify-between gap-4">
        <div class="flex items-center gap-4">
            <h1 class="text-3xl font-bold text-gray-800 dark:text-gray-100">Dashboard</h1>
            <button
                type="button"
                onclick={handleReset}
                class="cursor-pointer rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-50 dark:border-red-800 dark:text-red-300 dark:hover:bg-red-900/40"
                title="Reset the entire application"
            >
                Reset
            </button>
        </div>

        <UserControls />
    </div>

    <div class="row-start-2 mb-4 flex max-w-lg items-center justify-between gap-4 lg:col-start-1">
        <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">Agent</h2>
        <MonitorModelQualityToggle
            enabled={monitorModelQuality}
            onChange={handleMonitorModelQualityChange}
        />
    </div>

    <div
        class="row-start-4 mb-4 flex items-center justify-between gap-4 lg:col-start-2 lg:row-start-2"
    >
        <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">Data Stream</h2>
        <DataGeneratorControls />
    </div>

    <div
        class="row-start-6 mb-4 flex items-center justify-between gap-4 lg:col-start-2 lg:row-start-4 2xl:col-start-3 2xl:row-start-2"
    >
        <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">Models</h2>
        <button
            onclick={handleAddChart}
            class="cursor-pointer rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
        >
            Add Chart
        </button>
    </div>

    <div
        class="row-start-3 mb-8 flex min-h-0 flex-col lg:col-start-1 lg:row-span-3 lg:mb-0 2xl:row-span-1"
    >
        <ChatPanel
            {serverEvents}
            {isDone}
            {isStopping}
            onSendMessage={handleSendMessage}
            onStop={handleStop}
        />
    </div>

    <div
        class="row-start-5 mb-8 lg:col-start-2 lg:row-start-3 2xl:mb-0 2xl:min-h-0 2xl:overflow-y-auto"
    >
        <DataStreamCharts />
    </div>

    <div
        class="row-start-7 lg:col-start-2 lg:row-start-5 2xl:col-start-3 2xl:row-start-3 2xl:min-h-0 2xl:overflow-y-auto"
    >
        <ModelSection {selectedCharts} onRemoveChart={removeChart} />
    </div>
</div>
