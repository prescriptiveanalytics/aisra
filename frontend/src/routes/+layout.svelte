<script lang="ts">
    import "./layout.css";
    import favicon from "$lib/assets/favicon.svg";
    import { themeState } from "$lib/theme.svelte";
    import { Modals } from "svelte-modals";

    const { children } = $props();

    $effect(() => {
        const stored = localStorage.getItem("theme");
        if (stored === "light" || stored === "dark") {
            themeState.preference = stored;
        } else {
            themeState.preference = "system";
        }
    });

    $effect(() => {
        const isSystemDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        themeState.current =
            themeState.preference === "system"
                ? isSystemDark
                    ? "dark"
                    : "light"
                : themeState.preference;

        if (themeState.current === "dark") {
            document.documentElement.classList.add("dark");
        } else {
            document.documentElement.classList.remove("dark");
        }
        localStorage.setItem("theme", themeState.preference);
    });

    $effect(() => {
        const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
        const listener = (e: MediaQueryListEvent): void => {
            if (themeState.preference === "system") {
                themeState.current = e.matches ? "dark" : "light";
            }
        };
        mediaQuery.addEventListener("change", listener);
        return () => mediaQuery.removeEventListener("change", listener);
    });
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>

<Modals>
    {#snippet backdrop({ close })}
        <div
            class="fixed inset-0 z-40 bg-black/50"
            onclick={() => {
                close();
            }}
            aria-hidden="true"
        ></div>
    {/snippet}
</Modals>

<div class="absolute top-4 right-4">
    <select
        bind:value={themeState.preference}
        class="rounded-md border border-gray-300 bg-gray-100 p-2 text-sm text-gray-800 hover:bg-gray-200 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-200 dark:hover:bg-gray-700"
        aria-label="Theme Preference"
    >
        <option value="system">Automatic</option>
        <option value="light">Light</option>
        <option value="dark">Dark</option>
    </select>
</div>

{@render children()}
