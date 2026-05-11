<script lang="ts">
    import {
        type ServerEvent,
        type ServerEventType,
        serverEventTypes,
    } from "$lib/types/serverEvents";
    import { SvelteMap } from "svelte/reactivity";

    let {
        event,
    }: {
        event: ServerEvent;
    } = $props();

    const containerClasses = new SvelteMap<ServerEventType, string>([
        [
            serverEventTypes.tool,
            "bg-sky-100 border-sky-500 text-sky-700 dark:bg-sky-900/40 dark:border-sky-400 dark:text-sky-300",
        ],
        [
            serverEventTypes.qualitydrop,
            "bg-red-100 border-red-500 text-red-700 dark:bg-red-900/40 dark:border-red-400 dark:text-red-300",
        ],
    ]);

    const label = $derived(
        {
            [serverEventTypes.tool]: "Tool",
            [serverEventTypes.qualitydrop]: "Quality Drop",
        }[event.type] ?? "",
    );
</script>

<div class="flex flex-col border-l-4 px-3 py-2 {containerClasses.get(event.type)} rounded">
    <span class="text-xs font-semibold">{label}</span>
    <span class="text-black dark:text-gray-100">
        {event.message ?? ""}
    </span>
</div>
