# mp3-slap

Helps process FFMpeg's or other's silence between tracks / track splitting, and other such functions

## Links


*

## Generate log of silence points with ffmpeg silencedetect

Helpful links:

* [Docs (ffmpeg:silencedetect)](https://ffmpeg.org/ffmpeg-all.html#silencedetect)

* [A](https://stackoverflow.com/questions/55057778/how-can-i-split-an-mp4-video-with-ffmpeg-every-time-the-volume-is-zero)

* [B](https://stackoverflow.com/questions/36074224/how-to-split-video-or-audio-by-silent-parts)

Under ["5.4 Main options"](https://ffmpeg.org/ffmpeg-all.html#toc-Main-options):

`-nostats` : Ah-hah, seems we can cleanup the log with this, but ... our processor still will strip the other crud fine:

> Print encoding progress/statistics. It is on by default, to explicitly disable it you need to specify `-nostats`.

### silencedetect script

When `cd` in directory containing file `gen.mp3`, and with a 'gen' subdirectory (else remove `gen/`)

```bash
ffmpeg -nostats -i gen.mp3 -af silencedetect=noise=-30dB:d=3.5 -f null - 2> silence.log
```

Notes on this terse language:

`-af`: "filtergraph (output)", alias for `-filter:a` [see](https://ffmpeg.org/ffmpeg-all.html#filter_005foption)

Very confusing this language:

> Create the filtergraph specified by filtergraph and use it to filter the stream. ... filtergraph is a description of the filtergraph to apply to the stream, and must have a single input and a single output of the same type of the stream.

Only options in docs are `-filter:v` (seems to be for Video) and `-filter:a`, for audio... Maybe we don't even need this. So `-af` may just mean: Filter to only Audio! Why the heck not just say that simply like that?!

`-f null -`

* [See](https://ffmpeg.org/ffmpeg-all.html#toc-Examples-70)

`2> silence.log`: Print out to that log file (see not on `2` next). Just leave this off (including the 2) and it will print to console instead.

`2>`: weird. I can find no reason why `2` is here. The `>` seems obvious, perhaps saying push output to this file instead of to console? But why `2`? Find no examples in docs.

