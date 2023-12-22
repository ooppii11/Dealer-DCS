import grpc
import cloud_pb2
import cloud_pb2_grpc


def fils_test(stub):
    FILENAME = "test24"
    try:
        response = stub.login(cloud_pb2.LoginRequest(username="test1", password="test1password"))
        print(response)
        sessionId = response.sessionId

        request = cloud_pb2.UploadFileRequest(sessionId=sessionId, fileName=FILENAME, type="plain/text", fileData=b"upload test")
        response = stub.UploadFile(iter([request]))
        print("Upload:")
        print(response)

        request = cloud_pb2.GetListOfFilesRequest(sessionId=sessionId)
        response = stub.getListOfFiles(request)
        print("Files:")
        print(response)

        request = cloud_pb2.GetFileMetadataRequest(sessionId=sessionId, fileName=FILENAME)
        response = stub.getFileMetadata(request)
        print("File:")
        print(response)


        request = cloud_pb2.DownloadFileRequest(sessionId=sessionId, fileName=FILENAME)
        download_file_reply = stub.DownloadFile(request)
        print("Download File:")
        for response in download_file_reply:
            print(response.fileData.decode())



        request = cloud_pb2.DeleteFileRequest(sessionId=sessionId, fileName=FILENAME)
        response = stub.DeleteFile(request)
        print("Delete:")
        print("Success" if response.status == 0 else "Failure")
        stub.logout(cloud_pb2.LogoutRequest(sessionId=sessionId))
        print("logout successfully")
        return 0
    except:
        return 1


def login_test(stub):
    try:
        request = cloud_pb2.SignupRequest(username="test8", password="test8password", email="test4@gmail.com", phoneNumber="1")
        response = stub.signup(request)
        print(response)
        print("Login:")
        response = stub.login(cloud_pb2.LoginRequest(username="test1", password="test1password"))
        sessionId = response.sessionId
        print(f"The session id is:{sessionId}")
        print("Try to login to connected user")
        response = stub.login(cloud_pb2.LoginRequest(username="test1", password="test1password"))
        print(response)
        stub.logout(cloud_pb2.LogoutRequest(sessionId=sessionId))
        print("logout successfully")
        return 0

    except Exception as e:
        return 1


def main():
    channel = grpc.insecure_channel('localhost:50053')
    stub = cloud_pb2_grpc.CloudStub(channel)

    if fils_test(stub) == 1: #  or login_test(stub) == 1 
        print("Test Faild")



if __name__ == "__main__":
    main()