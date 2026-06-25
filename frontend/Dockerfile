FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:22-alpine
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=3000
ENV BACKEND_HOST=127.0.0.1:8080
COPY --from=build /app/dist ./dist
COPY server.mjs .
EXPOSE 3000
CMD ["node", "server.mjs"]
