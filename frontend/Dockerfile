# GLA frontend — build the SPA with Node, serve it with nginx.
# Both base images are multi-arch (amd64 + arm64), so the image builds natively
# on an ARM64 host; the target platform is set in docker-compose.yml.

FROM node:24-slim AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# `build` = vue-tsc type-check + vite build → dist/. The alphatab vite plugin
# also copies the Bravura font and SONiVOX soundfont into public/ during the
# build, so dist/ carries them too.
RUN npm run build

FROM nginx:1.29-alpine AS runtime

# SPA server + /api reverse proxy (see the file for the routing contract).
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
