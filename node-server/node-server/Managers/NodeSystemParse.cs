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
                File.Create(_fileName).Close();
                _locations = new Dictionary<string, List<string>>();
            }
            else if (!File.Exists(_fileName))
            {
                File.Create(_fileName).Close();
                _locations = new Dictionary<string, List<string>>();

            }
            else
            {
                parseWhere();
            }

        }

        public int GetNumOfFiles()
        {
            return _numOfFilesInSystem;
        }

        public void SetNumOfFiles(int numOfFiles)
        {
            if (numOfFiles * _fileSize < _systemSize)
            {
                _numOfFilesInSystem = numOfFiles;
                File.WriteAllText(_fileName, numOfFiles.ToString());
            }
            else
            {
                throw new Exception($"Can't set the num of files to more then {_systemSize / _fileSize}.");
            }
        }

        public bool canAddFile()
        {
            return _numOfFilesInSystem * _fileSize < _systemSize;
        }

        public void addFile(string fileID, List<string> locations)
        {
            _locations[fileID] = locations;
            _numOfFilesInSystem++;
            TextWriter tsw = new StreamWriter(_fileName, true);
            tsw.WriteLine(fileID + "," + string.Join(",", locations.ToArray()));
            tsw.Close();
        }

        public void removeFile(string fileID)
        {
            _locations.Remove(fileID);
            _numOfFilesInSystem--;
            File.WriteAllText(_fileName, "");
            foreach (KeyValuePair<string, List<string>> entry in _locations)
            {
                TextWriter tsw = new StreamWriter(_fileName, true);
                tsw.WriteLine(entry.Key + "," + string.Join(",", entry.Value.ToArray()));
                tsw.Close();
            }
        }
        private void parseWhere()
        {
            _locations = new Dictionary<string, List<string>>();
            using (TextFieldParser parser = new TextFieldParser(_fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {

                    List<string> fields = parser.ReadFields().ToList();
                    string id = fields[0];
                    fields.RemoveAt(0);
                    _numOfFilesInSystem++;
                    _locations[id] = fields;

                }

            }
        }

        public bool filExists(string fileID)
        {
            return _locations.ContainsKey(fileID);
        }
    }
}
