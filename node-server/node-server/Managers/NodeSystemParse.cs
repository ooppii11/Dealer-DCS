using System.Threading.Tasks;
using System.IO;
using Grpc.Core;
using System;


namespace NodeServer.Managers
{
    public class NodeSystemParse
    {
        private const int _fileSize = 50; //MB
        private const int _systemSize = 1000; //MB -> 1GB
        private int _numOfFilesInSystem = 0;
        private const string _fileName = "Managers/numOfFilesInTheSysetm.txt";
        private const string subPath = "Managers";

        
        public NodeSystemParse() 
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, subPath)))
            {
                Directory.CreateDirectory(subPath);
                File.Create(NodeSystemParse._fileName).Close();
                File.WriteAllText(NodeSystemParse._fileName, "0");
            }
            else if (!File.Exists(NodeSystemParse._fileName))
            {
                File.Create(NodeSystemParse._fileName).Close();
                File.WriteAllText(NodeSystemParse._fileName, "0");

            }
            else
            {
                string data = File.ReadAllText(NodeSystemParse._fileName);
                Int32.TryParse(data, out this._numOfFilesInSystem);
            }
            
        }

        public int GetNumOfFiles()
        {
            return this._numOfFilesInSystem;
        }

        public void SetNumOfFiles(int numOfFiles)
        {
            if (numOfFiles * NodeSystemParse._fileSize < NodeSystemParse._systemSize)
            {
                this._numOfFilesInSystem = numOfFiles;
                File.WriteAllText(NodeSystemParse._fileName, numOfFiles.ToString());
            }
            else
            {
                throw new Exception($"Can't set the num of files to more then {NodeSystemParse._systemSize/ NodeSystemParse._fileSize}.");
            }
        }

        public bool canAddFile()
        {
            return this._numOfFilesInSystem * NodeSystemParse._fileSize < NodeSystemParse._systemSize;
        }

        public void addFile()
        {
            if (this.canAddFile())
            {
                this._numOfFilesInSystem += 1;
                File.WriteAllText(NodeSystemParse._fileName, this._numOfFilesInSystem.ToString());
            }
            else 
            {
                throw new Exception("System is full, can't add another file.");
            }
            
        }

        public void removeFile()
        {
            if (this._numOfFilesInSystem > 0)
            {
                this._numOfFilesInSystem -= 1;
                File.WriteAllText(NodeSystemParse._fileName, this._numOfFilesInSystem.ToString());
            }
            else 
            {
                throw new Exception("Can't remove a file from the system, there are 0 files in the system.");
            }
        }
    }
}
