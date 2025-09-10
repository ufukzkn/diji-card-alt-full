# Frontend build (Angular)
FROM node:20-alpine AS build
WORKDIR /app
COPY Frontend/package*.json ./Frontend/
COPY Frontend/angular.json Frontend/tsconfig*.json Frontend/.editorconfig ./Frontend/
WORKDIR /app/Frontend
RUN npm ci
COPY Frontend/ ./
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/Frontend/dist/Frontend/browser /usr/share/nginx/html
# Basic Angular routing support
RUN printf 'server {\n  listen 80;\n  root /usr/share/nginx/html;\n  location / { try_files $uri $uri/ /index.html; }\n}\n' > /etc/nginx/conf.d/default.conf
EXPOSE 80
