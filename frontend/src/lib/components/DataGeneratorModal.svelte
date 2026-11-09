<script lang="ts">
    import ConfigField from "$lib/components/ConfigField.svelte";
    import {
        generatorState,
        getConfig,
        getDefaultConfig,
        restart,
        saveConfig,
        start,
    } from "$lib/dataGenerator.svelte";
    import {
        COMMON_SERIES_FIELDS,
        GENERATOR_SECTIONS,
        SERIES_TYPE_FIELDS,
        SERIES_TYPE_LABELS,
        SERIES_TYPES,
        createSeries,
        isStreamingType,
        type DataGeneratorConfig,
        type SeriesConfig,
        type SeriesType,
    } from "$lib/types/dataGenerator";

    const { isOpen, close }: { isOpen: boolean; close: () => void } = $props();

    let config = $state<DataGeneratorConfig | null>(null);
    let mode = $state<"form" | "json">("form");
    let jsonText = $state("");
    let error = $state<string | null>(null);
    let busy = $state(false);

    // Stable keys for series, so local input state survives adding and removing series.
    const seriesKeys = new WeakMap<SeriesConfig, number>();
    let nextSeriesKey = 0;

    function seriesKey(series: SeriesConfig): number {
        let key = seriesKeys.get(series);
        if (key === undefined) {
            key = nextSeriesKey++;
            seriesKeys.set(series, key);
        }
        return key;
    }

    const running = $derived(generatorState.status?.status === "running");
    const streaming = $derived(isStreamingType(config?.generator.type ?? null));

    $effect(() => {
        void load(getConfig);
    });

    async function load(loader: typeof getConfig): Promise<void> {
        busy = true;
        const result = await loader();
        busy = false;

        if (result.ok) {
            config = result.value;
            jsonText = JSON.stringify(result.value, null, 4);
            error = null;
        } else {
            error = result.error;
        }
    }

    function parseJson(): DataGeneratorConfig | null {
        try {
            const parsed = JSON.parse(jsonText) as DataGeneratorConfig;
            if (typeof parsed !== "object" || parsed === null || !Array.isArray(parsed.series)) {
                error = "The JSON must be an object with 'generator' and 'series'.";
                return null;
            }
            error = null;
            return parsed;
        } catch (e) {
            error = `Invalid JSON: ${(e as Error).message}`;
            return null;
        }
    }

    function switchMode(next: "form" | "json"): void {
        if (next === mode) {
            return;
        }

        if (next === "json") {
            jsonText = JSON.stringify(config, null, 4);
        } else {
            const parsed = parseJson();
            if (!parsed) {
                return;
            }
            config = parsed;
        }

        mode = next;
    }

    function currentConfig(): DataGeneratorConfig | null {
        // The snapshot is typed via `unknown` because the recursive config type is too deep for `Snapshot<T>`.
        const value =
            mode === "json"
                ? parseJson()
                : ($state.snapshot(config as unknown) as DataGeneratorConfig | null);

        // The form has a single field for both, so the title always follows the ID.
        for (const series of value?.series ?? []) {
            series.title = series.id;
        }

        return value;
    }

    async function save(then?: "start" | "restart"): Promise<void> {
        const value = currentConfig();
        if (!value) {
            return;
        }

        busy = true;
        let result = await saveConfig(value);
        if (result.ok && then) {
            result = await (then === "start" ? start() : restart());
        }
        busy = false;

        if (result.ok) {
            close();
        } else {
            error = result.error;
        }
    }

    function addSeries(): void {
        if (!config) {
            return;
        }

        const series = createSeries("ME");
        series.id = `s${config.series.length + 1}`;
        series.rank = config.series.length + 1;
        config.series.push(series);
    }

    function changeSeriesType(index: number, type: SeriesType): void {
        if (config) {
            config.series[index] = createSeries(type, config.series[index]);
        }
    }

    const buttonClass =
        "cursor-pointer rounded-lg px-4 py-2 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50";
    const secondaryButtonClass = `${buttonClass} border border-gray-300 text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-200 dark:hover:bg-gray-700`;
</script>

