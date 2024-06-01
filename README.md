# Dealer DCS: Distributed Cloud Storage System

Dealer DCS is a distributed cloud storage system that leverages gRPC and Docker to provide scalable and reliable cloud storage solutions. This project includes a variety of components, including a Python CLI client, a central cloud server, and multiple node servers to handle data storage and synchronization. It employs the Raft consensus algorithm for data consistency and dynamic leader election, introducing an enhanced Viewer mode to efficiently track the current leader.

## Project Structure

### Client
- **protos**: Protocol buffer files defining the gRPC interfaces.
- **android (xamarin)**: Xamarin-based Android client code.
- **CLI (in python)**: Python-based Command Line Interface for interacting with the system.

### Cloud-Server
- **DB**: PostgreSQL related code in C#.
- **Managers**: C# code for handling Authentication, File Metadata, Files Management, and Raft Viewer Logger.
- **Protos**: Protocol buffer files for the cloud server.
- **Services**: C# services including CloudGrpcService and NodeServerCommunication.
- **Tests**: Test code for verifying the cloud server functionality.
- **Utilities**: Utility classes in C#, including Converter and Exceptions.
- **dockerfile**: Dockerfile for building the cloud server image.
- **entrypoint.sh**: Entrypoint script for the cloud server Docker container.

### Design
- **umls**: UML diagrams created in draw.io to illustrate the system design and architecture.

### Docker
- **docker compose files**: Docker Compose files organized in directories for orchestrating multi-container Docker applications.

### Node-Server
- **file-saving-microservice**: Python code for a microservice designed to communicate with Google Cloud for file saving.
- **Managers**: 
  - **Raft**: Self-implementation of the Raft algorithm and related log management in C#.
  - **RPCClients**: RPC client implementations.
  - **Action**: Action handling classes.
  - **ActionMaker**: Action creation logic.
  - **DynamicStorageActionsManager**: Manages dynamic storage actions.
  - **FileVersionManager (SQLite)**: Manages file versions using SQLite.
  - **IDynamicActions**: Interfaces for dynamic actions.
  - **NewConnectionLogger**: Logs new connections.
- **Protos**: Protocol buffer files for the node server.
- **Services**: 
  - **NodeServerService**: C# code for the primary node server service.
  - **ServerToServerService**: C# code for inter-server communication.
- **Tests**: Test code for verifying node server functionality.
- **Utilities**: 
  - **OnMachineStorageActions**: C# utilities for storage actions on the machine.
- **dockerfile**: Dockerfile for building the node server image.
- **entrypoint.sh**: Entrypoint script for the node server Docker container.

## Getting Started

### Prerequisites
- Docker and Docker Compose
- .NET SDK
- Python 3
- Xamarin (for Android client)

### Setup

1. **Clone the Repository**
   ```sh
   git clone https://github.com/ooppii11/Dealer-DCS.git
   cd dealer-dcs
   ```

2. **Build and Run the Cloud Server**
   ```sh
   cd cloud-server
   docker build -t cloud-server .
   ```

3. **Build and Run the Node Server**
   ```sh
   cd node-server
   docker build -t node-server .
   ```
4. **Run the system in docker using compose
   ```sh
   cd docker\system_docker_compose
   docker compose up -d
   ```
5. **Run the Python CLI Client**
   ```sh
   cd client/CLI
   python client.py
   ```


## Contact

Project Link: [https://github.com/ooppii11/Dealer-DCS.git](https://github.com/ooppii11/Dealer-DCS.git)

---
