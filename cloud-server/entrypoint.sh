#!/bin/bash

cleanup() {
  echo -e "\nReceived SIGINT, stopping services..."
    
  kill -TERM "$DOTNET_PID"
  
  exit
}

trap cleanup SIGINT

sleep 1
echo -e "\nStarting dotnet cloud server\n"
dotnet cloud-server.dll &
DOTNET_PID=$!

wait $DOTNET_PID