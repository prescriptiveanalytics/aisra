export type ServerEventType =
    | "started"
    | "tool"
    | "fragment"
    | "qualitydrop"
    | "done"
    | "interrupted"
    | "reset"
    | "user_message";

export type ServerEvent = {
    type: ServerEventType;
    message?: string;
};
