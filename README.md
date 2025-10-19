This utility is written to extract the files from Rise of the Dragon (though it's named WillyBeamishDump.  It can extract the speech for that game, but I haven't really tested it since adding Rise of the Dragon support).  This utility will extract and unpack specific files.  You can place the extracted files in the Rise of the Dragon directory, and the game will use them instead of the data found in the VOLUME files.

Also included is a utility to convert the TTM files to/from a human readable XML format.  This helped with quickly editing the files to change stuff around, though it still uses hard coded values, so you'll need to edit those.

This code was quickly written, and assuming a perfect copy of the game (unmodded), so I didn't add any real error detection, where there probably should be some.

Also attached is a program to compress and decompress the LZW files.  It's matched every file I've put it through for Willy Beamish and Rise of the Dragon, but I haven't done a full test of every single compressed section in their games, so there might be a bug or two.  This tries to do an exact matching compression, rather than a lazy LZW compression from 

https://www.vogons.org/viewtopic.php?p=273120#273120

The decompression code is based off of ScummVM's Dynamix LZW code.
