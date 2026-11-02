<script lang="ts">
    import { authState, login } from "$lib/auth.svelte";

    const authError = new URLSearchParams(window.location.search).get("authError");

    const errorMessage = $derived(
        authState.status === "forbidden" || authError === "forbidden"
            ? "Your account is not authorized to use this application."
            : authError === "failed"
              ? "Sign-in failed. Please try again."
              : authState.status === "error"
                ? "The server could not be reached."
                : null,
    );
</script>

<div class="flex min-h-screen items-center justify-center p-8 font-sans">
    <div
        class="w-full max-w-sm rounded-lg border border-gray-200 bg-white p-8 text-center shadow-sm dark:border-gray-700 dark:bg-gray-800"
    >
        <h1 class="mb-2 text-2xl font-bold text-gray-800 dark:text-gray-100">AIsra</h1>
        <p class="mb-6 text-sm text-gray-600 dark:text-gray-300">
            Please sign in with your Google account to continue.
        </p>

        {#if errorMessage}
            <p
                class="mb-6 rounded-md bg-red-50 p-3 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-300"
            >
                {errorMessage}
            </p>
        {/if}

        <button
            type="button"
            onclick={login}
            class="w-full rounded-md bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700"
        >
            Sign in with Google
        </button>
    </div>
</div>
