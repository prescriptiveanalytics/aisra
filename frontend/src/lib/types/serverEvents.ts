export type ServerEventType =
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
