FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 5263
EXPOSE 7263

ENV ASPNETCORE_URLS=https://+:7263;http://+:5263

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["ProfileAPI.csproj", "ProfileAPI/"]
RUN dotnet restore "ProfileAPI/ProfileAPI.csproj"
WORKDIR "/src/ProfileAPI"
COPY . .

RUN dotnet build "ProfileAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProfileAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProfileAPI.dll"]
