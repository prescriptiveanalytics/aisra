export type ServerEventType = "tool" | "fragment" | "qualitydrop" | "done" | "user_message";

export type ServerEvent = {
    type: ServerEventType;
    message?: string;
};

export const serverEventTypes: Record<string, ServerEventType> = {
    tool: "tool",
    fragment: "fragment",
    qualitydrop: "qualitydrop",
    done: "done",
    user_message: "user_message",
} as const;
