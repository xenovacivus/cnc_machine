
#include "SerialProtocol.h"


/** Serial Packet Definition:
//
// START_BYTE     start byte
// address
// length
// data(n bytes)
// Checksum ~(address + length + data)

// Escape Character and Byte Stuffing:
// Any control character is replaced
// with an escape character followed
// by it's "escaped" value.  All data
// in the transmission except for the
// start byte can be escaped.  This
// means the transmission may take up
// to twice as long as expected from
// just transmitting escape characters.
**/


/* Private helper function declarations */
void SerialStateMachineProcess(volatile SerialData *s, byte data);
byte * SerialEscapeData(volatile SerialData * s, byte address, byte length);
void SerialError(volatile SerialData *s, byte errCode);

/**
 * Helper Function: Distribute error codes to
 * handling functions, if they exist.
 */
void SerialError(volatile SerialData *s, byte errCode)
{
    if (s->ReceiveDataError!=0)
    {
        s->ReceiveDataError(errCode);
    }
}

/**
 * Initialize the serial data structure.
 * Call once before any other methods.
 */
void SerialDataInitialize(volatile SerialData * s)
{
    /* Receive State Variables */
    s->receive_state = PROC_STATE_AWAITING_START_BYTE;
    s->receive_next_char_is_escaped = false;

    /* Function Pointers */
    s->Transmit = 0;
    s->TransmitPacketComplete = 0;
    s->ReceivePacketComplete = 0;
    s->ReceiveDataError = 0;

    /* Transmit State Variables */
    s->transmit_state = PROC_STATE_TRANSMIT_COMPLETE;
    s->transmit_address = 0;
    s->transmit_length = 0;
    s->transmit_checksum = 0;
    s->transmit_data_ptr = 0;

}

/** Processes a character from a serial stream
 * and reconstructs packets.
 * @param data The next character in the stream
 */
void ProcessDataChar (volatile SerialData * s, byte data)
{
    /* Unstuff bytes and locate start bytes here */

    /* See if the data received is value to ignore
     * This most likely occurs in conjunction with
     * a frame error: start byte detected, but no
     * valid data afterwards. */
    if (data == NULL_BYTE || data == MAX_BYTE)
    {
        SerialError(s, ERR_RECEIVED_IGNORE_BYTE);
        return;
    }


    /* If any start byte is found, any current data
     * transfer will be reset, and a new data transfer
     * will begin.
     */
    if (data == START_BYTE) /* Start byte */
    {
        if (s->receive_state != PROC_STATE_AWAITING_START_BYTE)
        {
            SerialError(s, ERR_START_BYTE_INSIDE_PACKET);
        }

        /* Reset state */
        s->receive_state = PROC_STATE_AWAITING_ADDRESS;
        s->receive_data_count = 0;
        s->receive_next_char_is_escaped = false;
    }
    else
    {
        if (s->receive_state == PROC_STATE_AWAITING_START_BYTE)
        {
            SerialError(s, ERR_UNEXPECTED_START_BYTE);
            //printf("Unexpected Start Byte: Expected 0x%x, Got 0x%x\n", START_BYTE, data);
        }
        else
        {
            /* Otherwise, unstuff bytes and send data to the state machine */
            if (data == ESCAPE_CHAR) // Escape Character
            {
                s->receive_next_char_is_escaped = true;
            }
            else
            {
                if (s->receive_next_char_is_escaped)
                {
                    s->receive_next_char_is_escaped = false;
                    switch (data)
                    {
                        case ESCAPE_CHAR_ESCAPED:
                            data = ESCAPE_CHAR;
                        break;

                        case START_BYTE_ESCAPED:
                            data = START_BYTE;
                        break;

                        case NULL_BYTE_ESCAPED:
                            data = NULL_BYTE;
                        break;

                        case MAX_BYTE_ESCAPED:
                            data = MAX_BYTE;
                        break;
                    }
                }
                SerialStateMachineProcess(s, data);
            }
        }
    }
}

