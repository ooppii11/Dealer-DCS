#include "IResponse.h"

IResponse::IResponse(int responseCode): _responseCode(responseCode) {}

IResponse::~IResponse() {}

int IResponse::getResponseCode() const
{
    return this->_responseCode;
}
