# Use the official .NET image for building
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5256

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "TheDriveAPI.csproj"
RUN dotnet build "TheDriveAPI.csproj" -c Release -o /app/build
RUN dotnet publish "TheDriveAPI.csproj" -c Release -o /app/publish
# Install EF tools for migrations
RUN dotnet tool install --global dotnet-ef

FROM base AS final
WORKDIR /app
# Set PATH for dotnet tools
ENV PATH="$PATH:/root/.dotnet/tools"
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TheDriveAPI.dll"]
