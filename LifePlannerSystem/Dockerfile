# --- STAGE 1: Build i ASP.NET Core MVC ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LifePlanner.csproj", "./"]
RUN dotnet restore "LifePlanner.csproj"
COPY . .
RUN dotnet publish "LifePlanner.csproj" -c Release -o /app/publish --no-restore

# --- STAGE 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 10000

ENTRYPOINT ["dotnet", "LifePlanner.dll"]