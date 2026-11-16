export type ServerEventType =
    | "tool"
    | "fragment"
    | "qualitydrop"
    | "done"
    | "interrupted"
    | "user_message";

export type ServerEvent = {
    type: ServerEventType;
    message?: string;
};
