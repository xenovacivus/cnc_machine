/*
 * xmega_test.c
 *
 * Created: 3/27/2012 7:05:53 PM
 *  Author: xeno
 */ 

#include <avr/io.h>
#include <util/delay.h>
#include <math.h>
#include <avr/interrupt.h>
#include "serial\Serial.h"
#include "clksys_driver.h"
#include "axis.h"

//#include <string.h>
//#include <stdio.h>
#include <stdbool.h>

#define X_AXIS_PWR 0
//#define Y_AXIS_PWR 33
//#define Y_AXIS_IDLE 33
#define Z_AXIS_PWR 0

#define SPEED_MIN 16
#define clock TC_CLKSEL_DIV256_gc

// Lookup tables for sinusoidal comutation
//char v[100] = {0,1,1,2,2,3,3,3,4,4,5,5,6,6,7,7,8,8,9,9,9,10,10,11,11,11,12,12,12,13,13,13,14,14,14,14,14,15,15,15,15,15,15,16,16,16,16,16,16,16,16,16,16,16,16,16,16,16,15,15,15,15,15,15,14,14,14,14,14,13,13,13,12,12,12,11,11,11,10,10,9,9,9,8,8,7,7,6,6,5,5,4,4,3,3,3,2,2,1,1};
//char v[64] = {0,1,2,2,3,4,5,5,6,7,8,8,9,10,10,11,11,12,12,13,13,14,14,14,15,15,15,16,16,16,16,16,16,16,16,16,16,16,15,15,15,14,14,14,13,13,12,12,11,11,10,10,9,8,8,7,6,5,5,4,3,2,2,1};
//uint8_t v[64] = {0,2,3,5,6,8,9,11,12,14,15,16,18,19,20,21,23,24,25,26,27,27,28,29,30,30,31,31,31,32,32,32,32,32,32,32,31,31,31,30,30,29,28,27,27,26,25,24,23,21,20,19,18,16,15,14,12,11,9,8,6,5,3,2};
uint8_t v[32] = {0,3,6,9,12,15,18,20,23,25,27,28,30,31,31,32,32,32,31,31,30,28,27,25,23,20,18,15,12,9,6,3};
//volatile uint8_t count = 0;
//volatile char direction = 1;
volatile int32_t location = 0;
volatile int32_t delta = 0;



volatile AXIS_t x_axis;
volatile AXIS_t y_axis;
volatile AXIS_t z_axis;

//volatile char directionC = 1;
volatile int32_t locationC = 0;
volatile int32_t deltaC = 0;

//volatile char directionD = 1;
//volatile int32_t locationD = 0;
//volatile int32_t deltaD = 0;
char LimitSwitchHelper (AXIS_t * axis_ptr);


volatile OnAxisTimerOverflow (volatile AXIS_t * axis)
{
	//PORTE.OUTSET = 0x01;
	if (axis->location == axis->delta)
	{
		AxisStop (axis);
		return;
	}
	if (axis->location > axis->delta)
	{
		axis->location -= 1;
	}
	else
	{
		axis->location += 1;
	}
	
	if (axis->can_update_output)
	{
		axis->current_location_buffer = axis->location;
	}
	
	uint8_t count = axis->location & 0xFF;
	if (count & 32)
	{
		axis->sign_select_port->OUTSET = axis->sign_switch_mask1;
	}
	else
	{
		axis->sign_select_port->OUTCLR = axis->sign_switch_mask1;
	}
	*axis->phase_pwm_cmp1 = v[count & 0x1F];
	
	count += 16;
	if (count & 32)
	{
		axis->sign_select_port->OUTSET = axis->sign_switch_mask2;
	}
	else
	{
		axis->sign_select_port->OUTCLR = axis->sign_switch_mask2;
	}
	*axis->phase_pwm_cmp2 = v[count & 0x1F];
	//PORTE.OUTCLR = 0x01;
}

/* Z Axis */
ISR(TCE1_OVF_vect)
{
	OnAxisTimerOverflow(&z_axis);
}

/* Y Axis */
ISR(TCD1_OVF_vect)
{
	OnAxisTimerOverflow(&y_axis);
}

ISR(TCC1_OVF_vect)
{
	OnAxisTimerOverflow(&x_axis);
}




//  Receive complete interrupt service routine.
//  Calls the common receive complete handler with pointer to the correct USART
//  as argument.
//volatile char str_ready = 0;
//volatile char str [33]; // 32 chars + null terminator
//volatile char* str_ptr = &str[0]; 


void transmit_str(char* c)
{
	while (*c != 0)
	{
		while((USARTC0.STATUS & USART_DREIF_bm) == 0) {} 
		USARTC0.DATA = *c;
		c++;
	}
}

