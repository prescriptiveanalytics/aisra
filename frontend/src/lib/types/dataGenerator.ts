/**
 * Types and form metadata for the data stream generator (dsg-web).
 * The field lists mirror `GeneratorConfig` and the `SeriesConfig` types of the DataStreamGenerator library.
 */

export type ConfigValue = string | number | boolean | null | ConfigValue[] | ConfigObject;
export type ConfigObject = { [key: string]: ConfigValue };

export type GeneratorConfig = ConfigObject;

export type SeriesConfig = ConfigObject & { seriesType: SeriesType; id: string };

export type DataGeneratorConfig = {
    generator: GeneratorConfig;
    series: SeriesConfig[];
};

export type DataGeneratorStatus = {
    status: "running" | "stopped";
    hasConfiguration: boolean;
    hasPendingConfiguration: boolean;
    startedAt: string | null;
    generatorId: string | null;
    generatorType: string | null;
};

export type Driver = { id: string; p: number[] | null; q: number[] | null };

export type FieldKind =
    | "text"
    | "number"
    | "integer"
    | "boolean"
    | "datetime"
    | "select"
    | "numberList"
    | "integerList"
    /** Comma-separated strings. */
    | "stringList"
    /** One string per line, for values that may contain commas (e.g. expressions). */
    | "lines"
    | "drivers";

export type FieldDescriptor = {
    key: string;
    label: string;
    kind: FieldKind;
    options?: readonly string[];
    hint?: string;
    defaultValue: ConfigValue;
};

export function isStreamingType(type: ConfigValue): boolean {
    return type === "stream1" || type === "stream2";
}

export type FieldSection = { title: string; fields: FieldDescriptor[]; streaming?: boolean };

export const GENERATOR_SECTIONS: FieldSection[] = [
    {
        title: "General",
        fields: [
            {
                key: "seed",
                label: "Seed",
                kind: "integer",
                hint: "≥ 0 for a fixed seed, -1 for a random one.",
                defaultValue: -1,
            },
            {
                key: "interval",
                label: "Interval (ms)",
                kind: "integer",
                hint: "Check interval of the generator.",
                defaultValue: 100,
            },
            {
                key: "duration",
                label: "Duration",
                kind: "integer",
                hint: "In ms.",
                defaultValue: 60000,
            },
            {
                key: "startDateTime",
                label: "Start date/time",
                kind: "datetime",
                defaultValue: "0001-01-01T00:00:00",
            },
            {
                key: "decimalPrecision",
                label: "Decimal precision",
                kind: "integer",
                defaultValue: 4,
            },
            {
                key: "shuffle",
                label: "Shuffle",
                kind: "boolean",
                hint: "Shuffle the order of series every iteration instead of using the rank.",
                defaultValue: false,
            },
            { key: "disclosure", label: "Disclosure", kind: "boolean", defaultValue: false },
        ],
    },
    {
        // Only shown if a file generation type was set in the JSON view.
        title: "File export",
        streaming: false,
        fields: [
            {
                key: "outputFilePath",
                label: "Output file path",
                kind: "text",
                hint: "Path inside the generator container.",
                defaultValue: null,
            },
            { key: "separator", label: "Separator", kind: "text", defaultValue: null },
            {
                key: "exportIdAsHeader",
                label: "Export IDs as header",
                kind: "boolean",
                defaultValue: true,
            },
            {
                key: "exportDateTime",
                label: "Export date/time",
                kind: "boolean",
                defaultValue: true,
            },
            {
                key: "exportEventCount",
                label: "Export event count",
                kind: "boolean",
                defaultValue: true,
            },
            { key: "exportDiff", label: "Export difference", kind: "boolean", defaultValue: false },
            {
                key: "exportDiffFPS",
                label: "Five-point stencil difference",
                kind: "boolean",
                defaultValue: false,
            },
            {
                key: "exportMovingAvg",
                label: "Export moving average",
                kind: "boolean",
                defaultValue: false,
            },
            {
                key: "exportMovingAvgWindowSize",
                label: "Moving average window size",
                kind: "integer",
                defaultValue: 0,
            },
            { key: "exportLagOffset", label: "Lag offset", kind: "integer", defaultValue: 0 },
            {
                key: "exportLags",
                label: "Lags",
                kind: "integerList",
                hint: "Comma-separated.",
                defaultValue: null,
            },
        ],
    },
];

export const SERIES_TYPES = ["AR", "ARMA", "ARIMA", "ME", "MEC", "MEMC", "XF", "XG", "IS"] as const;
export type SeriesType = (typeof SERIES_TYPES)[number];

export const SERIES_TYPE_LABELS: Record<SeriesType, string> = {
    AR: "AR – autoregressive",
    ARMA: "ARMA – autoregressive moving average",
    ARIMA: "ARIMA – autoregressive integrated moving average",
    ME: "ME – mathematical expression",
    MEC: "MEC – mathematical expression with condition",
    MEMC: "MEMC – mathematical expression with multiple conditions",
    XF: "XF – external file",
    XG: "XG – external generator (streaming only)",
    IS: "IS – integer sequence",
};

