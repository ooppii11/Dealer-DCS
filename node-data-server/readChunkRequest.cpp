#include "readChunkRequest.h"

readChunkRequest::readChunkRequest(const std::string& chunckId): 
	IRequest(READ_CHUNCK_REQUEST_CODE), _chunckId(chunckId) {}

readChunkRequest::~readChunkRequest() {}

const std::string& readChunkRequest::getChunckId() const
{
	return this->_chunckId;
}