export type ServerEventType = "tool" | "fragment" | "qualitydrop";

export type ServerEvent = {
    type: ServerEventType;
    message?: string;
};

export const serverEventTypes: Record<string, ServerEventType> = {
    tool: "tool",
    fragment: "fragment",
    qualitydrop: "qualitydrop",
} as const;
