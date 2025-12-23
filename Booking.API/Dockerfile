FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PMS.sln", "./"]
COPY ["global.json", "./"]
COPY ["Booking.API/Booking.API.csproj", "Booking.API/"]
COPY ["Booking.Application/Booking.Application.csproj", "Booking.Application/"]
COPY ["Booking.Infrastrcure.Persistent/Booking.Infrastrcure.Persistent.csproj", "Booking.Infrastrcure.Persistent/"]
COPY ["Booking.Model.Shared/Booking.Model.Shared.csproj", "Booking.Model.Shared/"]
COPY ["code/Services/Booking/Booking.Model/Booking.Model.csproj", "code/Services/Booking/Booking.Model/"]
COPY ["SharedKernel.EventDriven/SharedKernel.EventDriven.csproj", "SharedKernel.EventDriven/"]
COPY ["SharedKernel.EventDriven.Abstraction/SharedKernel.EventDriven.Abstraction.csproj", "SharedKernel.EventDriven.Abstraction/"]
COPY ["SharedKernel.Infrastrucrure.Abstraction/SharedKernel.Infrastrucrure.Abstraction.csproj", "SharedKernel.Infrastrucrure.Abstraction/"]
COPY ["SharedKernel.Infratsrucrure.Persistent/SharedKernel.Infratsrucrure.Persistent.csproj", "SharedKernel.Infratsrucrure.Persistent/"]
COPY ["SharedKernel.Infratsrucrure.Persistent.Abstraction/SharedKernel.Infratsrucrure.Persistent.Abstraction.csproj", "SharedKernel.Infratsrucrure.Persistent.Abstraction/"]
COPY ["SharedKernel.MessageBus.Abstraction/SharedKernel.MessageBus.Abstraction.csproj", "SharedKernel.MessageBus.Abstraction/"]
COPY ["SharedKernel.MessageBus.Kafka/SharedKernel.MessageBus.Kafka.csproj", "SharedKernel.MessageBus.Kafka/"]
COPY ["SharedKernel.Models.Enum/SharedKernel.Models.Enum.csproj", "SharedKernel.Models.Enum/"]
COPY ["SharedKernel.Persistent/SharedKernel.Persistent.csproj", "SharedKernel.Persistent/"]
COPY ["SharedKernel.Logging/SharedKernel.Logging.csproj", "SharedKernel.Logging/"]
COPY ["Site.Model.Shared/Site.Model.Shared.csproj", "Site.Model.Shared/"]


RUN dotnet restore "Booking.API/Booking.API.csproj"
COPY . .
WORKDIR "/src/Booking.API"
RUN dotnet build "Booking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Booking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup --shell /bin/sh appuser && \
    mkdir -p /app/logs && \
    chown -R appuser:appgroup /app/logs
COPY --from=publish /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/api/booking/health || exit 1
ENTRYPOINT ["dotnet", "Booking.API.dll"]
