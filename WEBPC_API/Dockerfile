# Giai đoạn 1: Base Image (Dùng để chạy ứng dụng)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Giai đoạn 2: Build Image (Dùng SDK để compile code)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# --- LƯU Ý QUAN TRỌNG: COPY file .csproj ---
# Dòng dưới đây giả định file Dockerfile NẰM CÙNG CẤP với file WEBPC_API.csproj
# Nếu Dockerfile đang nằm chung với file .csproj, hãy dùng dòng này:
COPY ["WEBPC_API.csproj", "."]
RUN dotnet restore "./WEBPC_API.csproj"

# Copy toàn bộ source code vào
COPY . .
WORKDIR "/src/."

# Build dự án (Lưu ý: dùng $ thay vì % trên Linux)
RUN dotnet build "./WEBPC_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Giai đoạn 3: Publish (Đóng gói)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WEBPC_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Giai đoạn 4: Final (Tạo Image cuối cùng)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WEBPC_API.dll"]
