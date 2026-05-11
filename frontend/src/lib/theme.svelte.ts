class ThemeState {
    preference: "light" | "dark" | "system" = $state("system");
    current: "light" | "dark" = $state("light");
}

export const themeState = new ThemeState();