{#if isOpen}
    <div role="dialog" class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
        <div
            class="relative z-50 flex max-h-full w-full max-w-5xl flex-col rounded-xl bg-white shadow-2xl dark:bg-gray-800"
        >
            <div
                class="flex flex-wrap items-center justify-between gap-4 border-b border-gray-200 p-6 dark:border-gray-700"
            >
                <h2 class="text-2xl font-semibold text-gray-800 dark:text-gray-100">
                    Data Stream Generation
                </h2>
                <div class="flex items-center gap-2">
                    <div
                        class="flex rounded-lg border border-gray-300 p-0.5 text-sm dark:border-gray-600"
                    >
                        {#each ["form", "json"] as const as m (m)}
                            <button
                                type="button"
                                onclick={() => switchMode(m)}
                                class="cursor-pointer rounded-md px-3 py-1 {mode === m
                                    ? 'bg-blue-600 text-white'
                                    : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700'}"
                            >
                                {m === "form" ? "Form" : "JSON"}
                            </button>
                        {/each}
                    </div>
                    <button
                        onclick={() => close()}
                        class="cursor-pointer rounded-lg p-2 text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700"
                        aria-label="Close modal"
                    >
                        <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path
                                stroke-linecap="round"
                                stroke-linejoin="round"
                                stroke-width="2"
                                d="M6 18L18 6M6 6l12 12"
                            />
                        </svg>
                    </button>
                </div>
            </div>

            <div class="flex-1 overflow-y-auto p-6">
                {#if config === null}
                    <p class="text-gray-600 dark:text-gray-400">
                        {busy ? "Loading configuration…" : "No configuration available."}
                    </p>
                {:else if mode === "json"}
                    <textarea
                        bind:value={jsonText}
                        spellcheck="false"
                        aria-label="Configuration JSON"
                        class="h-[60vh] w-full rounded-md border border-gray-300 bg-gray-50 p-3 font-mono text-sm text-gray-800 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-200"
                    ></textarea>
                {:else}
                    <div class="flex flex-col gap-6">
                        {#each GENERATOR_SECTIONS as section (section.title)}
                            {#if section.streaming === undefined || section.streaming === streaming}
                                <fieldset
                                    class="rounded-lg border border-gray-200 p-4 dark:border-gray-700"
                                >
                                    <legend
                                        class="px-1 font-semibold text-gray-700 dark:text-gray-200"
                                    >
                                        Generator – {section.title}
                                    </legend>
                                    <div
                                        class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3"
                                    >
                                        {#each section.fields as field (field.key)}
                                            <ConfigField
                                                {field}
                                                idPrefix="generator"
                                                bind:value={config.generator[field.key]}
                                            />
                                        {/each}
                                    </div>
                                </fieldset>
                            {/if}
                        {/each}

                        <div class="flex items-center justify-between">
                            <h3 class="text-lg font-semibold text-gray-700 dark:text-gray-200">
                                Series
                            </h3>
                            <button
                                type="button"
                                onclick={addSeries}
                                class="{buttonClass} bg-green-600 text-white hover:bg-green-700"
                            >
                                Add Series
                            </button>
                        </div>

                        {#if !streaming}
                            <p class="text-sm text-amber-700 dark:text-amber-400">
                                File generation types write to a file inside the generator
                                container. No data reaches AIsra.
                            </p>
                        {/if}

                        {#each config.series as series, index (seriesKey(series))}
                            {@const prefix = `series-${seriesKey(series)}`}
                            <fieldset
                                class="rounded-lg border border-gray-200 p-4 dark:border-gray-700"
                            >
                                <legend class="px-1 font-semibold text-gray-700 dark:text-gray-200">
                                    {series.id || `Series ${index + 1}`}
                                </legend>
                                <div class="mb-4 flex flex-wrap items-end justify-between gap-4">
                                    <div class="flex flex-col gap-1">
                                        <label
                                            for="{prefix}-type"
                                            class="text-sm font-medium text-gray-700 dark:text-gray-300"
                                        >
                                            Series type
                                        </label>
                                        <select
                                            id="{prefix}-type"
                                            value={series.seriesType}
                                            onchange={(e) =>
                                                changeSeriesType(
                                                    index,
                                                    e.currentTarget.value as SeriesType,
                                                )}
                                            class="rounded-md border border-gray-300 bg-white px-2 py-1 text-sm text-gray-800 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-200"
                                        >
                                            {#each SERIES_TYPES as type (type)}
                                                <option value={type}
                                                    >{SERIES_TYPE_LABELS[type]}</option
                                                >
                                            {/each}
                                        </select>
                                    </div>
                                    <button
                                        type="button"
                                        onclick={() => config?.series.splice(index, 1)}
                                        class="cursor-pointer rounded-lg bg-red-100 px-3 py-1 text-sm text-red-600 hover:bg-red-200 dark:bg-red-900/30 dark:text-red-400 dark:hover:bg-red-900/50"
                                    >
                                        Remove
                                    </button>
                                </div>
                                <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                                    {#each [...COMMON_SERIES_FIELDS, ...(SERIES_TYPE_FIELDS[series.seriesType] ?? [])] as field (field.key)}
                                        <ConfigField
                                            {field}
                                            idPrefix={prefix}
                                            bind:value={config.series[index][field.key]}
                                        />
                                    {/each}
                                </div>
                            </fieldset>
                        {/each}
                    </div>
                {/if}
            </div>

            <div
                class="flex flex-col gap-3 border-t border-gray-200 p-6 sm:flex-row sm:items-center sm:justify-between dark:border-gray-700"
            >
                <div class="min-w-0 text-sm">
                    {#if error}
                        <p class="text-red-600 dark:text-red-400">{error}</p>
                    {:else if running}
                        <p class="text-gray-600 dark:text-gray-400">
                            The stream is running. A saved configuration takes effect on restart.
                        </p>
                    {/if}
                </div>
                <div class="flex flex-wrap justify-end gap-2">
                    <button
                        type="button"
                        disabled={busy}
                        onclick={async () => {
                            mode = "form";
                            await load(getDefaultConfig);
                        }}
                        class={secondaryButtonClass}
                    >
                        Reset to Default
                    </button>
                    <button
                        type="button"
                        disabled={busy || config === null}
                        onclick={() => save()}
                        class={secondaryButtonClass}
                    >
                        Save
                    </button>
                    <button
                        type="button"
                        disabled={busy || config === null}
                        onclick={() => save(running ? "restart" : "start")}
                        class="{buttonClass} bg-blue-600 text-white hover:bg-blue-700"
                    >
                        {running ? "Save & Restart" : "Save & Start"}
                    </button>
                </div>
            </div>
        </div>
    </div>
{/if}
