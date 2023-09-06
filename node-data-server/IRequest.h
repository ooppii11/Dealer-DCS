#pragma once


class IRequest
{
public:
	IRequest(int requestCode);
	~IRequest();

	int getRequestCode() const;

protected:
	int _requestCode;
};