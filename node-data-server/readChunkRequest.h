#pragma once
#include <string>
#include "IRequest.h"


#define READ_CHUNCK_REQUEST_CODE 0


class readChunkRequest : public IRequest
{
public:
	readChunkRequest(const std::string& chunckId);
	~readChunkRequest();

	const std::string& getChunckId() const;

private:
	std::string _chunckId;
};