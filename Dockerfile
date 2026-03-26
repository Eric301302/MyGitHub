# 階段 1: 編譯 (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 複製當前目錄下所有的 .sln 和 .csproj (這會抓到你的專案檔)
COPY *.sln ./
COPY *.csproj ./
RUN dotnet restore

# 複製剩下的所有原始碼
COPY . .

# 執行編譯 (請將 "MyAPI.csproj" 替換成你實際的檔案名稱，注意大小寫)
RUN dotnet build "MyAPI.csproj" -c Release -o /app/build

# 階段 2: 發布 (Publish)
FROM build AS publish
RUN dotnet publish "MyAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 階段 3: 運行環境 (Final)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyAPI.dll"]