#define TASK_LIMIT_SWITCH_X 1
#define TASK_LIMIT_SWITCH_Y 2
#define TASK_LIMIT_SWITCY_Z 3





int main(void)
{
	InitClock();
	SerialInit();
	
	PORTC.DIRSET = 0xF3;
	PORTE.DIRSET = 0xFF;
	PORTD.DIRSET = 0x33;
	PORTB.DIRCLR = 0xFF; /* PortB all inputs */
	
	x_axis.step_timer = &TCC1;
	x_axis.pwm_timer = &TCC0;
	x_axis.pwm_prescaler = TC_CLKSEL_DIV1_gc;
	x_axis.limit_switch_port = &PORTB;
	x_axis.limit_switch_mask = (1<<0); /* PORTB.0 */
	x_axis.sign_select_port = &PORTC;
	x_axis.sign_switch_mask1 = 0x20; /* PORTC.5 */
	x_axis.sign_switch_mask2 = 0x10; /* PORTC.4 */
	x_axis.phase_pwm_cmp1 = &(TCC0.CCB);
	x_axis.phase_pwm_cmp2 = &(TCC0.CCA);
	x_axis.counter_cmp_mask = TC0_CCBEN_bm | TC0_CCAEN_bm;
	x_axis.axis_idle_power = 60;
	x_axis.axis_run_power = 31;
	x_axis.min_period = 15;
	AxisInit (&x_axis);
	
	y_axis.step_timer = &TCD1;
	y_axis.pwm_timer = &TCD0;
	y_axis.pwm_prescaler = TC_CLKSEL_DIV1_gc;
	y_axis.limit_switch_port = &PORTB;
	y_axis.limit_switch_mask = (1<<1); /* PORTB.1 */
	y_axis.sign_select_port = &PORTD;
	y_axis.sign_switch_mask1 = 0x20;
	y_axis.sign_switch_mask2 = 0x10;
	y_axis.phase_pwm_cmp1 = &(TCD0.CCB);
	y_axis.phase_pwm_cmp2 = &(TCD0.CCA);
	y_axis.counter_cmp_mask = TC0_CCBEN_bm | TC0_CCAEN_bm;
	y_axis.axis_idle_power = 75;
	y_axis.axis_run_power = 50;
	y_axis.min_period = 25;
	AxisInit (&y_axis);
	
	z_axis.step_timer = &TCE1;
	z_axis.pwm_timer = &TCE0;
	z_axis.pwm_prescaler = TC_CLKSEL_DIV1_gc;
	z_axis.limit_switch_port = &PORTB;
	z_axis.limit_switch_mask = (1<<2); /* PORTB.2 */
	z_axis.sign_select_port = &PORTE;
	z_axis.sign_switch_mask1 = 0x20; /* PORTE.5 */
	z_axis.sign_switch_mask2 = 0x10; /* PORTE.4 */
	z_axis.phase_pwm_cmp1 = &(TCE0.CCD);
	z_axis.phase_pwm_cmp2 = &(TCE0.CCC);
	z_axis.counter_cmp_mask = TC0_CCDEN_bm | TC0_CCCEN_bm;
	z_axis.axis_idle_power = 60;
	z_axis.axis_run_power = 31; /* 33 unique waveform values: 0 (ground) to 33 (3.3v) */
	z_axis.min_period = 15;
	AxisInit (&z_axis);

	PMIC.CTRL |= PMIC_LOLVLEN_bm | PMIC_MEDLVLEN_bm | PMIC_HILVLEN_bm;
	uint16_t spd = 0;
	//uint16_t speed_x = 0;
	//uint16_t speed_y = 0;
	//uint16_t speed_z = 0;
	
	char task = 0;
	sei();
	while(1)
    {
		//_delay_ms (10);
		
		//SerialData * s = SerialDataTransmitStruct();
		//if (s != 0)
		//{
		//	s->transmit_data[0] = 'a';
		//	s->transmit_data[1] = 'b';
		//	s->transmit_data[2] = 'c';
		//	SerialTransmit(s, 0x00, 3);
		//}
		
		SerialData * s = SerialDataAvailable();
		if (s != 0)
		{
			switch (s->receive_data[0])
			{
				case 0x88: // Reset everything (all axis back to limit switches)
				{
					task = TASK_LIMIT_SWITCH_Y;
					x_axis.state = 1;
					y_axis.state = 1;
					z_axis.state = 1;
					s->transmit_data[0] = 0x88;
					SerialTransmit (s, 0x00, 1); // Transmit to master device
					break;
				}
				case 0x77: // Ping (and location/status information)
				{
					s->transmit_data[0] = 0x77;
					
					int32_t x = AxisGetCurrentPosition(&x_axis);
					int32_t y = AxisGetCurrentPosition(&y_axis);
					int32_t z = AxisGetCurrentPosition(&z_axis);
					
					s->transmit_data[1] = x & 0xFF; x = x >> 8;
					s->transmit_data[2] = x & 0xFF; x = x >> 8;
					s->transmit_data[3] = x & 0xFF; x = x >> 8;
					s->transmit_data[4] = x & 0xFF;
					
					s->transmit_data[5] = y & 0xFF; y = y >> 8;
					s->transmit_data[6] = y & 0xFF; y = y >> 8;
					s->transmit_data[7] = y & 0xFF; y = y >> 8;
					s->transmit_data[8] = y & 0xFF;
					
					s->transmit_data[9] = z & 0xFF; z = z >> 8;
					s->transmit_data[10] = z & 0xFF; z = z >> 8;
					s->transmit_data[11] = z & 0xFF; z = z >> 8;
					s->transmit_data[12] = z & 0xFF;
					
					uint8_t status_bits = 0;
					if (IsMoving (&x_axis))
					{
						status_bits |= 0x01;
					}
					if (IsMoving (&y_axis))
					{
						status_bits |= 0x02;
					}
					if (IsMoving (&z_axis))
					{
						status_bits |= 0x04;
					}
					s->transmit_data[13] = status_bits;
					
					SerialTransmit (s, 0x00, 14); // Transmit to master device
					break;
				}
				case 0x22: // Set Speed
				{
					uint16_t speed = 0;
					uint8_t inc = 0;
					uint8_t i = 0;
					for (i = 2; i >= 1; i--)
                    {
						speed = (speed << 8) | s->receive_data[i];
                    }
					spd = speed;
					
					s->transmit_data[0] = 0x22;
					SerialTransmit (s, 0x00, 1); // Transmit to master device
					break;
				}
				case 0x32: // Set Position + Speed
				{
					uint16_t speed_x = 0;
					uint16_t speed_y = 0;
					uint16_t speed_z = 0;
					int32_t x = 0;
					int32_t y = 0;
					int32_t z = 0;
					uint8_t i = 0;
					
					/* Bytes 1:6 are speed */
					for (i = 2; i >= 1; i--)
					{
						speed_x = (speed_x << 8) | s->receive_data[i];
						speed_y = (speed_y << 8) | s->receive_data[i+2];
						speed_z = (speed_z << 8) | s->receive_data[i+4];
					}
					
					/* Bytes 7:18 are position */
					for (i = 10; i >= 7; i--)
                    {
                        x = (x << 8) | s->receive_data[i];
                        y = (y << 8) | s->receive_data[i + 4];
                        z = (z << 8) | s->receive_data[i + 8];
                    }
					
					AxisRun(&x_axis, x, speed_x);
					AxisRun(&y_axis, y, speed_y);
					AxisRun(&z_axis, z, speed_z);
					
					s->transmit_data[0] = 0x32;
					SerialTransmit(s, 0x00, 1);
					break;
				}
				case 0x42: // Set Position
				{
					// Stop timers
					TCC1.CTRLA = 0;
					TCD1.CTRLA = 0;
					TCE1.CTRLA = 0;
					
					uint8_t i;
					int32_t x = 0;
					int32_t y = 0;
					int32_t z = 0;
					for (i = 4; i >= 1; i--)
                    {
                        x = (x << 8) | s->receive_data[i];
                        y = (y << 8) | s->receive_data[i + 4];
                        z = (z << 8) | s->receive_data[i + 8];
                    }
					
					int32_t largest = abs(x);
				
					if (abs (y) > largest)
					{
						largest = abs (y);
					}
					if (abs (z) > largest)
					{
						largest = abs (z);
					}
					
					uint32_t x_speed_big = 0;
					uint32_t y_speed_big = 0;
					uint32_t z_speed_big = 0;
					
					if (x == 0)
					{
						x_speed_big = 0xFFFFFFFF;
					}
					else
					{
						x_speed_big = (uint32_t)(((uint32_t)spd * largest) / abs (x));
					}
					
					if (y == 0)
					{
						y_speed_big = 0xFFFFFFFF;
					}
					else
					{
						y_speed_big = (uint32_t)(((uint32_t)spd * largest) / abs (y));
					}
					
					if (z == 0)
					{
						z_speed_big = 0xFFFFFFFF;
					}
					else
					{
						z_speed_big = (uint32_t)(((uint32_t)spd * largest) / abs (z));
					}
					
					//uint32_t y_speed_big = (uint32_t)(((uint32_t)spd * largest) / abs (y));
					//uint32_t z_speed_big = (uint32_t)(((uint32_t)spd * largest) / abs (z));
				
					if (x_speed_big & 0xFFFF0000) { x_speed_big = (1<<16) - 1;}
					if (y_speed_big & 0xFFFF0000) { y_speed_big = (1<<16) - 1;}
					if (z_speed_big & 0xFFFF0000) { z_speed_big = (1<<16) - 1;}
				
					uint16_t x_speed = (uint16_t) x_speed_big;
					uint16_t y_speed = (uint16_t) y_speed_big;
					uint16_t z_speed = (uint16_t) z_speed_big;
					
					// Reset Timers
					TCC1.CNT = 0;
					TCD1.CNT = 0;
					TCE1.CNT = 0;
					TCC1.PER = x_speed;
					TCD1.PER = y_speed;
					TCE1.PER = z_speed;
				
					deltaC += x;
					//deltaD += y;
					delta  += z;
					//TCD0.PER = Y_AXIS_PWR; // Y Axis Run Value
					TCD0.CTRLFSET = TC_CMD_RESTART_gc;
					
					// Start timers
					TCC1.CTRLA = clock;
					TCD1.CTRLA = clock;
					TCE1.CTRLA = clock;
					
					// Confirm data received
					s->transmit_data[0] = 0x42;
					
					s->transmit_data[1] = x_speed & 0xFF;
					s->transmit_data[2] = (x_speed >> 8) & 0xFF;
					
					s->transmit_data[3] = y_speed & 0xFF;
					s->transmit_data[4] = (y_speed >> 8) & 0xFF;
					
					s->transmit_data[5] = z_speed & 0xFF;
					s->transmit_data[6] = (z_speed >> 8) & 0xFF;
					
					SerialTransmit (s, 0x00, 7);
					break;
				}
			}				
		}
		else if (task == TASK_LIMIT_SWITCH_Y)
		{
			uint8_t v = LimitSwitchHelper(&x_axis);
			v &= LimitSwitchHelper(&y_axis);
			v &= LimitSwitchHelper(&z_axis);
			if (v)
			{
				task = 0;
			}
			
		}
	}
}

