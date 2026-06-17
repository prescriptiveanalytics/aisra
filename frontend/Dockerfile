FROM node:24-alpine AS build
ARG PUBLIC_API_URL
ENV PUBLIC_API_URL=$PUBLIC_API_URL
WORKDIR /app
RUN corepack enable

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml .npmrc ./
RUN pnpm install

COPY . .
RUN pnpm build

FROM node:24-alpine AS runtime
WORKDIR /app
RUN corepack enable

COPY --from=build /app/build ./build
COPY --from=build /app/package.json ./
COPY --from=build /app/node_modules ./node_modules

EXPOSE 3000
ENTRYPOINT ["node", "build/index.js"]
