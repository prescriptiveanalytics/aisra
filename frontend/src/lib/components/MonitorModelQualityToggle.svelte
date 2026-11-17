<script lang="ts">
    const {
        enabled,
        onChange,
    }: {
        enabled: boolean | null;
        onChange: (enabled: boolean) => void;
    } = $props();
</script>

<label
    class="flex cursor-pointer items-center gap-2 text-sm text-gray-700 select-none dark:text-gray-200"
    title="Prompt the agent when the model quality falls below the threshold."
>
    <input
        type="checkbox"
        role="switch"
        class="peer sr-only"
        checked={enabled ?? false}
        disabled={enabled === null}
        onchange={(e) => onChange(e.currentTarget.checked)}
    />
    <span
        class="relative h-5 w-9 rounded-full bg-gray-300 transition-colors peer-checked:bg-blue-600 peer-focus-visible:ring-2 peer-focus-visible:ring-blue-400 peer-disabled:opacity-50 after:absolute after:top-0.5 after:left-0.5 after:h-4 after:w-4 after:rounded-full after:bg-white after:transition-transform peer-checked:after:translate-x-4 dark:bg-gray-600"
    ></span>
    Monitor model quality
</label>
