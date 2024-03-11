from multiprocessing import Pool
from option_actions import *


def get_session_id(stub):
    while True:
        try:
            session_id = AuthActions.get_user_credentials(stub)
            if session_id is not None:
                break  
        except Exception as e:
            print("Error:", e)    

if __name__ == "__main__":
    channel = grpc.insecure_channel('localhost:50053')
    stub = cloud_pb2_grpc.CloudStub(channel)
    session_id = get_session_id(stub)
        
    
    #options_functions = {
    #    'Upload': option1,
    #    'Update': option2
    #}

    while True:
        print("Choose an option:")
        print("1. Option 1")
        print("2. Option 2")
        print("Enter 'exit' to quit")
        option_choice = input("Enter your choice: ")

        if option_choice == "exit":
            break

     #   selected_function = options_functions.get(option_choice)
     #   if selected_function:
     #       args = (session_id, arg1_value, arg2_value)  # Additional arguments for the selected function
     #       with Pool() as pool:
     #           pool.apply(selected_function, args)
     #   else:
     #       print("Invalid option choice")
