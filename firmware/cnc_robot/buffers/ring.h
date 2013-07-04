#ifndef RING_BUFFER_H_INCLUDED
#define RING_BUFFER_H_INCLUDED

#include <stdint.h>
#include <stdbool.h>
#include "../location.h"

typedef Location_t elem_type;

typedef struct RingBuffer_T
{
    uint8_t max_length;
    uint8_t head;
    uint8_t count;
    elem_type * buffer;
} RingBuffer;

/**
 * Initialize the ring buffer
 * @param RingBuffer * r Ring buffer to initialize
 * @param uint8_t length Number of elements in the ring buffer.
 * Must be a power of 2 in the range [2, 128]
 * @param elem_type * buffer Pointer to the raw storage array
 */
void RingBufferInit(RingBuffer * r, uint8_t length, elem_type * buffer);

bool RingBufferIsEmpty(RingBuffer * r);

bool RingBufferIsFull(RingBuffer * r);

uint8_t RingBufferCount(RingBuffer * r);

void RingBufferAdd(RingBuffer * r, elem_type element);

elem_type RingBufferGet(RingBuffer * r);


#endif // RING_BUFFER_H_INCLUDED
