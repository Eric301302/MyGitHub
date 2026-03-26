# 階段 1: 編譯 (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. 只複製專案檔與方案檔 (加速快取)
# 如果 .csproj 就在根目錄，直接用下行：
COPY *.sln ./
COPY *.csproj ./
RUN dotnet restore

# 2. 複製其餘所有檔案
COPY . .

# 3. 執行編譯 (確保專案檔名稱正確)
# 注意：這裡的 MyAPI.csproj 必須與你資料夾中的檔名大小寫完全一致
RUN dotnet build "MyAPI.csproj" -c Release -o /app/build

# 階段 2: 發布 (Publish)
FROM build AS publish
RUN dotnet publish "MyAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 階段 3: 運行 (Final)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyAPI.dll"]