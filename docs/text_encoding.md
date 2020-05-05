# `FC`

Appears to be settings/rendering hints about a block of string data.

`FC ff ww ww ll uu ..`

* `ff` - Flags.
Flag 0 (`01`) appears to indicate end of data, and stops parsing completely even if length is non-zero.
* `ww ww` - Width? Possibly a hint to the text renderer of some sort.
* `ll` - Byte length of following string data.
* `uu` - Always seems to be 8.

# `FD`

Unknown.

# `FE`

Unknown.

# `FF` - Text Formatting Operations

[See Text Formatting](text_formatting.md).
