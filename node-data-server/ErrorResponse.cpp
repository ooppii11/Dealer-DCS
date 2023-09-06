#include "ErrorResponse.h"

ErrorResponse::ErrorResponse(std::string errorMessage):
	IResponse(ERROR_RESPONSE_CODE), _errorMessage(errorMessage) {}

ErrorResponse::~ErrorResponse() {}

const std::string& ErrorResponse::getError() const
{
	return this->_errorMessage;
}
