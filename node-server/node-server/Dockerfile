#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 50052

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["node-server.csproj", "."]
RUN dotnet restore "./node-server.csproj"
COPY . .
RUN rm -rf ./file-saving-microservice
RUN rm  ./entrypoint.sh
WORKDIR "/src/."
RUN dotnet build "node-server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "node-server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./file-saving-microservice/ ./file-saving-microservice/
RUN chmod +r ./file-saving-microservice/dealer-dcs-150291856e98.json


RUN apt-get update
RUN apt-get install -y python3 python3-pip

RUN pip3 install --no-cache-dir -r ./file-saving-microservice/requirements.txt

COPY entrypoint.sh .
RUN chmod +x ./entrypoint.sh

RUN cat ./entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]


#docker build -t node_server:0.1 .
#docker run --rm --net system-net --ip 172.18.0.4 --name node_server-container node_server:0.1
#docker run --rm --net system-net --ip 172.18.0.4 -p 50052:50052 --name node_server-container node_server:0.1
