import { env } from "$env/dynamic/public";

export const apiBase = env.PUBLIC_API_URL ?? "";
