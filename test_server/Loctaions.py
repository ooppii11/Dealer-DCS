class Loctaions:
    def __init__(self, first_loction, second_loction, third_location) -> None:
        self.__first_location = first_loction
        self.__second_location = second_loction 
        self.__third_location = third_location
    

    @property
    def first_location(self):
        return self.__first_location
    

    @property
    def second_location(self):
        return self.__second_location
    

    @property
    def third_location(self):
        return self.__third_location
