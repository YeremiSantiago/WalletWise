FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src 

COPY WalletWise.Domain/WalletWise.Domain.csproj WalletWise.Domain/
COPY WalletWise.Application/WalletWise.Application.csproj WalletWise.Application/
COPY WalletWise.Infrastructure/WalletWise.Infrastructure.csproj WalletWise.Infrastructure/
COPY WalletWise.WebApi/WalletWise.WebApi.csproj WalletWise.WebApi/

RUN dotnet restore WalletWise.WebApi/WalletWise.WebApi.csproj

COPY . .

RUN dotnet publish WalletWise.WebApi/WalletWise.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WalletWise.WebApi.dll"] 