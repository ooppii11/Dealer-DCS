#include "IRequest.h"

IRequest::IRequest(int requestCode): _requestCode(requestCode) {}

IRequest::~IRequest()
{
}

int IRequest::getRequestCode() const
{
	return this->_requestCode;
}
