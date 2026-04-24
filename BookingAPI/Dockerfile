# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj trước để tận dụng cache
COPY BookingAPI/BookingAPI.csproj BookingAPI/
RUN dotnet restore BookingAPI/BookingAPI.csproj

# copy toàn bộ source
COPY . .

# publish
RUN dotnet publish BookingAPI/BookingAPI.csproj -c Release -o /app/publish

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookingAPI.dll"]