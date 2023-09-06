#pragma once
#include "IResponse.h"


#define ACKNOWLEDGMENT_RESPONSE_CODE 0


class ACKResponse: public IResponse
{
	ACKResponse();
	~ACKResponse();
};