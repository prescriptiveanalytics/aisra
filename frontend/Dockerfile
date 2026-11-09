# syntax=docker/dockerfile:1

# Both build stages run natively; their output is plain JavaScript that runs on every target platform.
FROM --platform=$BUILDPLATFORM node:24-alpine AS base
WORKDIR /app
RUN corepack enable
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml .npmrc ./

FROM base AS build
RUN pnpm install --frozen-lockfile
COPY . .
RUN pnpm build

# Only the runtime dependencies, without the platform-specific build tools.
FROM base AS deps
RUN pnpm install --frozen-lockfile --prod

FROM node:24-alpine AS runtime
WORKDIR /app

COPY --from=deps /app/package.json ./
COPY --from=deps /app/node_modules ./node_modules
COPY --from=build /app/build ./build

ENV NODE_ENV=production
# The URL the browser uses to reach the API; read at runtime, so it is set on the container.
ENV PUBLIC_API_URL=

USER node
EXPOSE 3000
ENTRYPOINT ["node", "build/index.js"]
