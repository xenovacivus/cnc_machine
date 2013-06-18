

#include "Serial.h"

// Set to 1 when data is available
volatile char serialPacketReady = 0;

// For sending and receiving serial packets
SerialData ser;

void SerialInit(void)
{
	SerialDataInitialize(&ser);
	
	// Setup function pointers
    ser.TransmitPacketComplete = &TransmitPacketComplete;
    ser.Transmit = &Transmit;
    ser.ReceivePacketComplete = &ReceivePacketComplete;
    ser.ReceiveDataError = &ReceiveDataError;

	USART_Init();
}

void USART_Init(void)
{
	// USART Configuration
	PORTC.DIRSET = PIN3_bm;
	PORTC.DIRCLR = PIN2_bm;
	
	// RX Interrupt Level
	char rxd_int_level = USART_RXCINTLVL_LO_gc;
	

	USARTC0.CTRLA = (USARTC0.CTRLA & ~USART_RXCINTLVL_gm) | rxd_int_level;
	
	// Format Set: 8 data bits, no parity, 1 stop bit
	char two_stop_bits = 0;
	char char_size = USART_CHSIZE_8BIT_gc;
	char parity_mode = USART_PMODE_DISABLED_gc;
	USARTC0.CTRLC = (char_size | parity_mode | (two_stop_bits ? USART_SBMODE_bm : 0));
	
	// Baud Rate Setup
	// Baudrate select = (1/16) * (((I/O clock frequency)/Baudrate) -1)

	// BSEL = fper / (16 * BaudRate) - 1
	//                 = 12
	// 
	uint8_t bsel_value = BSEL;
	uint8_t bscale_factor = 0;
	USARTC0.BAUDCTRLA = (uint8_t) bsel_value;
	USARTC0.BAUDCTRLB = (bscale_factor << USART_BSCALE0_bp) | (bsel_value >> 8);
	
	// USART RX enable
	USARTC0.CTRLB |= (USART_RXEN_bm | USART_TXEN_bm);
}


SerialData * SerialDataTransmitStruct (void)
{
	if (SerialTransferInProgress (&ser))
	{
		return 0;
	}
	else
	{
		return &ser;
	}
}

SerialData * SerialDataAvailable (void)
{
	if (serialPacketReady != 0)
	{
		serialPacketReady = 0;
		return &ser;
	}
	else
	{
		return 0;
	}
}

void Transmit(byte data)
{
	//if (RS485_IS_INPUT())
	//{
		// Disable Receiver
	//	UCSR0B &= ~(1<<RXEN0);
//		RS485_SET_OUTPUT();
	//}
	USART_Transmit (data);
}

void TransmitPacketComplete (void)
{
	/* Disable DRE interrupts. */
	uint8_t tempCTRLA = USARTC0.CTRLA;
	tempCTRLA = (tempCTRLA & ~USART_DREINTLVL_gm) | USART_DREINTLVL_OFF_gc;
	USARTC0.CTRLA = tempCTRLA;
	
	
	// Enable receiver
	//UCSR0B  |= (1<<RXEN0);
	//RS485_SET_INPUT();
}

void ReceivePacketComplete (volatile SerialData * s)
{
	// Check if the address matches
	if (s->receive_address == ADDRESS)
	{
		serialPacketReady = 1;
	}
}

void ReceiveDataError (byte errCode)
{
}

void USART_Transmit( unsigned char data )
{
	// Wait for empty transmit buffer
	while((USARTC0.STATUS & USART_DREIF_bm) == 0) {} 

	USARTC0.DATA = data;
		
	/* Enable DRE interrupt if not already enabled. */
	uint8_t tempCTRLA = USARTC0.CTRLA;
	if (!(tempCTRLA & USART_TXCINTLVL_LO_gc))
	{
		tempCTRLA = (tempCTRLA & ~USART_DREINTLVL_gm) | USART_DREINTLVL_LO_gc;
		USARTC0.CTRLA = tempCTRLA;
	}
	
	
}

// USART Receive Interrupt
ISR (USARTC0_RXC_vect)
{
	//uint8_t data = UDR0;
	uint8_t data = USARTC0.DATA;
	ProcessDataChar (&ser, data); // 6-12 uS
}

// USART Transmit Complete (Data Register Empty)
ISR (USARTC0_DRE_vect) 
{
	// Disable the DRE interrupt
    //USART_DataRegEmpty( &USART_data );
	//uint8_t d = USARTC0.DATA;
	// Send out the next byte
	SerialByteTransmitComplete (&ser); // 6 uS
}