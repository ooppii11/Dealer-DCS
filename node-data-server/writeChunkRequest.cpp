#include "writeChunkRequest.h"

writeChunkRequest::writeChunkRequest(const std::string& chunckId, ChunckData data): 
    IRequest(WRITE_CHUNCK_REQUEST_CODE), _chunckId(chunckId), _chunkData(data) {}

writeChunkRequest::~writeChunkRequest() {}

ChunckData writeChunkRequest::getChunckData() const
{
    return this->_chunkData;
}

const std::string& writeChunkRequest::getChunckId() const
{
    return this->_chunckId;
}
