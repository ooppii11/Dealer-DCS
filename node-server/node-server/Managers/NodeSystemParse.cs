﻿using System.Threading.Tasks;
using System.IO;
namespace node_server.Managers
{
    public class NodeSystemParse
    {
        private const int _fileSize = 50; //MB
        private const int _systemSize = 1000; //MB -> 1GB
        private int _numOfFilesInSystem = 0;
        private const string _fileName = "numOfFilesInTheSysetm.txt";

        
        public NodeSystemParse() 
        {
            string data = File.ReadAllText(NodeSystemParse._fileName);
            Int32.TryParse(data, out this._numOfFilesInSystem);
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