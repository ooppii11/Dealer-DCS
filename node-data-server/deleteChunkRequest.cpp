#include "deleteChunkRequest.h"

deleteChunkRequest::deleteChunkRequest(const std::string& chunckId): IRequest(DELETE_CHUNCK_REQUEST_CODE), _chunckId(chunckId) {}

deleteChunkRequest::~deleteChunkRequest()
{
}

const std::string& deleteChunkRequest::getChunckId() const
{
    return this->_chunckId;
}
