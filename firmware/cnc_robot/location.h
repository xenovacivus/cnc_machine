/*
 * location.h
 *
 * Created: 7/1/2013 9:59:40 PM
 *  Author: xeno
 */ 


#ifndef LOCATION_H_
#define LOCATION_H_


typedef struct
{
	uint16_t time;
	int32_t x;
	int32_t y;
	int32_t z;
} Location_t;


#endif /* LOCATION_H_ */