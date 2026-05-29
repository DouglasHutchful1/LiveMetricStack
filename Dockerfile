FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LiveMetricStack.sln ./
COPY LiveMetricStack.Domain/LiveMetricStack.Domain.csproj LiveMetricStack.Domain/
COPY LiveMetricStack.Application/LiveMetricStack.Application.csproj LiveMetricStack.Application/
COPY LiveMetricStack.Infrastructure/LiveMetricStack.Infrastructure.csproj LiveMetricStack.Infrastructure/
COPY LiveMetricStack.WebApi/LiveMetricStack.WebApi.csproj LiveMetricStack.WebApi/

RUN dotnet restore LiveMetricStack.sln

COPY . .
RUN dotnet publish LiveMetricStack.WebApi/LiveMetricStack.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet LiveMetricStack.WebApi.dll --urls http://0.0.0.0:${PORT:-8080}"]
