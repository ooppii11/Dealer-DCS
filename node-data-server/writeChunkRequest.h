#pragma once
#include <string>
#include "IRequest.h"
#include "Chunck.h"


#define WRITE_CHUNCK_REQUEST_CODE 0


class writeChunkRequest : public IRequest
{
public:
	writeChunkRequest(const std::string& chunckId, ChunckData data);
	~writeChunkRequest();

	ChunckData getChunckData() const;
	const std::string& getChunckId() const;

private:
	std::string _chunckId;
	ChunckData _chunkData;
};