void SerialStateMachineProcess(volatile SerialData *s, byte data)
{
    switch (s->receive_state)
    {
        case PROC_STATE_AWAITING_ADDRESS:
            s->receive_address = data;
            s->receive_checksum = data;
            s->receive_state = PROC_STATE_AWAITING_LENGTH;
        break;

        case PROC_STATE_AWAITING_LENGTH:
            if (data > SERIAL_RECEIVE_BUFFER_SIZE)
            {
                /* Error, length too long.  Ignore packet. */
                s->receive_state = PROC_STATE_AWAITING_START_BYTE;

                /* Look for the next start byte.  Note: this
                 * will likey produce unexpected start byte errors.
                 */
                SerialError(s, ERR_EXCESSIVE_PACKET_LENGTH);
            }
            else
            {
                s->receive_length = data;
                s->receive_checksum += data;
                s->receive_state = PROC_STATE_AWAITING_DATA;
            }
        break;

        case PROC_STATE_AWAITING_DATA:

            s->receive_length--;

            s->receive_checksum += data;
            s->receive_data[s->receive_data_count] = data;
            s->receive_data_count++;

            if (s->receive_length == 0)
            {
                s->receive_state = PROC_STATE_AWAITING_CHECKSUM;
            }

        break;

        case PROC_STATE_AWAITING_CHECKSUM:
            s->receive_checksum = ~s->receive_checksum;
            if (data == s->receive_checksum)
            {
                if (s->ReceivePacketComplete != 0)
                {
                    s->ReceivePacketComplete (s);
                }
            }
            else
            {
                SerialError(s, ERR_CHECKSUM_MISMATCH);
                //printf("Error: Checksum Mismatch.  Expected 0x%x, Got 0x%x\n", s->receive_checksum, data);
            }
            s->receive_state = PROC_STATE_AWAITING_START_BYTE;
        break;

        default:
            // (It'll never get here)
        break;
    }
}



/** Call this method on USART data
 * transmit complete.
 */
void SerialByteTransmitComplete(volatile SerialData * s)
{
    byte dataToTx = 0;

    // Check if we need to transmit an escaped character:
    if (s->transmit_escaped_char != 0)
    {
        dataToTx = s->transmit_escaped_char;
        s->transmit_escaped_char = 0;

    }
    else
    {
        switch (s->transmit_state)
        {
            case PROC_STATE_TRANSMIT_ADDRESS:
                dataToTx = s->transmit_address;
                s->transmit_checksum = dataToTx;
                s->transmit_state = PROC_STATE_TRANSMIT_LENGTH;
            break;

            case PROC_STATE_TRANSMIT_LENGTH:
                dataToTx = s->transmit_length;
                s->transmit_checksum += dataToTx;
                s->transmit_state = PROC_STATE_TRANSMIT_DATA;
            break;

            case PROC_STATE_TRANSMIT_DATA:
                dataToTx = *(s->transmit_data_ptr);
                s->transmit_checksum += dataToTx;
                s->transmit_data_ptr++;
                s->transmit_length--;
                if (s->transmit_length == 0)
                {
                    s->transmit_state = PROC_STATE_TRANSMIT_CHECKSUM;
                }
            break;

            case PROC_STATE_TRANSMIT_CHECKSUM:
                dataToTx = ~s->transmit_checksum;
                s->transmit_state = PROC_STATE_TRANSMIT_ALMOST_COMPLETE;
            break;

            case PROC_STATE_TRANSMIT_ALMOST_COMPLETE:
                // Done transmitting!
                s->transmit_state = PROC_STATE_TRANSMIT_COMPLETE;
                if (s->TransmitPacketComplete!=0)
                {
                    s->TransmitPacketComplete();
                }
                return;
            break;

            default:
                // Shouldn't ever get here.
            break;
        }

        // Check for control characters
        switch(dataToTx)
        {
            case START_BYTE:
                s->transmit_escaped_char = START_BYTE_ESCAPED;
                dataToTx = ESCAPE_CHAR;
            break;

            case ESCAPE_CHAR:
                s->transmit_escaped_char = ESCAPE_CHAR_ESCAPED;
                dataToTx = ESCAPE_CHAR;
            break;

            case NULL_BYTE:
                s->transmit_escaped_char = NULL_BYTE_ESCAPED;
                dataToTx = ESCAPE_CHAR;
            break;

            case MAX_BYTE:
                s->transmit_escaped_char = MAX_BYTE_ESCAPED;
                dataToTx = ESCAPE_CHAR;
            break;

            default:
                s->transmit_escaped_char = 0;
            break;
        }
    }

    // Transmit the data!
    if (s->Transmit!=0)
    {
        s->Transmit(dataToTx);
    }

}

/** Serial Packet Transfer Query Function
 * @return true if a packet transfer is currently
 *      in progress, false otherwise.
 */
char SerialTransferInProgress(volatile SerialData * s)
{
    return (s->transmit_state != PROC_STATE_TRANSMIT_COMPLETE);
}

/**
 * Transmit serial data
 * @param address The address to send the data to
 * @param length Number of bytes in data to be sent
 * @param maxLength Number of bytes allocated for
 *      the data array.
 * @return 0 for success, nonzero for failure.
 */
char SerialTransmit(volatile SerialData * s, byte address, byte length)
{
    if (SerialTransferInProgress(s))
    {
        return -1;
    }

    if (s->Transmit == 0)
    {
        return -2;
    }

    if (length > SERIAL_TRANSMIT_BUFFER_SIZE)
    {
        return -3;
    }

    s->transmit_address = address;
    s->transmit_length = length;
    s->transmit_data_ptr = (byte *)s->transmit_data;
    s->transmit_escaped_char = 0;
    s->transmit_state = PROC_STATE_TRANSMIT_ADDRESS;

    s->Transmit(START_BYTE);

    return 0;
}
