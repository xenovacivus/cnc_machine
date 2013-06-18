

#ifndef SERIAL_H
#define SERIAL_H

#include <avr/io.h>
#include <avr/interrupt.h>
#include "SerialProtocol.h"

#define ADDRESS 0x21


#ifndef F_CPU
/* Don't allow compiling if F_CPU is not defined */
#error "F_CPU not defined for serial.h"
#endif

#define BAUD_RATE 9600
#define BSEL (((F_CPU/16)/BAUD_RATE) - 1)

// Public Functions //
void SerialInit(void);

SerialData * SerialDataAvailable (void);
SerialData * SerialDataTransmitStruct (void);

// Private Functions //

void USART_Init(void);
void TransmitPacketComplete (void);
void Transmit(byte data);
void ReceivePacketComplete (volatile SerialData * s);
void ReceiveDataError (byte errCode);
void USART_Transmit(unsigned char data);


#endif
