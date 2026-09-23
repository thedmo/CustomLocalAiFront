FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Chat.Core/Chat.Core.csproj src/Chat.Core/
COPY src/Chat.Adapter.ModelRunner/Chat.Adapter.ModelRunner.csproj src/Chat.Adapter.ModelRunner/
COPY src/LocalAiFront/LocalAiFront.csproj src/LocalAiFront/
RUN dotnet restore src/LocalAiFront/LocalAiFront.csproj

COPY src/ src/
COPY src/LocalAiFront/ src/LocalAiFront/
RUN dotnet publish src/LocalAiFront/LocalAiFront.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "LocalAiFront.dll"]
