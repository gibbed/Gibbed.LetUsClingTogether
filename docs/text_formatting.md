# Text Formatting

Text formatting operations are done with a `FF` byte followed by an operation byte, plus any arguments.

`FF xx ..`

Using an operation outside of the range `80` - `A9` will likely cause game crashes.

| Code                   | Note |
| ---------------------- | ---- |
| `FF 80`                | Insert newline. |
| `FF 81 ?? ??`          | Unknown. Dialogue pacing? |
| `FF 82 rr gg bb aa`    | [Set text color](#set-text-color). |
| `FF 83 ?? ?? ?? ?? ??` | Unknown. |
| `FF 84 ?? ?? ??`       | Unknown. |
| `FF 85 ?? ??`          | Unknown. |
| `FF 86`                | Unknown. |
| `FF 87 ??`             | Unknown. |
| `FF 88 ss`             | Insert pixel spacing. |
| `FF 89 ??`             | Unknown. |
| `FF 8A ii`             | [Insert glyph](#insert-glyph). 1-23 (`01`-`17`) are valid. |
| `FF 8B ?? ??`          | Unknown. |
| `FF 8C ??`             | Unknown. |
| `FF 8D ?? ??`          | Unknown. |
| `FF 8E ?? ??`          | Unknown. |
| `FF 8F ?? ??`          | Unknown. |
| `FF 90 ?? ??`          | Unknown. |
| `FF 91`                | Unknown. |
| `FF 92`                | Unknown. |
| `FF 93`                | Insert protagonist name. |
| `FF 94`                | Insert order name. |
| `FF 95 ?? ?? ??`       | Unknown. |
| `FF 96`                | Unknown. |
| `FF 97`                | Unknown. |
| `FF 98 ?? ??`          | Unknown. |
| `FF 99 ?? ??`          | Unknown. |
| `FF 9A ??`             | Unknown. |
| `FF 9B`                | Reset text color. |
| `FF 9C ?? ??`          | Unknown. |
| `FF 9D ??`             | Unknown. |
| `FF 9E`                | Unknown. |
| `FF 9F`                | Uppercase following character. |
| `FF A0`                | Unknown. |
| `FF A1`                | Unknown. |
| `FF A2`                | Unknown. |
| `FF A3`                | Unknown. |
| `FF A4`                | Unknown. |
| `FF A5`                | Unknown. |
| `FF A6`                | Unknown. |
| `FF A7`                | Unknown. |
| `FF A8`                | Unknown. |
| `FF A9`                | Unknown. |

## Set Text Color

`FF 82 rr gg bb aa`

`rr` = Red component
`gg` = Green component
`bb` = Blue component
`aa` = Alpha component

Each component is computed as `component - 1`. So for the brighest red, `FF 82 00 01 01 00` becomes `rgba(255,0,0,255)` (`FF0000FF`).

## Insert Glyph

`FF 8A ii`

`ii` = Glyph index

*For the original unmodified font family in the game:*

Indices 1-23 (`01`-`17`) are valid.

![Glyph Map](https://i.imgur.com/SWHNzEe.png)

| d  | h    | Description                        | Extra |
| -- | ---- | ---------------------------------- | ----- |
| 1  | `01` | Square button                      | large |
| 2  | `02` | Circle button                      | large |
| 3  | `03` | Cross button                       | large |
| 4  | `04` | Triangle button                    | large |
| 5  | `05` | D-pad Up button                    | small |
| 6  | `06` | D-pad Down button                  | small |
| 7  | `07` | D-pad Right button                 | small |
| 8  | `08` | D-pad Left button                  | small |
| 9  | `09` | Trigger Left button                |       |
| 10 | `0A` | Trigger Right button               |       |
| 11 | `0B` | Select button                      |       |
| 12 | `0C` | Start button                       |       |
| 13 | `0D` | Font block                         |       |
| 14 | `0E` | Square button                      | small |
| 15 | `0F` | Circle button                      | small |
| 16 | `10` | Triangle button                    | small |
| 17 | `11` | Cross button                       | small |
| 18 | `12` | D-Pad Up and D-Pad down buttons    | small |
| 19 | `13` | D-Pad Left and D-Pad Right buttons | small |
| 20 | `14` | D-Pad Right button                 | large |
| 21 | `15` | D-Pad Left button                  | large |
| 22 | `16` | :x:                                |       |
| 23 | `17` | :heavy_check_mark:                 |       |
