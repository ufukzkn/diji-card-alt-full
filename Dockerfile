################################################
# Stable framework-dependent build (simplified) #
################################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY digital-business-card.csproj ./
COPY *.sln ./
RUN dotnet restore digital-business-card.csproj
COPY . .
RUN dotnet publish digital-business-card.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ENV ASPNETCORE_URLS=http://0.0.0.0:5078
WORKDIR /app
COPY --from=build /app/publish/ ./
# Create internal seed copy (publish already copied). If default.png missing log a warning.
RUN mkdir -p /app/profile-photo-seed /app/wwwroot/profile-photos \
	&& if [ -f /app/wwwroot/profile-photos/default.png ]; then cp /app/wwwroot/profile-photos/default.png /app/profile-photo-seed/default.png; else echo "[WARN] default.png missing"; fi
EXPOSE 5078
ENTRYPOINT ["dotnet","digital-business-card.dll"]
