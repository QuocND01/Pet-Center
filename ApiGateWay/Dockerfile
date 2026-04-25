# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj trước để tận dụng cache
COPY ApiGateWay/ApiGateWay.csproj ApiGateWay/
RUN dotnet restore ApiGateWay/ApiGateWay.csproj

# copy toàn bộ source
COPY . .

# publish
RUN dotnet publish ApiGateWay/ApiGateWay.csproj -c Release -o /app/publish

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ApiGateWay.dll"]