# PicoFlasher

Picoflasher is a simple tool to auto flash a raspberry pico in bootloader mode, you bind it to .elf files on windows and it will try to detect the .u2f file and copy it any usb key who's model name is RPI RP2

This is usefull because visual studio code (Using the cmake tools) executes the .elf file so to flash it you can just click the start button. It will also wait for the pi if none is present
