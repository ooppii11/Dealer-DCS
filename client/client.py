from option_actions import *


def get_session_id(stub):
    while True:
        try:
            session_id = AuthActions.get_user_credentials(stub)
            if session_id is not None:
                return session_id
        except Exception as e:
            print("Error:", e)    

def main():
    channel = grpc.insecure_channel('localhost:50053')
    stub = cloud_pb2_grpc.CloudStub(channel)
    session_id = get_session_id(stub)


    options = {
        "upload": FilesActions.upload,
        "delete": FilesActions.delete,
        "download": FilesActions.download,
        "ls": FilesActions.ls,
        "file metadata": FilesActions.file_metadata
    }

    while True:
        user_input  = input(">> ")
        if "logout" in user_input.lower():
            AuthActions.logout(stub, session_id)
            exit()
        user_option, *user_args = user_input.split()
        print((stub, session_id,*user_args))
        options[user_option](stub, session_id, *user_args)
    
   
if __name__ == "__main__":
    main()