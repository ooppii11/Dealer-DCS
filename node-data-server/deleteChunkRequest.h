#pragma once
#include <string>
#include "IRequest.h"


#define DELETE_CHUNCK_REQUEST_CODE 0



class deleteChunkRequest : public IRequest
{
public:
	deleteChunkRequest(const std::string& chunckId);
	~deleteChunkRequest();


	const std::string& getChunckId() const;

private:
	std::string _chunckId;
};