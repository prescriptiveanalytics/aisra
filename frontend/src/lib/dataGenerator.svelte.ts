import { apiFetch } from "$lib/auth.svelte";
import {
    getErrorMessage,
    type DataGeneratorConfig,
    type DataGeneratorStatus,
} from "$lib/types/dataGenerator";

export const generatorState = $state<{ status: DataGeneratorStatus | null; error: string | null }>({
    status: null,
    error: null,
});

type Result<T> = { ok: true; value: T } | { ok: false; error: string };

async function request<T>(path: string, init?: RequestInit): Promise<Result<T>> {
    try {
        const res = await apiFetch(`/api/generator/${path}`, init);
        const body: unknown = await res.json().catch(() => null);

        if (!res.ok) {
            return { ok: false, error: getErrorMessage(body, `Request failed (${res.status}).`) };
        }

        return { ok: true, value: body as T };
    } catch {
        return { ok: false, error: "The backend is unreachable." };
    }
}

export async function refreshStatus(): Promise<void> {
    const result = await request<DataGeneratorStatus>("status");

    if (result.ok) {
        generatorState.status = result.value;
        generatorState.error = null;
    } else {
        generatorState.status = null;
        generatorState.error = result.error;
    }
}

/**
 * Gets the configuration stored in the generator, or the default configuration if none is stored.
 */
export function getConfig(): Promise<Result<DataGeneratorConfig>> {
    return request<DataGeneratorConfig>("config");
}

export function getDefaultConfig(): Promise<Result<DataGeneratorConfig>> {
    return request<DataGeneratorConfig>("config/default");
}

export async function saveConfig(config: DataGeneratorConfig): Promise<Result<unknown>> {
    const result = await request("config", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(config),
    });
    await refreshStatus();
    return result;
}

async function command(name: "start" | "stop" | "restart"): Promise<Result<unknown>> {
    const result = await request(name, { method: "POST" });
    await refreshStatus();
    return result;
}

/**
 * Starts the stream. If no configuration has been stored yet, the default configuration is stored first.
 */
export async function start(): Promise<Result<unknown>> {
    if (generatorState.status && !generatorState.status.hasConfiguration) {
        const config = await getConfig();
        if (!config.ok) {
            return config;
        }

        const saved = await saveConfig(config.value);
        if (!saved.ok) {
            return saved;
        }
    }

    return command("start");
}

export const stop = (): Promise<Result<unknown>> => command("stop");

export const restart = (): Promise<Result<unknown>> => command("restart");
