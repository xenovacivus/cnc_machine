/*
 * Axis.h
 *
 * Created: 4/15/2012 3:48:17 PM
 *  Author: xeno
 */ 

#ifndef AXIS_H
#define AXIS_H

#include <stdbool.h>
#include <avr/io.h>
#include <avr/interrupt.h>

typedef struct
{
	volatile int32_t location;
	volatile int32_t delta;
	
	TC0_t * step_timer; /* TC1 for Sinusoidal Commutated PWM */
	TC1_t * pwm_timer; /* TC0 for step timing (increment/decrement on overflow */
	
	uint8_t pwm_prescaler;
	
	volatile PORT_t * limit_switch_port; /* Port attached to the limit switch */
	uint8_t limit_switch_mask;  /* Pin on the limit switch port */
	
	PORT_t * sign_select_port; /* Port for sign select pins */
	uint8_t sign_switch_mask1; /* Sign switch mask for phase 1 */
	uint8_t sign_switch_mask2; /* Sign switch mask for phase 2 */
	
	uint16_t * phase_pwm_cmp1;
	uint16_t * phase_pwm_cmp2;
	
	uint8_t axis_idle_power;
	uint8_t axis_run_power;
	
	uint16_t min_period;
	
	uint8_t counter_cmp_mask;
	
	char state;
	
	/* Some private data ... */
	bool can_update_output;
	int32_t current_location_buffer;
	
} AXIS_t;

int32_t AxisGetCurrentPosition (AXIS_t * axis);
bool IsAxisInterruptEnabled (AXIS_t * axis);
volatile bool IsOnLimit (volatile AXIS_t * a);
volatile bool IsMoving (volatile AXIS_t * axis);
void AxisRun (AXIS_t * axis, int32_t location, uint16_t speed);
void AxisStop (AXIS_t * axis);
void ZeroLocation (AXIS_t * axis);

#endif