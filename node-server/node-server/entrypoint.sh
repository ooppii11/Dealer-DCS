#!/bin/bash

cleanup() {
  echo -e "\nReceived SIGINT, stopping services..."
  
  kill -TERM "$PYTHON_PID"
  
  kill -TERM "$DOTNET_PID"
  
  exit
}

trap cleanup SIGINT

echo -e "Starting file saving microservice\n"
python3 file-saving-microservice/file_saving_microservice_server.py &
PYTHON_PID=$!

echo -e "\nStarting dotnet server\n"
dotnet node-server.dll &
DOTNET_PID=$!

wait $PYTHON_PID
wait $DOTNET_PID