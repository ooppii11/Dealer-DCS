using System.Threading.Tasks;
using System.IO;
using Grpc.Core;
using System;
using Microsoft.VisualBasic.FileIO;


namespace NodeServer.Managers
{
    public class NodeSystemParse
    {
        private const int _fileSize = 50; //MB
        private const int _systemSize = 1000; //MB -> 1GB
        private const string _fileName = "Managers/System.csv";
        private const string subPath = "Managers";

        private int _numOfFilesInSystem = 0;
        private Dictionary<string, List<string>> _locations;


        public NodeSystemParse() 
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, subPath)))
            {
                Directory.CreateDirectory(subPath);
                File.Create(NodeSystemParse._fileName).Close();
                _locations = new Dictionary<string, List<string>>();
            }
            else if (!File.Exists(NodeSystemParse._fileName))
            {
                File.Create(NodeSystemParse._fileName).Close();
                _locations = new Dictionary<string, List<string>>();

            }
            else
            {
                this.parseWhere();
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

        public void addFile(string fileId, List<string> locations)
        {
            this._locations[fileId] = locations;
            this._numOfFilesInSystem++;
            File.WriteAllText(NodeSystemParse._fileName, fileId.ToString() + ",");
            File.WriteAllText(NodeSystemParse._fileName, String.Join(",", locations.ToArray()));
            File.WriteAllText(NodeSystemParse._fileName, "\n");
        }

        public void removeFile(string fileId)
        {
            this._locations.Remove(fileId);
            this._numOfFilesInSystem--;
            foreach (KeyValuePair<string, List<string>> entry in this._locations)
            {
                File.WriteAllText(NodeSystemParse._fileName, entry.Key.ToString() + ",");
                File.WriteAllText(NodeSystemParse._fileName, String.Join(",", entry.Value.ToArray()));
                File.WriteAllText(NodeSystemParse._fileName, "\n");
            }
        }
        private void parseWhere()
        {
            using (TextFieldParser parser = new TextFieldParser(NodeSystemParse._fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {

                    List<string> fields = parser.ReadFields().ToList();
                    string id = fields[0];
                    fields.RemoveAt(0);
                    this._numOfFilesInSystem++;
                    this._locations[id] = fields;

                }

            }
        }

        public bool filExists(string fileId)
        {
            return this._locations.ContainsKey(fileId);
        }
    }
}
