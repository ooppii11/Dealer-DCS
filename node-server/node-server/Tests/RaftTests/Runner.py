import os
import time
def open_terminal_and_run_exe(exe_path):
    os.system(f'start cmd /k "{exe_path}"')

def main():
    #update the path
    #base_path = r'C:\Users\test0\OneDrive\שולחן העבודה\cloud storage\DEALER_DCS\node-server\node-server\bin\Debug\net7.0\node-server.exe'
    base_path = r'C:\src\Dealer-DCS\node-server\node-server\bin\Debug\net7.0\node-server.exe'
    
    # Extract the directory part of the path
    directory = os.path.dirname(base_path)

    # Set the working directory
    os.chdir(directory)

    configurations = [
        {"port": "1111", "log": "1.log"},
        {"port": "2222", "log": "2.log"},
        {"port": "3333", "log": "3.log"}
    ]

    for config in configurations:
        full_path = f'"{base_path}" {config["port"]} {config["log"]}'
        print(full_path)
        time.sleep(0.2)
        open_terminal_and_run_exe(full_path)

if __name__ == "__main__":
    main()

