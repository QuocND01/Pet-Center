# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj trước để tận dụng cache
COPY AttendanceAPI/AttendanceAPI.csproj AttendanceAPI/
RUN dotnet restore AttendanceAPI/AttendanceAPI.csproj

# copy toàn bộ source
COPY . .

# publish
RUN dotnet publish AttendanceAPI/AttendanceAPI.csproj -c Release -o /app/publish

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AttendanceAPI.dll"]