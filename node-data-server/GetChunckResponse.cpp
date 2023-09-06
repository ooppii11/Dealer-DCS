#include "GetChunckResponse.h"

GetChunckResponse::GetChunckResponse(Chunck chunck): IResponse(GET_CHUNCK_RESPONSE_CODE), _chunck(chunck) {}

GetChunckResponse::~GetChunckResponse() {}

const Chunck& GetChunckResponse::getChunck()
{
	return this->_chunck;
}
