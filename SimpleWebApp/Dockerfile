FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /src

COPY ["SimpleWebApp.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish "SimpleWebApp.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SimpleWebApp.dll"]
