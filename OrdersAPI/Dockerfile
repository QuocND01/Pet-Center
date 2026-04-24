# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj trước để tận dụng cache
COPY OrdersAPI/OrdersAPI.csproj OrdersAPI/
RUN dotnet restore OrdersAPI/OrdersAPI.csproj

# copy toàn bộ source
COPY . .

# publish
RUN dotnet publish OrdersAPI/OrdersAPI.csproj -c Release -o /app/publish

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrdersAPI.dll"]