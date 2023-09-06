import sqlite3
from User import User
from Loctaions import Loctaions


class DBHandler:
    def __init__(self, db_path="test.db") -> None:
        self.__db_path = db_path 
        self.create_tables()



    def create_tables(self, file_path="tables.sql") -> None:
        tables = ""
        with open(file_path, 'r', encoding='utf-8') as f:
            tables = f.read()

        with sqlite3.connect(self.__db_path) as conn:
            cursor = conn.cursor()
            cursor.executescript(tables)


    def add_user(self, user:User) -> None:
        command = f"""
        INSERT INTO USERS VALUES(
            '{user.username}',
            '{user.password}',
            '{user.locations.first_location}',
            '{user.locations.second_location}',
            '{user.locations.third_location}'
            );
        """
        with sqlite3.connect(self.__db_path) as connction:
            cursor = connction.cursor()
            try:    
                cursor.execute(command)
                connction.commit()
            except sqlite3.IntegrityError as e:
                raise Exception("Username already taken")


    def authenticate_user(self, username:str, password:str) -> User:
        command = f"""
        SELECT first_location, second_location, third_location
        FROM USERS
        WHERE username == '{username}' AND password == '{password}';
        """

        with sqlite3.connect(self.__db_path) as connction:
            cursor = connction.cursor()
            cursor.execute(command)
            data = cursor.fetchall()

            if data == []:
                raise Exception("Incorrect username or password")
            return User(username, password, Loctaions(data[0][0], data[0][1], data[0][2]))

