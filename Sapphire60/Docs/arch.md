# *Sapphire/60 Documentation*

Follow [this link](asm.md) for assembly reference.

## Memory mapping

The *Sapphire/60* has 16K RAM, expandable to 64K.

> ***Note:** All address ranges in this list are hexadecimal.*

1. **`0000 - 0080`**: Interrupt vector table, 128 bytes (64 addresses)
2. **`0080 - 0880`**: Interrupts, 2K
3. **`0880 - 1050`**: Screen buffer, monochrome, *bankable?*, 80x25 bytes
4. **`1050 - 1850`**: Character set, *bankable?*, 2K
5. **`1850 - 1852`**: Keyboard, 2 bytes
6. **`1852 - 1872`**: Peripheral port, 32 bytes

## Keyboard

The keyboard has an internal buffer of 256 ASCII characters, addressible as per the memory map. The first byte denotes the queue length, the second contains the character at the top of the queue.

To read a string from the keyboard, you could loop and grab the second byte until the first byte becomes 0.

## Peripheral access

Sapphire/60 supports up to 256 devices, although actually addressing that many will take ages. The first byte is how you select a peripheral device from the bus. The second byte is the device identifier (see table in the *[Device Identifiers](#device-identifiers)* section). The next 12 bytes are RX, the remaining 12 are TX. It's up to the device to decide the function of those bytes.

## Interrupts

Sapphire/60 memory map allows for 64 custom software interrupts and 2K of interrupt code. The interrupt vector should point at the first instruction of your interrupt and end with an `RFI` instruction in order to return back to where the program left off.

## Device identifiers

> ***Note:** This information is not unique to Sapphire/60 and is part of Retrograde itself.*

> ***Note:** Mods could add their own identifiers as part of mod compatibility. **Mod developers should allow users to remap identifiers in their configs.*** 

> ***Note:** All indices in this list are in hexadecimal.*

**`00`**: Not connected
**`01`**: Persistent storage
**`02`**: Redstone I/O
**`03`**: Sound I/O 
**`04`**: Network modem
**`05`**: Proximity card reader
**`06`**: Printer
**`07`**: Scanner 
**`08`**: Transposer
**`09`**: EEPROM
**`0A`**: Entropy source
**`0B`**: Precision timer
**`0C`**: Float coprocessor
**`0D`**: RTC
**`0E`**: Reserved
**`0F`**: Adapter
**`--`**: Reserved
**`17`**: Reserved
