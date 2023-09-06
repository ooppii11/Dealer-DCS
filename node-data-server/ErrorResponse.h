#pragma once
#include "IResponse.h"
#include <string>


#define ERROR_RESPONSE_CODE 0

class ErrorResponse : public IResponse
{
public:
	ErrorResponse(std::string errorMessage = "");
	~ErrorResponse();

	const std::string& getError() const;
private:
	std::string _errorMessage;
};