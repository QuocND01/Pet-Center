# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj trước để tận dụng cache
COPY PayrollAPI/PayrollAPI.csproj PayrollAPI/
RUN dotnet restore PayrollAPI/PayrollAPI.csproj

# copy toàn bộ source
COPY . .

# publish
RUN dotnet publish PayrollAPI/PayrollAPI.csproj -c Release -o /app/publish

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PayrollAPI.dll"]