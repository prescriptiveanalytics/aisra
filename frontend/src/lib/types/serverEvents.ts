export type ServerEventType = "tool" | "fragment" | "qualitydrop" | "done" | "user_message";

export type ServerEvent = {
    type: ServerEventType;
    message?: string;
};
