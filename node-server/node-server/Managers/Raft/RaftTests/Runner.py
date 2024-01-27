import os

def open_terminal_and_run_exe(exe_path):
    os.system(f'start cmd /k "{exe_path}"')

def main():
    #update the path
    base_path = r'C:\src\cloud-project\DEALER_DCS\node-server\node-server\bin\Debug\net7.0\node-server.exe'
    
    # Extract the directory part of the path
    directory = os.path.dirname(base_path)

    # Set the working directory
    os.chdir(directory)

    ports = ["1111", "2222", "3333"]

    for port in ports:
        full_path = f'"{base_path}" {port}'
        print(full_path)
        open_terminal_and_run_exe(full_path)

if __name__ == "__main__":
    main()