export const COMMON_SERIES_FIELDS: FieldDescriptor[] = [
    { key: "id", label: "ID", kind: "text", hint: "Also used as the title.", defaultValue: "" },
    { key: "topic", label: "MQTT topic", kind: "text", defaultValue: null },
    {
        key: "export",
        label: "Export",
        kind: "boolean",
        hint: "Publish/write this series. Non-exported series can still drive others.",
        defaultValue: true,
    },
    { key: "rank", label: "Rank", kind: "integer", defaultValue: 1 },
    { key: "delay", label: "Delay", kind: "integer", defaultValue: 0 },
    { key: "interval", label: "Interval (ms)", kind: "integer", defaultValue: 500 },
    { key: "initialValue", label: "Initial value", kind: "number", defaultValue: 0 },
    { key: "outlierRatio1s", label: "Outlier ratio (1σ)", kind: "number", defaultValue: 0 },
    { key: "outlierRatio2s", label: "Outlier ratio (2σ)", kind: "number", defaultValue: 0 },
];

const c: FieldDescriptor = { key: "c", label: "Constant (c)", kind: "number", defaultValue: 0 };
const mean: FieldDescriptor = { key: "mean", label: "Noise mean", kind: "number", defaultValue: 0 };
const stdDev: FieldDescriptor = {
    key: "stdDev",
    label: "Noise std. dev.",
    kind: "number",
    defaultValue: 0,
};
const p: FieldDescriptor = {
    key: "p",
    label: "AR coefficients (p)",
    kind: "numberList",
    hint: "Comma-separated.",
    defaultValue: [0.5],
};
const q: FieldDescriptor = {
    key: "q",
    label: "MA coefficients (q)",
    kind: "numberList",
    hint: "Comma-separated.",
    defaultValue: [],
};
const drivers: FieldDescriptor = {
    key: "drivers",
    label: "Drivers",
    kind: "drivers",
    hint: "Other series that influence this one.",
    defaultValue: null,
};
const args: FieldDescriptor = {
    key: "arguments",
    label: "Arguments",
    kind: "stringList",
    hint: "Comma-separated. i = iteration counter, or IDs of other series.",
    defaultValue: ["i"],
};
const expression: FieldDescriptor = {
    key: "expression",
    label: "Expression",
    kind: "text",
    defaultValue: "sin(i/10)",
};

export const SERIES_TYPE_FIELDS: Record<SeriesType, FieldDescriptor[]> = {
    AR: [c, mean, stdDev, p, drivers],
    ARMA: [c, mean, stdDev, p, q, drivers],
    ARIMA: [
        c,
        mean,
        stdDev,
        p,
        q,
        { key: "i", label: "Order of differencing (i)", kind: "integer", defaultValue: 0 },
        drivers,
    ],
    ME: [c, mean, stdDev, args, expression, drivers],
    MEC: [
        c,
        mean,
        stdDev,
        args,
        { key: "condition", label: "Condition", kind: "text", defaultValue: null },
        { ...expression, label: "Expression (condition true)" },
        {
            key: "expressionF",
            label: "Expression (condition false)",
            kind: "text",
            defaultValue: null,
        },
        drivers,
    ],
    MEMC: [
        c,
        mean,
        stdDev,
        args,
        {
            key: "conditions",
            label: "Conditions",
            kind: "lines",
            hint: "One per line.",
            defaultValue: null,
        },
        {
            key: "expressions",
            label: "Expressions",
            kind: "lines",
            hint: "One per line, matching the conditions.",
            defaultValue: null,
        },
        drivers,
    ],
    XF: [
        {
            key: "sourcePath",
            label: "Source file path",
            kind: "text",
            hint: "Path inside the generator container.",
            defaultValue: null,
        },
        { key: "sourceIndex", label: "Source index", kind: "text", defaultValue: null },
    ],
    XG: [
        { key: "sourceBroker", label: "Source broker", kind: "text", defaultValue: null },
        { key: "sourceTopic", label: "Source topic", kind: "text", defaultValue: null },
    ],
    IS: [
        { key: "min", label: "Min", kind: "integer", defaultValue: 0 },
        { key: "max", label: "Max", kind: "integer", defaultValue: 10 },
        { key: "minLength", label: "Min length", kind: "integer", defaultValue: 1 },
        { key: "maxLength", label: "Max length", kind: "integer", defaultValue: 5 },
        { key: "strategy", label: "Strategy", kind: "text", defaultValue: null },
    ],
};

/**
 * Creates a series of the given type, keeping the common fields and every type-specific field
 * the new type shares with `from`.
 */
export function createSeries(type: SeriesType, from?: SeriesConfig): SeriesConfig {
    const series: SeriesConfig = { seriesType: type, id: "", title: from?.title ?? null };

    for (const field of [...COMMON_SERIES_FIELDS, ...SERIES_TYPE_FIELDS[type]]) {
        series[field.key] =
            from && field.key in from ? from[field.key] : structuredClone(field.defaultValue);
    }

    return series;
}

/**
 * Extracts a readable error message from an error response body of the backend or dsg-web.
 */
export function getErrorMessage(body: unknown, fallback: string): string {
    if (typeof body !== "object" || body === null) {
        return fallback;
    }

    const record = body as Record<string, unknown>;

    if (typeof record.error === "string") {
        return record.error;
    }

    // ASP.NET Core validation problem details.
    if (typeof record.errors === "object" && record.errors !== null) {
        const messages = Object.values(record.errors as Record<string, string[]>).flat();
        if (messages.length > 0) {
            return messages.join(" ");
        }
    }

    if (typeof record.title === "string") {
        return record.title;
    }

    return fallback;
}
