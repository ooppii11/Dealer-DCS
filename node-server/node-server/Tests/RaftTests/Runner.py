import os
import time
def open_terminal_and_run_exe(exe_path):
    os.system(f'start cmd /k "{exe_path}"')

def main():
    #update the path
    base_path = r'C:\src\cloud-project\DEALER_DCS\node-server\node-server\bin\Debug\net7.0\node-server.exe'
    
    # Extract the directory part of the path
    directory = os.path.dirname(base_path)

    # Set the working directory
    os.chdir(directory)

    ports = ["1111 1.log", "2222 2.log", "3333 3.log"]
    #ports = ["2222 2.log", "3333 3.log"]

    for port in ports:
        full_path = f'"{base_path}" {port}'
        print(full_path)
        time.sleep(0.2)
        open_terminal_and_run_exe(full_path)

if __name__ == "__main__":
    main()
