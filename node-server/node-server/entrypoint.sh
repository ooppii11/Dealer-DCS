#!/bin/bash

# Function to handle cleanup on SIGINT
cleanup() {
  echo -e "\nReceived SIGINT, stopping services..."
  
  # Stop the Python microservice
  kill -TERM "$PYTHON_PID"
  
  # Stop the dotnet server
  kill -TERM "$DOTNET_PID"
  
  # Add any other cleanup steps if needed
  
  # Exit the script
  exit
}

# Trap SIGINT and call the cleanup function
trap cleanup SIGINT

# Start the file saving microservice
echo -e "Starting file saving microservice\n"
python3 file-saving-microservice/file_saving_microservice_server.py &
PYTHON_PID=$!

# Start the dotnet server
echo -e "\nStarting dotnet server\n"
dotnet node-server.dll &
DOTNET_PID=$!

# Wait for both processes to finish
wait $PYTHON_PID
wait $DOTNET_PID
