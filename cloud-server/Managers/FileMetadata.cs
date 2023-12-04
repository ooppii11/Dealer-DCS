namespace cloud_server.Managers
{
    public class FileMetadata
    {
        private string _id;
        private string _creatorId;
        private string _name;
        private string _type;
        private string _size;
        private string _creationDate;
        private string _lastModified;
        public FileMetadata(string creatorId, string name, string type, string size, string creationDate="", string lastModified="")
        {
            this._creatorId = creatorId;
            this._name = name;
            this._type = type;
            this._size = size;
            this._creationDate = creationDate;
            this._lastModified = lastModified;
        }

        public string Id
        {
            get { return this._id; }
        }

        public string Name
        {
            get { return this._name; }
        }

        public string Type
        {
            get { return this._type; }
        }

        public string Size
        {
            get { return this._size; }
        }

        public string CreationDate
        {
            get { return this._creationDate; }
        }
        public string LastModified
        {
            get { return this._lastModified; }
        }

        public string CreatorId
        {
            get { return this._creatorId; }
        }

    }
}
