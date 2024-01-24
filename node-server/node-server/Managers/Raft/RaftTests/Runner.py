import subprocess
import time

def open_terminal_and_run_exe(exe_path):
    subprocess.run(f'start cmd /k "{exe_path}"', shell=True)

def main():
    base_path = r'C:\Users\test0\OneDrive\שולחן העבודה\cloud storage\DEALER_DCS\node-server\node-server\bin\Debug\net7.0\node-server.exe'
    ports = ["1111", "2222", "3333"]

    for port in ports:
        full_path = f'"{base_path}" {port}'
        open_terminal_and_run_exe(full_path)
        time.sleep(1)  # Optional: add a delay between opening terminals

if __name__ == "__main__":
    main()
