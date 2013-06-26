/*
 * Axis.c
 *
 * Created: 4/15/2012 3:47:48 PM
 *  Author: xeno
 */ 

#include "Axis.h"

void AxisInit (volatile AXIS_t * axis)
{
	axis->pwm_timer->CTRLA = TC_CLKSEL_DIV1_gc;
	axis->pwm_timer->CTRLB = TC_WGMODE_SS_gc | axis->compare_mask; /* Single Slope PWM, Top at PER, Output Compares Enabled */
	axis->pwm_timer->CTRLD = TC_EVSEL_OFF_gc; /* No Event Action */
	axis->pwm_timer->PER = axis->axis_run_power;
	*axis->phase_pwm_cmp1 = 0;
	*axis->phase_pwm_cmp2 = 0;
	
	axis->delta = 0;
	axis->location = 0;
	
	axis->state = 0;
	
	axis->can_update_output = true;
	axis->current_location_buffer = 0;
}
int32_t AxisGetCurrentPosition (volatile AXIS_t * axis)
{
	/* If it's moving, use the buffered value for interrupt safe access */
	if (IsMoving(axis))
	{
		axis->can_update_output = false;
		uint32_t return_value = axis->current_location_buffer;
		axis->can_update_output = true;
		return return_value;
	}
	else
	{
		return axis->location;
	}
}
volatile bool IsOnLimit (volatile AXIS_t * a)
{
	/* TODO: read limit switch pin multiple times */
	return (a->limit_switch_port->IN & a->limit_switch_mask); /* Active high */
}
volatile bool IsMoving (volatile AXIS_t * axis)
{
	/* TODO: Return something reasonable here... */
	return (axis->delta != 0);
}
void AxisRun (volatile AXIS_t * axis, int32_t location, uint16_t speed)
{
	// TODO: Re-implement!
	///* Make sure the axis is stopped before updating values and running again */
	//if (IsMoving (axis))
	//{
	//	AxisStop(axis);
	//}
	//
	///* Clamp the period to the minimum defined period (fastest speed) */
	//if (speed < axis->min_period)
	//{
	//	speed = axis->min_period;
	//}
	//
	///* Update location */
	//axis->delta = location;
	
	/* Update and restart the timers */
	//axis->pwm_timer->PER = axis->axis_run_power;
	//axis->pwm_timer->CTRLFSET = TC_CMD_RESTART_gc;
}
void AxisStop (volatile AXIS_t * axis)
{
	// TODO: Re-implement!
	/* Set the axis to idle power */
	//axis->pwm_timer->PER = axis->axis_idle_power;
	//axis->pwm_timer->CTRLFSET = TC_CMD_RESTART_gc;
	
	/* Update the current position buffer */
	//axis->current_location_buffer = axis->location;
}
void ZeroLocation (volatile AXIS_t * axis)
{
	AxisStop(axis);
	axis->delta = 0;
	axis->location = 0;
	axis->current_location_buffer = 0;
}

