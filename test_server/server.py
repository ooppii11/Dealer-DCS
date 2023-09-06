import threading
import socket
from Request import Request
from Response import ErrorResponse
from DBHandler import DBHandler
from RequestsHandler import RequestsHandler


HOST = "127.0.0.1"
PORT = 55555
BUFF_SIZE = 1024

class Server:
    def __init__(self, host=HOST, port=PORT):
        self.__host = host
        self.__port = port 
        self.__server_socket = None


    def run(self):
        self.__DB_Handler = DBHandler()
        self.__requests_handler = RequestsHandler(self.__DB_Handler)
        self.__server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.__server_socket.bind((self.__host, self.__port))
        self.__server_socket.listen()

        print("Server listening on", self.__host, self.__port)

        receive_thread = threading.Thread(target=self.receive_requests)
        handle_thread = threading.Thread(target=self.__requests_handler.handle_requests)
        
        receive_thread.start()
        handle_thread.start()

        receive_thread.join()
        handle_thread.join()


    def receive_requests(self):
        while True:
            client_socket, _ = self.__server_socket.accept()
            try:
                request = Request.decode(client_socket, self.recvall(client_socket))
                self.__requests_handler.add_request(request)
            except Exception as e:
                client_socket.send(ErrorResponse(e.args[0]).encode())
                client_socket.close()


    def recvall(self, sock) -> bytes:
        data = b''
        while True:
            try:
                part = sock.recv(BUFF_SIZE)
                data += part
                if len(part) < BUFF_SIZE:
                    break
            except: 
                pass
        return data


                    