# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj trước → tận dụng Docker layer cache
# (chỉ restore lại khi csproj thay đổi)
COPY ./src/MyApp.API/*.csproj ./
RUN dotnet restore

# Copy source code và publish
COPY ./src/MyApp.API/. ./
RUN dotnet publish -c Release -o /publish --no-restore

# ============================================
# Stage 2: Runtime (Production)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# --- Security: Chạy bằng non-root user ---
RUN groupadd -r appgroup && useradd -r -g appgroup -d /app -s /sbin/nologin appuser

# --- Timezone: Set giờ Việt Nam ---
ENV TZ=Asia/Ho_Chi_Minh
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# --- Environment: Production ---
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_gcServer=1

# --- Copy published output ---
COPY --from=build /publish .

# --- Security: Set ownership và switch user ---
RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080

# --- Healthcheck ---
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.API.dll"]