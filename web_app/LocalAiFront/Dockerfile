FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY LocalAiFront.csproj .
RUN dotnet restore LocalAiFront.csproj
COPY . .
RUN dotnet publish LocalAiFront.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "LocalAiFront.dll"]
