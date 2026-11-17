<script lang="ts">
    import "./layout.css";
    import favicon from "$lib/assets/favicon.svg";
    import { themeState } from "$lib/theme.svelte";
    import { Modals } from "svelte-modals";
    import { authState, checkAuth } from "$lib/auth.svelte";
    import LoginScreen from "$lib/components/LoginScreen.svelte";
    import UserControls from "$lib/components/UserControls.svelte";

    const { children } = $props();

    $effect(() => {
        void checkAuth();
    });

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

{#if authState.status === "authenticated"}
    <!-- The dashboard shows the user controls in its own header. -->
    {@render children()}
{:else}
    <div class="absolute top-4 right-4">
        <UserControls />
    </div>

    {#if authState.status !== "loading"}
        <LoginScreen />
    {/if}
{/if}
