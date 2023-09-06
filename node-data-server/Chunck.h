#pragma once
#include <string>
#include <vector>

typedef struct ChunckData 
{
	std::vector<unsigned char> data;
} ChunckData;


class Chunck {
public:
	Chunck(std::string id, ChunckData data);
	~Chunck();

	const std::string& getChunckId() const;
	const ChunckData getData() const;

private:
	std::string _chunckId;
	ChunckData _data;
};