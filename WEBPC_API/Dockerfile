# 1. Dùng Image Linux (Sửa lỗi nanoserver của Windows)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 2. Build Image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# --- QUAN TRỌNG: Copy giữ nguyên cấu trúc thư mục ---
# Vì Root Directory để trống, ta đang đứng ở ngoài cùng
# Nên cần copy từ folder WEBPC_API vào trong Image
COPY ["WEBPC_API/WEBPC_API.csproj", "WEBPC_API/"]

# Restore các thư viện
RUN dotnet restore "WEBPC_API/WEBPC_API.csproj"

# Copy toàn bộ code vào
COPY . .

# Chuyển hướng vào thư mục chứa code chính để Build
WORKDIR "/src/WEBPC_API"
RUN dotnet build "WEBPC_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3. Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WEBPC_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WEBPC_API.dll"]
