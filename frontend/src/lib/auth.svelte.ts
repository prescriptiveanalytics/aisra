import { apiBase } from "$lib/config";

export type User = {
    authenticationEnabled: boolean;
    name: string | null;
    email: string | null;
};

type AuthStatus = "loading" | "authenticated" | "unauthenticated" | "forbidden" | "error";

export const authState = $state<{ status: AuthStatus; user: User | null }>({
    status: "loading",
    user: null,
});

export async function checkAuth(): Promise<void> {
    try {
        const res = await fetch(`${apiBase}/api/auth/me`, { credentials: "include" });

        if (res.ok) {
            authState.user = (await res.json()) as User;
            authState.status = "authenticated";
        } else if (res.status === 401) {
            authState.user = null;
            authState.status = "unauthenticated";
        } else if (res.status === 403) {
            authState.user = null;
            authState.status = "forbidden";
        } else {
            authState.status = "error";
        }
    } catch {
        authState.status = "error";
    }
}

export function login(): void {
    // The query is dropped so that a previous "authError" is not shown again after sign-in.
    const returnUrl = `${window.location.origin}${window.location.pathname}`;
    window.location.href = `${apiBase}/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;
}

export function logout(): void {
    // A form submission is used so the browser follows the backend's redirect back to the frontend.
    const form = document.createElement("form");
    form.method = "POST";
    form.action = `${apiBase}/api/auth/logout`;
    document.body.appendChild(form);
    form.submit();
}

/**
 * Wrapper around fetch that sends the auth cookie and re-checks the session on 401/403.
 */
export async function apiFetch(path: string, init?: RequestInit): Promise<Response> {
    const res = await fetch(`${apiBase}${path}`, { ...init, credentials: "include" });

    if (res.status === 401 || res.status === 403) {
        await checkAuth();
    }

    return res;
}
