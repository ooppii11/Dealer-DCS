#pragma once
#include "IResponse.h"
#include "Chunck.h"


#define GET_CHUNCK_RESPONSE_CODE 0

class GetChunckResponse : public IResponse
{
public:
	GetChunckResponse(Chunck chunck);
	~GetChunckResponse();

	const Chunck& getChunck();

private:
	Chunck _chunck;
};