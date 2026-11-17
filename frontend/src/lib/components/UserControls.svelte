<script lang="ts">
    import { themeState } from "$lib/theme.svelte";
    import { authState, logout } from "$lib/auth.svelte";
</script>

<div class="flex items-center gap-2">
    {#if authState.status === "authenticated" && authState.user?.authenticationEnabled}
        <span class="text-sm text-gray-600 dark:text-gray-300">
            {authState.user.email ?? authState.user.name}
        </span>
        <button
            type="button"
            onclick={logout}
            class="rounded-md border border-gray-300 bg-gray-100 p-2 text-sm text-gray-800 hover:bg-gray-200 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-200 dark:hover:bg-gray-700"
        >
            Sign out
        </button>
    {/if}
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
