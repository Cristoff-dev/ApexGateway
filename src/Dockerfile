FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["ApexGateway.csproj", "./"]
RUN dotnet restore "ApexGateway.csproj"

COPY . .
RUN dotnet publish "ApexGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
EXPOSE 8080

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_HTTP_PORTS=8080

USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ApexGateway.dll"]
