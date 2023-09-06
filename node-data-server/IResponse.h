#pragma once


class IResponse
{
public:
	IResponse(int responseCode);
	~IResponse();

	int getResponseCode() const;

protected:
	int _responseCode;
};