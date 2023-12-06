namespace cloud_server.Managers
{
    public class FileMetadata
    {
        private string _id;
        private int _creatorId;
        private string _name;
        private string _type;
        private int _size;
        private DateTime _creationDate;
        private DateTime _lastModified;
        public FileMetadata(int creatorId,
                            string name,
                            string type,
                            int size,
                            DateTime creationDate = default(DateTime),
                            DateTime lastModified = default(DateTime))
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

        public int Size
        {
            get { return this._size; }
        }

        public DateTime CreationDate
        {
            get { return this._creationDate; }
        }
        public DateTime LastModified
        {
            get { return this._lastModified; }
        }

        public int CreatorId
        {
            get { return this._creatorId; }
        }

    }
}
