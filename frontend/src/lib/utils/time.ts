export function formatTimeLabel(): string {
    const now = new Date();
    return (
        `${now.getHours()}` +
        `:${now.getMinutes().toString().padStart(2, "0")}` +
        `:${now.getSeconds().toString().padStart(2, "0")}`
    );
}
