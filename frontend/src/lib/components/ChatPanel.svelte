<script lang="ts">
    import SvelteMarkdown from "@humanspeak/svelte-markdown";
    import ServerEventNotification from "$lib/components/ServerEventNotification.svelte";
    import type { ServerEvent } from "$lib/types/serverEvents";

    const {
        serverEvents,
        isDone,
        isStopping,
        monitorModelQuality,
        onSendMessage,
        onStop,
        onMonitorModelQualityChange,
    }: {
        serverEvents: ServerEvent[];
        isDone: boolean;
        isStopping: boolean;
        monitorModelQuality: boolean | null;
        onSendMessage: (message: string) => void;
        onStop: () => void;
        onMonitorModelQualityChange: (enabled: boolean) => void;
    } = $props();

    let chatInput = $state("");

    function handleSubmit(e: Event): void {
        e.preventDefault();
        if (!chatInput.trim() || !isDone) {
            return;
        }
        onSendMessage(chatInput.trim());
        chatInput = "";
    }
</script>

<div class="flex w-full max-w-lg flex-col lg:max-w-md">
    <div class="mb-4 flex items-center justify-between gap-4">
        <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200">Agent</h2>

        <label
            class="flex cursor-pointer items-center gap-2 text-sm text-gray-700 select-none dark:text-gray-200"
            title="Prompt the agent when the model quality falls below the threshold."
        >
            <input
                type="checkbox"
                role="switch"
                class="peer sr-only"
                checked={monitorModelQuality ?? false}
                disabled={monitorModelQuality === null}
                onchange={(e) => onMonitorModelQualityChange(e.currentTarget.checked)}
            />
            <span
                class="relative h-5 w-9 rounded-full bg-gray-300 transition-colors peer-checked:bg-blue-600 peer-focus-visible:ring-2 peer-focus-visible:ring-blue-400 peer-disabled:opacity-50 after:absolute after:top-0.5 after:left-0.5 after:h-4 after:w-4 after:rounded-full after:bg-white after:transition-transform peer-checked:after:translate-x-4 dark:bg-gray-600"
            ></span>
            Monitor model quality
        </label>
    </div>

    <div
        class="flex max-h-100 flex-1 flex-col-reverse gap-4 overflow-y-scroll rounded-xl border-gray-200 bg-gray-50 p-6 text-sm leading-relaxed text-gray-800 shadow-sm dark:border-gray-700 dark:bg-gray-900 dark:text-gray-300"
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

    <form onsubmit={handleSubmit} class="mt-4 flex gap-2">
        <input
            type="text"
            bind:value={chatInput}
            placeholder="Type your message..."
            class="flex-1 rounded-lg border border-gray-300 p-2 text-sm dark:border-gray-600 dark:bg-gray-800 dark:text-white"
        />
        {#if isDone}
            <button
                type="submit"
                disabled={!chatInput.trim()}
                class="cursor-pointer rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
                Send
            </button>
        {:else}
            <button
                type="button"
                disabled={isStopping}
                onclick={onStop}
                class="cursor-pointer rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
            >
                Stop
            </button>
        {/if}
    </form>
</div>
