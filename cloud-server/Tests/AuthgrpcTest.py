import grpc
import cloud_pb2
import cloud_pb2_grpc


def main():
    channel = grpc.insecure_channel('localhost:5000')
    stub = cloud_pb2_grpc.CloudStub(channel)

    try:        
        #request = cloud_pb2.signupRequest(username="test1", password="test1password", email="test1@.gmailcom", phoneNumber="1")
        #response = stub.signup(request)
        #print(response)
        print("Login:")
        response = stub.login(cloud_pb2.loginRequest(username="test1", password="test1password"))
        sessionId = response.sessionId
        print(f"The session id is:{sessionId}")
        print("Try to login to connected user")
        response = stub.login(cloud_pb2.loginRequest(username="test1", password="test1password"))
        print(response)
        stub.logout(cloud_pb2.logoutRequest(sessionId=sessionId))
        print("logout successfully")
        
    
    except grpc.RpcError as e:
        print("Error")




if __name__ == "__main__":
    main()