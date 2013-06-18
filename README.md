CNC Machine
===========

Software, Firmware, and Hardware for a 3 axis CNC Router

Software
========
The software is written in C# and set up to build/run on Windows (possibly Mono).  The program does the following things:
* Load GCode (limited functionality)
* Display the router path with an estimation of cut area
* Add/Edit gcode in a visual fashion
* Various control options for manual movement of CNC axis
* Abstracts the specific robot control enabling multiple robots

Firmware
========
The CNC machine uses an XMega128a1.  The firmware directory contains AVR studio projects and C code which can be compiled with AVR-GCC.

Hardware
========
Coming Sometime!
