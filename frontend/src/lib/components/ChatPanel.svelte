<script lang="ts">
    import SvelteMarkdown from "@humanspeak/svelte-markdown";
    import ServerEventNotification from "$lib/components/ServerEventNotification.svelte";
    import type { ServerEvent } from "$lib/types/serverEvents";

    const {
        serverEvents,
        isDone,
        onSendMessage,
    }: {
        serverEvents: ServerEvent[];
        isDone: boolean;
        onSendMessage: (message: string) => void;
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
    <h2 class="mb-4 text-xl font-semibold text-gray-700 dark:text-gray-200">Agent</h2>

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
