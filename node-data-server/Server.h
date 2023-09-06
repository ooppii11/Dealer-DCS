#pragma once
#include <WinSock2.h>
#include <Windows.h>
#include <mutex>
#include <map>
#include <queue>
#include "IRequest.h"


class Server
{
public:
	Server();
	~Server();
	void serve(int port);

private:
	SOCKET _serverSocket;
	std::map<std::string, SOCKET> _users;
	std::queue<IRequest*> _requests;
	std::mutex _requestsMutex;
	std::condition_variable _empty;


	//void acceptClient();
//	void clientHandler(SOCKET clientSocket); 
//	void requestsHandler();
};
