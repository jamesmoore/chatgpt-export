import { useEffect } from "react"
import { useTheme } from "../components/theme-provider"

export function useSystemThemeListener() {
    const { theme } = useTheme();

    useEffect(() => {
        if (theme !== "system") return;

        const media = window.matchMedia("(prefers-color-scheme: dark)");

        const handler = (e: MediaQueryListEvent) => {
            const root = window.document.documentElement
            const newTheme = e.matches ? "dark" : "light";
            if (!root.classList.contains(newTheme)) {
                root.classList.remove("light", "dark");
                root.classList.add(newTheme);
            }
        }

        media.addEventListener("change", handler);
        return () => media.removeEventListener("change", handler);
    }, [theme]);
}