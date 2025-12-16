FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PMS.sln", "./"]
COPY ["global.json", "./"]
COPY ["ServiceTemplate/ServiceTemplate.csproj", "ServiceTemplate/"]
COPY ["ServiceTemplate.Application/ServiceTemplate.Application.csproj", "ServiceTemplate.Application/"]
COPY ["ServiceTemplate.Infrastrcure.Persistent/ServiceTemplate.Infrastrcure.Persistent.csproj", "ServiceTemplate.Infrastrcure.Persistent/"]
COPY ["ServiceTemplate.Model.Shared/ServiceTemplate.Model.Shared.csproj", "ServiceTemplate.Model.Shared/"]
COPY ["SharedKernel.EventDriven/SharedKernel.EventDriven.csproj", "SharedKernel.EventDriven/"]
COPY ["SharedKernel.EventDriven.Abstraction/SharedKernel.EventDriven.Abstraction.csproj", "SharedKernel.EventDriven.Abstraction/"]
COPY ["SharedKernel.Infrastrucrure.Abstraction/SharedKernel.Infrastrucrure.Abstraction.csproj", "SharedKernel.Infrastrucrure.Abstraction/"]
COPY ["SharedKernel.Infratsrucrure.Persistent/SharedKernel.Infratsrucrure.Persistent.csproj", "SharedKernel.Infratsrucrure.Persistent/"]
COPY ["SharedKernel.Infratsrucrure.Persistent.Abstraction/SharedKernel.Infratsrucrure.Persistent.Abstraction.csproj", "SharedKernel.Infratsrucrure.Persistent.Abstraction/"]
COPY ["SharedKernel.MessageBus.Abstraction/SharedKernel.MessageBus.Abstraction.csproj", "SharedKernel.MessageBus.Abstraction/"]
COPY ["SharedKernel.MessageBus.Kafka/SharedKernel.MessageBus.Kafka.csproj", "SharedKernel.MessageBus.Kafka/"]
COPY ["SharedKernel.Models.Enum/SharedKernel.Models.Enum.csproj", "SharedKernel.Models.Enum/"]
COPY ["SharedKernel.Persistent/SharedKernel.Persistent.csproj", "SharedKernel.Persistent/"]

RUN dotnet restore "ServiceTemplate/ServiceTemplate.csproj"
COPY . .
WORKDIR "/src/ServiceTemplate"
RUN dotnet build "ServiceTemplate.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ServiceTemplate.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup --shell /bin/sh appuser
COPY --from=publish /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "ServiceTemplate.dll"]