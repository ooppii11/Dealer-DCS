import json


class ResponseCodes:
    ACKNOWLEDGE = 300
    ERROR = 400


class Response:
    def __init__(self, response_code:int, response_data:dict={}) -> None:
        self.__response_code = response_code
        self.__response_data = response_data

    @property
    def response_code(self):
        return self.__response_code
    
    @property
    def response_data(self):
        return self.__response_data
    
    
    def encode(self) -> bytes:
        data_as_dict = data_as_dict = {
            "response_code": self.response_code,
            "response_data": self.response_data
        }
        return json.dumps(data_as_dict).encode()
    

class ErrorResponse(Response):
    def __init__(self, error_data:str = "") -> None:
        super().__init__(ResponseCodes.ERROR, {"Error": error_data})


class AcknowledgeResponse(Response):
    def __init__(self, response_data: dict = {}) -> None:
        super().__init__(ResponseCodes.ACKNOWLEDGE, response_data)