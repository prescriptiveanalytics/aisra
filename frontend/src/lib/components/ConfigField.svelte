<script lang="ts">
    import type { ConfigValue, Driver, FieldDescriptor } from "$lib/types/dataGenerator";

    // `value` is reassigned as a bindable prop, so the props can't be destructured with `const`.
    /* eslint-disable prefer-const */
    let {
        field,
        value = $bindable(),
        idPrefix,
    }: {
        field: FieldDescriptor;
        value: ConfigValue | undefined;
        idPrefix: string;
    } = $props();
    /* eslint-enable prefer-const */

    const inputId = $derived(`${idPrefix}-${field.key}`);

    const inputClass =
        "w-full rounded-md border border-gray-300 bg-white px-2 py-1 text-sm text-gray-800 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-200";

    function formatList(list: ConfigValue | undefined, separator: string): string {
        return Array.isArray(list) ? list.join(separator) : "";
    }

    function parseList(text: string, separator: RegExp, numeric: boolean): ConfigValue {
        const parts = text
            .split(separator)
            .map((part) => part.trim())
            .filter((part) => part.length > 0);

        if (parts.length === 0) {
            // Lists with a list as default value are required by the generator (e.g. AR coefficients).
            return Array.isArray(field.defaultValue) ? [] : null;
        }

        return numeric ? parts.map(Number) : parts;
    }

    function setNumber(text: string): void {
        if (text.trim() === "") {
            value = null;
            return;
        }

        const number = Number(text);
        value = field.kind === "integer" ? Math.trunc(number) : number;
    }

    function setText(text: string): void {
        // Optional text fields are omitted rather than sent as empty strings.
        value = text === "" && field.defaultValue === null ? null : text;
    }

    const drivers = $derived(Array.isArray(value) ? (value as unknown as Driver[]) : []);

    function setDrivers(next: Driver[]): void {
        value = next.length > 0 ? (next as unknown as ConfigValue) : null;
    }

    function updateDriver(index: number, patch: Partial<Driver>): void {
        setDrivers(drivers.map((driver, i) => (i === index ? { ...driver, ...patch } : driver)));
    }

    function parseCoefficients(text: string): number[] | null {
        return parseList(text, /,/, true) as number[] | null;
    }
</script>

{#if field.kind === "boolean"}
    <label class="flex items-start gap-2 text-sm text-gray-700 dark:text-gray-300">
        <input
            id={inputId}
            type="checkbox"
            class="mt-0.5"
            checked={value === true}
            onchange={(e) => (value = e.currentTarget.checked)}
        />
        <span>
            {field.label}
            {#if field.hint}
                <span class="block text-xs text-gray-500 dark:text-gray-400">{field.hint}</span>
            {/if}
        </span>
    </label>
{:else}
    <div
        class="flex flex-col gap-1 {field.kind === 'drivers' || field.kind === 'lines'
            ? 'sm:col-span-2'
            : ''}"
    >
        <label for={inputId} class="text-sm font-medium text-gray-700 dark:text-gray-300">
            {field.label}
        </label>

        {#if field.kind === "select"}
            <select
                id={inputId}
                class={inputClass}
                value={value ?? ""}
                onchange={(e) => (value = e.currentTarget.value)}
            >
                {#each field.options ?? [] as option (option)}
                    <option value={option}>{option}</option>
                {/each}
            </select>
        {:else if field.kind === "number" || field.kind === "integer"}
            <input
                id={inputId}
                type="number"
                step={field.kind === "integer" ? 1 : "any"}
                class={inputClass}
                value={typeof value === "number" ? value : ""}
                onchange={(e) => setNumber(e.currentTarget.value)}
            />
        {:else if field.kind === "datetime"}
            <input
                id={inputId}
                type="datetime-local"
                step="0.001"
                class={inputClass}
                value={typeof value === "string" ? value : ""}
                onchange={(e) => (value = e.currentTarget.value)}
            />
        {:else if field.kind === "numberList" || field.kind === "integerList" || field.kind === "stringList"}
            <input
                id={inputId}
                type="text"
                class={inputClass}
                value={formatList(value, ", ")}
                onchange={(e) =>
                    (value = parseList(e.currentTarget.value, /,/, field.kind !== "stringList"))}
            />
        {:else if field.kind === "lines"}
            <textarea
                id={inputId}
                rows="3"
                class="{inputClass} font-mono"
                value={formatList(value, "\n")}
                onchange={(e) => (value = parseList(e.currentTarget.value, /\r?\n/, false))}
            ></textarea>
        {:else if field.kind === "drivers"}
            <div class="flex flex-col gap-2">
                {#each drivers as driver, index (index)}
                    <div class="flex flex-wrap items-end gap-2">
                        <label class="flex flex-col text-xs text-gray-600 dark:text-gray-400">
                            Series ID
                            <input
                                type="text"
                                class="{inputClass} w-28"
                                value={driver.id}
                                onchange={(e) => updateDriver(index, { id: e.currentTarget.value })}
                            />
                        </label>
                        <label
                            class="flex flex-1 flex-col text-xs text-gray-600 dark:text-gray-400"
                        >
                            p
                            <input
                                type="text"
                                class={inputClass}
                                value={driver.p?.join(", ") ?? ""}
                                onchange={(e) =>
                                    updateDriver(index, {
                                        p: parseCoefficients(e.currentTarget.value),
                                    })}
                            />
                        </label>
                        <label
                            class="flex flex-1 flex-col text-xs text-gray-600 dark:text-gray-400"
                        >
                            q
                            <input
                                type="text"
                                class={inputClass}
                                value={driver.q?.join(", ") ?? ""}
                                onchange={(e) =>
                                    updateDriver(index, {
                                        q: parseCoefficients(e.currentTarget.value),
                                    })}
                            />
                        </label>
                        <button
                            type="button"
                            class="cursor-pointer rounded-md px-2 py-1 text-sm text-red-600 hover:bg-red-100 dark:text-red-400 dark:hover:bg-red-900/30"
                            onclick={() => setDrivers(drivers.filter((_, i) => i !== index))}
                        >
                            Remove
                        </button>
                    </div>
                {/each}
                <button
                    type="button"
                    class="cursor-pointer self-start rounded-md border border-gray-300 px-2 py-1 text-sm text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
                    onclick={() => setDrivers([...drivers, { id: "", p: [0.5], q: null }])}
                >
                    Add driver
                </button>
            </div>
        {:else}
            <input
                id={inputId}
                type="text"
                class={inputClass}
                value={typeof value === "string" ? value : ""}
                onchange={(e) => setText(e.currentTarget.value)}
            />
        {/if}

        {#if field.hint}
            <span class="text-xs text-gray-500 dark:text-gray-400">{field.hint}</span>
        {/if}
    </div>
{/if}
