#include "ring.h"

void RingBufferInit(RingBuffer * r, uint8_t length, elem_type * buffer)
{
    r->head = 0;
    r->count = 0;
    r->max_length = length - 1;
    r->buffer = buffer;
}

bool RingBufferIsEmpty(RingBuffer * r)
{
    return r->count == 0;
}

bool RingBufferIsFull(RingBuffer * r)
{
    return r->count > r->max_length;
}

uint8_t RingBufferCount(RingBuffer * r)
{
    return r->count;
}

void RingBufferAdd(RingBuffer * r, elem_type element)
{
    r->buffer[r->head & r->max_length] = element;
    r->head++;
    r->count++;
}

elem_type RingBufferGet(RingBuffer * r)
{
    elem_type e = r->buffer[(r->head - r->count) & r->max_length];
    r->count--;
    return e;
}
