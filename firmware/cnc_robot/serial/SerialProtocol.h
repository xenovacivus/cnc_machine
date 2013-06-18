
#ifndef _SERIAL_PROTOCOL_H
#define _SERIAL_PROTOCOL_H

//#include <stdio.h>
#include <stdbool.h>


/* Control Characters and their Escaped Equivalents */
#define START_BYTE 0xCA
#define ESCAPE_CHAR '\\'
#define NULL_BYTE 0
#define MAX_BYTE 255
#define START_BYTE_ESCAPED 2
#define ESCAPE_CHAR_ESCAPED 3
#define NULL_BYTE_ESCAPED 4
#define MAX_BYTE_ESCAPED 5

/* Receive State Machine States */
#define PROC_STATE_AWAITING_START_BYTE 0
#define PROC_STATE_AWAITING_ADDRESS 1
#define PROC_STATE_AWAITING_LENGTH 2
#define PROC_STATE_AWAITING_DATA 3
#define PROC_STATE_AWAITING_CHECKSUM 4

/* Transmit State Machine States */
#define PROC_STATE_TRANSMIT_ADDRESS 15
#define PROC_STATE_TRANSMIT_LENGTH 16
#define PROC_STATE_TRANSMIT_DATA 17
#define PROC_STATE_TRANSMIT_CHECKSUM 18
#define PROC_STATE_TRANSMIT_ALMOST_COMPLETE 19
#define PROC_STATE_TRANSMIT_COMPLETE 20

/* Error Codes */
#define ERR_START_BYTE_INSIDE_PACKET 1
#define ERR_UNEXPECTED_START_BYTE 2
#define ERR_UNKNOWN_ESCAPED_CHARACTER 3
#define ERR_EXCESSIVE_PACKET_LENGTH 4
#define ERR_CHECKSUM_MISMATCH 5
#define ERR_BUFFER_INSUFFICIENT 6
#define ERR_RECEIVED_IGNORE_BYTE 7

/* Buffer Sizes */
#define SERIAL_RECEIVE_BUFFER_SIZE 50
#define SERIAL_TRANSMIT_BUFFER_SIZE 50

#ifndef byte
typedef unsigned char byte;
#endif

typedef struct SSerialData SerialData;

struct SSerialData
{
    /* Receiving Variables */
    byte receive_data [SERIAL_RECEIVE_BUFFER_SIZE];
    byte receive_data_count; /* Number of bytes received so far */
    byte receive_length; /* Expected number of bytes to receive */
    byte receive_next_char_is_escaped; /* need to unstuff next char? */
    byte receive_address; /* address of the received packet */
    byte receive_checksum; /* Checksum of the received packet */
    byte receive_state; /* Serial receive state variable */

    /* Transmission Variables */
    byte transmit_data [SERIAL_TRANSMIT_BUFFER_SIZE];
    byte transmit_state;
    byte transmit_address;
    byte transmit_length;
    byte transmit_checksum;
    byte transmit_escaped_char;
    byte * transmit_data_ptr;

    /* Function Pointers */
    void (*Transmit)(byte data);
    void (*TransmitPacketComplete)(void);
    void (*ReceivePacketComplete)(volatile SerialData * s);
    void (*ReceiveDataError)(byte errCode);
};

/* Returns true if a transfer is in progress,
 * false otherwise */
char SerialTransferInProgress (volatile SerialData * s);

/* Call this method when a byte transmit is complete */
void SerialByteTransmitComplete(volatile SerialData * s);

/* Call this when creating a new data structure */
void SerialDataInitialize(volatile SerialData * s);

/* Call to begin transmission of whatever is in the transmit buffer */
char SerialTransmit(volatile SerialData * s, byte address, byte length);

/* Pass whatever comes in on the serial interface to this function */
void ProcessDataChar(volatile SerialData * s, byte data);



#endif
