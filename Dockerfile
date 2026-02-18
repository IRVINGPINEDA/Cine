FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Cine.sln ./
COPY Cine.Web/Cine.Web.csproj Cine.Web/
RUN dotnet restore Cine.Web/Cine.Web.csproj

COPY . .
RUN dotnet publish Cine.Web/Cine.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./
RUN mkdir -p /app/wwwroot/uploads

EXPOSE 8080
ENTRYPOINT ["dotnet", "Cine.Web.dll"]
