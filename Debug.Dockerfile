# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY BlazorEditor/*.csproj ./BlazorEditor/
RUN dotnet restore

# copy everything else and build app
COPY BlazorEditor/. ./BlazorEditor/
WORKDIR /source/BlazorEditor
RUN dotnet publish -c debug -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "BlazorEditor.dll"]
