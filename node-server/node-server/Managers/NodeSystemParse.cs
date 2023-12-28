using System.Threading.Tasks;
using System.IO;
using Grpc.Core;
using System;
using Microsoft.VisualBasic.FileIO;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;


namespace NodeServer.Managers
{
    public class NodeSystemParse
    {
        private const int _fileSize = 50; //MB
        private const int _systemSize = 1000; //MB -> 1GB
        private const string _fileName = @"Managers\System.csv";
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

        public void addFile(string fileID, List<string> locations)
        {
            this._locations[fileID] = locations;
            this._numOfFilesInSystem++;
            TextWriter tsw = new StreamWriter(NodeSystemParse._fileName, true);
            tsw.WriteLine(fileID + "," + String.Join(",", locations.ToArray()));
            tsw.Close();
        }

        public void removeFile(string fileID)
        {
            this._locations.Remove(fileID);
            this._numOfFilesInSystem--;
            File.WriteAllText(NodeSystemParse._fileName, "");
            foreach (KeyValuePair<string, List<string>> entry in this._locations)
            {
                TextWriter tsw = new StreamWriter(NodeSystemParse._fileName, true);
                tsw.WriteLine(entry.Key + "," + String.Join(",", entry.Value.ToArray()));
                tsw.Close();
            }
        }
        private void parseWhere()
        {
            this._locations = new Dictionary<string, List<string>>();
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

        public bool filExists(string fileID)
        {
            return this._locations.ContainsKey(fileID);
        }
    }
}
