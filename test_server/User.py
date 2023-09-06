from Loctaions import Loctaions


class User:
    def __init__(self, username:str, password:str, locations:Loctaions) -> None:
        self.__username = username
        self.__password = password
        self.__locations = locations

    
    @property
    def username(self):
        return self.__username
    

    @property
    def password(self):
        return self.__password
    

    @property
    def locations(self):
        return self.__locations