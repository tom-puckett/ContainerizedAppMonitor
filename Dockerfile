FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV MAP_DATA_MOUNT_PATH="/mnt/testing"
VOLUME /mnt/testing

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY ["ContainerizedAppMonitor/ContainerizedAppMonitor/ContainerizedAppMonitor.csproj", "ContainerizedAppMonitor/"]
RUN dotnet restore "ContainerizedAppMonitor/ContainerizedAppMonitor.csproj"
COPY ContainerizedAppMonitor/. .
WORKDIR "/ContainerizedAppMonitor"
RUN dotnet build "ContainerizedAppMonitor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContainerizedAppMonitor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContainerizedAppMonitor.dll"]
