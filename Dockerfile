# Backend multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY digital-business-card.csproj ./
COPY *.sln ./
RUN dotnet restore digital-business-card.csproj
COPY . .
RUN dotnet publish digital-business-card.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:5078
EXPOSE 5078
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","digital-business-card.dll"]
