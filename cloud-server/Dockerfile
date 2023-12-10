#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 6666

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["cloud-server.csproj", "."]
RUN dotnet restore "./cloud-server.csproj"
COPY . .
WORKDIR "/src/."
RUN rm -f DB/tables.sql
RUN dotnet build "cloud-server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cloud-server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY DB/tables.sql DB/tables.sql
ENTRYPOINT ["dotnet", "cloud-server.dll"]


#docker build -t docker-cloud_server:0.2 .
#docker run --net cloud_db-net --ip 172.18.0.3 --name cloud_server-container docker-cloud_server:0.2