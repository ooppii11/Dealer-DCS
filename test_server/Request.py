import socket
import json


class RequestCodes:
    ADD_USER = 100
    AUTHENTICATE_USER = 200


class Request:
    def __init__(self, client:socket, request_code:int, request_data:dict={}) -> None:
        if request_code not in (RequestCodes.ADD_USER, RequestCodes.AUTHENTICATE_USER):
            raise ValueError("Invalid request code")
        
        self.__client = client
        self.__request_code = request_code
        self.__request_data = request_data
    

    @property
    def client(self):
        return self.__client
    

    @property
    def request_code(self):
        return self.__request_code
    
    
    @property
    def request_data(self):
        return self.__request_data

    
    @staticmethod
    def decode(client:socket, bytes:bytes):
        data_string = bytes.decode(encoding="utf-8")
        data_variable = json.loads(data_string)
        try:
            return Request(client, data_variable["request_code"], data_variable["request_data"])
        except KeyError as e:
            raise Exception("Invalid request format")