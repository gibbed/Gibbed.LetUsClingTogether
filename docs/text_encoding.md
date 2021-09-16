# Codepoints

Each character is represented as a one- or two- byte encoded codepoint.

The standard control codes (such as `\n`) under value `0x20` do not exist one-to-one in this encoding.

Codepoints index into a font family present in the game data, which can be found in `600\5` file (likely named `600\5.unknown` by `UnpackFILETABLE`).

There are two known versions of the font family:

* [`JP`](text_encoding_codepoints_JP.md) has 2682 available codepoints/characters.
* [`EN`](text_encoding_codepoints_EN.md) has 267 available codepoints/characters.

The used font family depends on the release region:

* `ULJM05753` uses the `JP` font family.
* `ULUS10565` and `ULES01500` use the `EN` font family (it's identical in both regions).

# Bytes

## `00`..`1F`

`xx yy`

Indicates a two-byte encoded codepoint.

Codepoint is the result of the following calculation:

`((xx * 255) | ((yy - 0x20) & 0xFF)`

## `20`..`FB`

`xx`

Indicates a one-byte encoded codepoint.

Codepoint is the result of the following calculation:

`(xx - 0x20) & 0xFF`

## `FC`

Appears to be settings/rendering hints about a block of string data.

`FC ff ww ww ll uu ..`

* `ff` - Flags.
Flag 0 (`01`) appears to indicate end of data, and stops parsing completely even if length is non-zero.
* `ww ww` - Width? Possibly a hint to the text renderer of some sort.
* `ll` - Byte length of following string data.
* `uu` - Always seems to be 8.

## `FD`

Unknown.

## `FE`

Unknown.

## `FF` - Text Formatting Operations

[See Text Formatting](text_formatting.md).
