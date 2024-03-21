#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 6666
EXPOSE 50053

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["cloud-server.csproj", "."]
RUN dotnet restore "./cloud-server.csproj"
COPY . .
WORKDIR "/src/."
RUN rm -f DB/tables.sql
RUN rm  ./entrypoint.sh
RUN dotnet build "cloud-server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cloud-server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY DB/tables.sql DB/tables.sql


COPY entrypoint.sh .
RUN chmod +x ./entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]


#docker build -t cloud_server:0.1 .
#docker run --rm --net system-net --ip 172.18.0.3 --name cloud_server-container cloud_server:0.1
#docker run --rm --net system-net --ip 172.18.0.3 -p 50053:50053 --name cloud_server-container cloud_server:0.1