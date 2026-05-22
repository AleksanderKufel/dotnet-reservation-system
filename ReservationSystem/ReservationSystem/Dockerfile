# build stage
# Two steps of build COPY and RUN are used to leverage Docker's layer caching mechanism. 
# The first COPY and RUN dotnet restore will only be re-executed if the OrderSystem.API.csproj file changes, which can speed up subsequent builds when only source code changes.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ReservationSystem.API/ReservationSystem.API.csproj", "ReservationSystem.API/"]
COPY ["ReservationSystem.Infrastructure/ReservationSystem.Infrastructure.csproj", "ReservationSystem.Infrastructure/"]
COPY ["ReservationSystem.Application/ReservationSystem.Application.csproj", "ReservationSystem.Application/"]
COPY ["ReservationSystem.Domain/ReservationSystem.Domain.csproj", "ReservationSystem.Domain/"]

RUN dotnet restore "ReservationSystem.API/ReservationSystem.API.csproj"

COPY . .
WORKDIR "/src/ReservationSystem.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ReservationSystem.API.dll"]