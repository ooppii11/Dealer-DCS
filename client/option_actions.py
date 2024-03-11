import grpc
from grpc._channel import _InactiveRpcError
import cloud_pb2
import cloud_pb2_grpc
from grpc import StatusCode

class AuthActions():
    @staticmethod
    def get_user_credentials(grpc_stub):
        print("Choose an option:")
        print("1. Login")
        print("2. Signup")
        choice = input("Enter your choice (1 or 2): ")

        if choice == '1':
            action = 'login'
        elif choice == '2':
            action = 'signup'
        else:
            print("Invalid choice. Please enter 1 for Login or 2 for Signup.")
            return None

        username = input("Enter your username: ")
        password = input("Enter your password: ")

        if action == 'signup':
            email = input("Enter your email address: ")
            phone_number = input("Enter your phone number: ")
            return AuthActions.signup(grpc_stub, username, password, email, phone_number)
        return AuthActions.login(grpc_stub, username, password)
    

    @staticmethod
    def login(grpc_stub, username, password):
        try:
            response = grpc_stub.login(cloud_pb2.LoginRequest(username=username, password=password))
            sessionId = response.sessionId
            return sessionId
        
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")
            else:
                raise Exception("An unexpected RpcError occurred.")



    @staticmethod
    def signup(grpc_stub, username, password, email, phone_number):
        try:
            request = cloud_pb2.SignupRequest(username=username, password=password, email=email, phoneNumber=phone_number)
            response = grpc_stub.signup(request)
        
            response = grpc_stub.login(cloud_pb2.LoginRequest(username=username, password=password))
            sessionId = response.sessionId
            return sessionId
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")
            else:
                raise Exception("An unexpected RpcError occurred.")

