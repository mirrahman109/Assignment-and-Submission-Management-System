# Single build stage: installing and building in the same layer avoids copying node_modules
# across stages, which is by far the most expensive thing a multi-stage Node image can do.
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

ARG NEXT_PUBLIC_API_URL=http://localhost:5000
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}

COPY . .
RUN npm run build

# next.config.ts sets output: "standalone", so .next/standalone holds the server plus only the
# dependencies it actually traced — a fraction of node_modules, and the runtime image never has
# to carry the dev toolchain.
FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

COPY --from=build /app/public ./public
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static

EXPOSE 3000

CMD ["node", "server.js"]
