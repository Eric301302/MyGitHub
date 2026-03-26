# 階段 1: 編譯專案 (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 複製方案檔與專案檔並進行還原 (利用快取機制)
COPY ["MyAPI.sln", "./"]
COPY ["myapi/MyAPI.csproj", "myapi/"]
RUN dotnet restore

# 複製其餘程式碼並編譯
COPY . .
WORKDIR "/src/myapi"
RUN dotnet build "MyAPI.csproj" -c Release -o /app/build

# 階段 2: 發布 (Publish)
FROM build AS publish
RUN dotnet publish "MyAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 階段 3: 運行環境 (Final)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyAPI.dll"]