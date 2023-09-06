#include "Chunck.h"

Chunck::Chunck(std::string id, ChunckData data) : _chunckId(id), _data(data) {}

Chunck::~Chunck() {}

const std::string& Chunck::getChunckId() const
{
	return this->_chunckId;
}

const ChunckData Chunck::getData() const
{
	return this->_data;
}
