import socket
import json

HOST = '127.0.0.1' 
PORT  = 55555
AUTHENTICATE_USER_REQUEST = {
            "request_code": 200,
            "request_data": {
                "username": "test1",
                "password" : "test1_password"
            }
        }
ADD_USER_REQUEST = {
            "request_code": 200,
            "request_data": {
                "username": "test1",
                "password" : "test1_password",
                "first_location": "first_location",
                "second_location": "second_location",
                "third_location": "third_location"
            }
        }


def test_server(request):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client_socket:
        try:
            client_socket.connect((HOST, PORT))
            client_socket.send(json.dumps(request).encode())
            response = client_socket.recv(1024).decode()
            print("Server response:", response)
        except ConnectionRefusedError:
            print("Connection to the server was refused. Make sure the server is running.")
        finally:
            client_socket.close()


def main():
    test_server(ADD_USER_REQUEST)
    test_server(AUTHENTICATE_USER_REQUEST)


if __name__ == "__main__":
    main()
