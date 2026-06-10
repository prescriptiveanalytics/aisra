import prettier from "eslint-config-prettier";
import path from "node:path";
import { includeIgnoreFile } from "@eslint/compat";
import js from "@eslint/js";
import svelte from "eslint-plugin-svelte";
import { defineConfig } from "eslint/config";
import globals from "globals";
import ts from "typescript-eslint";
import svelteConfig from "./svelte.config.js";

const gitignorePath = path.resolve(import.meta.dirname, ".gitignore");

const typeCheckedFiles = [
    "src/**/*.ts",
    "src/**/*.svelte",
    "src/**/*.svelte.ts",
    "src/**/*.svelte.js",
];

export default defineConfig(
    includeIgnoreFile(gitignorePath),
    js.configs.recommended,
    ...ts.configs.recommended.map((config) => ({
        ...config,
        files: typeCheckedFiles,
    })),
    svelte.configs.recommended,
    prettier,
    svelte.configs.prettier,
    {
        languageOptions: { globals: { ...globals.browser, ...globals.node } },
        rules: {
            "no-undef": "off",
        },
    },
    {
        ignores: ["node_modules/", "build/", ".svelte-kit/", "dist/"],
    },
    {
        files: typeCheckedFiles,
        languageOptions: {
            parserOptions: {
                projectService: true,
                extraFileExtensions: [".svelte"],
                parser: ts.parser,
                svelteConfig,
            },
        },
    },
    {
        files: typeCheckedFiles,
        rules: {
            "@typescript-eslint/explicit-function-return-type": [
                "error",
                {
                    allowExpressions: true,
                },
            ],
            "@typescript-eslint/naming-convention": [
                "error",
                {
                    selector: "default",
                    format: ["camelCase"],
                    leadingUnderscore: "forbid",
                    trailingUnderscore: "forbid",
                },
                {
                    selector: "typeLike",
                    format: ["PascalCase"],
                    leadingUnderscore: "forbid",
                    trailingUnderscore: "forbid",
                },
                {
                    selector: "enumMember",
                    format: ["UPPER_CASE"],
                    leadingUnderscore: "forbid",
                    trailingUnderscore: "forbid",
                },
                {
                    selector: "classProperty",
                    modifiers: ["private"],
                    format: ["camelCase"],
                    leadingUnderscore: "require",
                    trailingUnderscore: "forbid",
                },
                {
                    selector: "function",
                    modifiers: ["exported"],
                    format: ["camelCase", "UPPER_CASE"],
                },
                {
                    selector: "import",
                    format: ["camelCase", "PascalCase"],
                },
                {
                    selector: "parameterProperty",
                    modifiers: ["private"],
                    format: ["camelCase"],
                    leadingUnderscore: "require",
                    trailingUnderscore: "forbid",
                },
                {
                    selector: "parameter",
                    modifiers: ["unused"],
                    format: ["camelCase"],
                    leadingUnderscore: "require",
                    trailingUnderscore: "forbid",
                },
                {
                    selector: "objectLiteralProperty",
                    format: null,
                },
                {
                    selector: "typeProperty",
                    format: ["camelCase", "snake_case"],
                },
                {
                    selector: "variable",
                    modifiers: ["const"],
                    format: ["camelCase", "UPPER_CASE"],
                },
            ],
            "@typescript-eslint/no-confusing-void-expression": [
                "error",
                {
                    ignoreArrowShorthand: true,
                },
            ],
            "@typescript-eslint/no-empty-function": [
                "error",
                {
                    allow: ["constructors"],
                },
            ],
            "@typescript-eslint/no-explicit-any": "error",
            "@typescript-eslint/no-throw-literal": "off",
            "@typescript-eslint/no-unsafe-argument": "error",
            "@typescript-eslint/no-unsafe-assignment": "error",
            "@typescript-eslint/no-unsafe-member-access": "error",
            "@typescript-eslint/no-unsafe-return": "error",
            "curly": ["error", "all"],
            "no-console": "warn",
            "svelte/valid-compile": ["error", { ignoreWarnings: true }],
        },
    },
);