char LimitSwitchHelper (AXIS_t * axis_ptr)
{
	if (axis_ptr->state == 1)
	{
		/* Send command to move axis toward limit switch (positive direction) */
		if (!IsMoving (axis_ptr))
		{
			if (IsOnLimit(axis_ptr)) //PORTB_IN & 0x02) // Active High, limit switch hit = high
			{
				axis_ptr->state = 2;
			}
			else
			{
				AxisRun (axis_ptr, axis_ptr->location + 200, 0); /* Move 200 ticks with the minimum period (fast as possible) */
			}					
		}				
	}
	else if (axis_ptr->state == 2)
	{
		/* Send command to move axis away from the limit switch (slowly) */
		if (!IsMoving (axis_ptr)) //if (deltaD == locationD)
		{
			if (IsOnLimit(axis_ptr)) //if (PORTB_IN & 0x02) // Active High, limit switch hit = high
			{
				AxisRun (axis_ptr, axis_ptr->location - 5, 200); /* Move -5 ticks with period 200 */
			}
			else
			{
				axis_ptr->state = 3; 
			}					
		}	
	}
	else if (axis_ptr->state == 3)
	{
		ZeroLocation (axis_ptr);
		//task = -1;
		axis_ptr->state = 0;
		/* Done */
		return 1;
	}
	return 0;
}


void InitClock ()
{
	/* Clock Setup */
	
	/* Enable for external 2-9 MHz crystal with quick startup time
		* (256CLK). Check if it's stable and set it as the PLL input.
		*/
	CLKSYS_XOSC_Config( OSC_FRQRANGE_2TO9_gc, false, OSC_XOSCSEL_EXTCLK_gc );
	CLKSYS_Enable( OSC_XOSCEN_bm );
	do {} while ( CLKSYS_IsReady( OSC_XOSCRDY_bm ) == 0 );
	
	/*  Configure PLL with the 8 MHz external clock as source and
		*  multiply by 4 to get 32 MHz PLL clock and enable it. Wait
		*  for it to be stable and set prescaler C to divide by two
		*  to set the CPU clock to 16 MHz.
		*/
	CLKSYS_PLL_Config(OSC_PLLSRC_XOSC_gc, 4 );
	CLKSYS_Enable( OSC_PLLEN_bm );
	CLKSYS_Prescalers_Config( CLK_PSADIV_1_gc, CLK_PSBCDIV_1_2_gc );
	do {} while ( CLKSYS_IsReady( OSC_PLLRDY_bm ) == 0 );
	CLKSYS_Main_ClockSource_Select( CLK_SCLKSEL_PLL_gc );
}