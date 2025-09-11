# Frontend build (Angular) - use official Node for npm availability
FROM node:20-alpine AS build
WORKDIR /app/Frontend
COPY Frontend/package*.json ./
COPY Frontend/angular.json Frontend/tsconfig*.json Frontend/.editorconfig ./
RUN npm ci --legacy-peer-deps
COPY Frontend/ ./
RUN npm run build
# Prepare nginx SPA config (no shell in runtime image, so create here)
RUN printf 'server {\n  listen 80;\n  server_name _;\n  root /usr/share/nginx/html;\n  index index.html;\n  location / {\n    try_files $uri $uri/ /index.html;\n  }\n  location /assets/ {\n    add_header Cache-Control "public, max-age=31536000, immutable";\n    try_files $uri =404;\n  }\n}\n' > /tmp/default.conf

# Switched to Chainguard's hardened nginx image to address vulnerabilities
FROM cgr.dev/chainguard/nginx:latest AS runtime
COPY --from=build /app/Frontend/dist/Frontend/browser /usr/share/nginx/html
COPY --from=build /tmp/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
