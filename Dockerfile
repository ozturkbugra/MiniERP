# 1. ÇALIŞMA ZAMANI
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. DERLEME AŞAMASI
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Bütün solution içeriğini kopyalıyoruz
COPY . .

# 🚀 ÖNEMLİ: Senin klasör adın MiniERP.WebAPI olduğu için yolları buna göre güncelledik
RUN dotnet restore "MiniERP.WebAPI/MiniERP.WebAPI.csproj"
RUN dotnet build "MiniERP.WebAPI/MiniERP.WebAPI.csproj" -c Release -o /app/build

# 3. PUBLISH AŞAMASI
FROM build AS publish
RUN dotnet publish "MiniERP.WebAPI/MiniERP.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. FİNAL
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Çalıştırılacak DLL adını da güncelledik
ENTRYPOINT ["dotnet", "MiniERP.WebAPI.dll"]