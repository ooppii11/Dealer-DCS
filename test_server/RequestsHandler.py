import threading
import socket
from User import User, Loctaions
from Request import Request, RequestCodes
from Response import Response, ErrorResponse, AcknowledgeResponse
from DBHandler import DBHandler


class RequestsHandler:
    def __init__(self, DB_Handler:DBHandler):
        self.__DB_Handler = DB_Handler
        self.__requests_mutex = threading.Lock()
        self.__condition_variable = threading.Condition(self.__requests_mutex)
        self.__requests = []
        self.__requests_actions = {
            # request_code: action function
            RequestCodes.ADD_USER: self.__add_user,
            RequestCodes.AUTHENTICATE_USER: self.__authenticate_user 
        }


    def __add_user(self, request_data:dict) -> Response:
        try:
            user_locations = Loctaions(request_data["first_location"], request_data["second_location"], request_data["third_location"])
            user = User(request_data["username"], request_data["password"], user_locations)
            self.__DB_Handler.add_user(user)
            return  AcknowledgeResponse()
        except KeyError as e:
            return ErrorResponse("Invalid request format")
        except Exception as e:
            return ErrorResponse(str(e))


    def __authenticate_user(self, request_data:dict) -> Response:
        try:
            user = self.__DB_Handler.authenticate_user(request_data["username"], request_data["password"])
            loctaions = {
                "locations": [
                    user.locations.first_location,
                    user.locations.second_location,
                    user.locations.third_location]
                }
            return AcknowledgeResponse(loctaions)
        except KeyError:
            print(request_data)
            return ErrorResponse("Invalid request fromat")
        except Exception as e:
            return ErrorResponse(str(e))


    def add_request(self, request:Request):
        with self.__requests_mutex:
                self.__requests.append(request)
                self.__condition_variable.notify()


    def __get_request(self) -> Request:
        with self.__requests_mutex:
            while len(self.__requests) == 0:
                self.__condition_variable.wait()  # Wait until notified
            return self.__requests.pop(0) 

        
    def handle_requests(self):
        while True:
            request = self.__get_request()
            try:
                response = self.__requests_actions[request.request_code](request.request_data)
            except KeyError as e:  # Invalid  request code
                response = ErrorResponse("Invalid  request code")
            try:
                request.client.send(response.encode())
            finally:
                request.client.close()
