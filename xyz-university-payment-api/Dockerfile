FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["xyz-university-payment-api.cspoj", "/"]
RUN dotnet restore "xyz-university-payment-api.csproj"

COPY . .

RUN dotnet build "xyz-university-payment-api.csproj" -c Release -o /app/build

FROM  build AS publish
RUN dotnet publish "xyz-university-payment-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR  /app

COPY  --from=publish /app/publish .

ENTRYPOINT ["dotnet", "xyz-university-payment-api.dll"]