import grpc
from grpc._channel import _InactiveRpcError
import cloud_pb2
import cloud_pb2_grpc
from grpc import StatusCode

class AuthActions():
    @staticmethod
    async def get_user_credentials(grpc_stub):
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
            return await AuthActions.signup(grpc_stub, username, password, email, phone_number)
        return await AuthActions.login(grpc_stub, username, password)
    
    @staticmethod
    def logout(grpc_stub, session_id):
        try:
            request = cloud_pb2.LogoutRequest(sessionId=session_id)
            grpc_stub.logout(request)
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")

    @staticmethod
    async def login(grpc_stub, username, password):
        try:
            response = grpc_stub.login(cloud_pb2.LoginRequest(username=username, password=password))
            session_id = response.sessionId
            return session_id
        
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")


    @staticmethod
    async def signup(grpc_stub, username, password, email, phone_number):
        try:
            request = cloud_pb2.SignupRequest(username=username, password=password, email=email, phoneNumber=phone_number)
            response = grpc_stub.signup(request)
        
            response = grpc_stub.login(cloud_pb2.LoginRequest(username=username, password=password))
            session_id = response.sessionId
            return session_id
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")


    # add Logout

class FilesActions():
    @staticmethod
    async def upload(grpc_stub, session_id, filename,file_path): 
        file_data = None
        try:
            with open(file_path, 'rb') as file:
                file_data = file.read()
        except:
            raise Exception("File not exists")
        
        try:
            request = cloud_pb2.UploadFileRequest(sessionId=session_id, fileName=filename, type="plain/text", fileData=file_data)
            response = grpc_stub.UploadFile(iter([request]))
            print("Upload:")
            print(response)
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")
    
    @staticmethod
    async def download(grpc_stub, session_id, filename, output_path):
        try:
            request = cloud_pb2.DownloadFileRequest(sessionId=session_id, fileName=filename)
            download_file_reply = grpc_stub.DownloadFile(request)
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")
            
        file_data = None
        for response in download_file_reply:
            file_data += response.fileData.decode()
        with open(output_path, 'wb') as file:
             file.write(file_data)

    @staticmethod
    async def update(grpc_stub, session_id, filename, file_path):
        file_data = None
        try:
            with open(file_path, 'rb') as file:
                file_data = file.read()
        except:
            raise Exception("File not exists")
        try:
            request = cloud_pb2.UpdateFileRequest(sessionId=session_id, fileName=filename, fileData=file_data)
            update_file_response = grpc_stub.UpdateFile(iter([request]))
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")
            
    @staticmethod
    async def delete(grpc_stub, session_id, filename):
        try:
            request = cloud_pb2.DeleteFileRequest(sessionId=session_id, fileName=filename)
            response = grpc_stub.DeleteFile(request) 
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")  
            
    async def ls(grpc_stub, session_id):
        try:
            request = cloud_pb2.GetListOfFilesRequest(sessionId=session_id)
            response = grpc_stub.getListOfFiles(request)
            print("Files:")
            print(response)
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")  

    async def file_metadata(grpc_stub, session_id, filename):
        try:
            request = cloud_pb2.GetFileMetadataRequest(sessionId=session_id, fileName=filename)
            response = grpc_stub.getFileMetadata(request)
            print(response)
        except grpc.RpcError as e:
            if e.code() == StatusCode.INTERNAL:
                    raise Exception(e.details())
            elif e.code() == StatusCode.UNAVAILABLE:
                    raise Exception("Connection refused error occurred.")  

