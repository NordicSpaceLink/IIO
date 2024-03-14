# NordicSpaceLink.IIO

This library provides C# bindings for `libiio`.

It should support almost all of the API surface using natural C# work flow.

Buffers are interfaced to with Span's only, and object lifetimes are managed with IDisposable interfaces where relevant.

## Example

The following example for an AD936x based system shows how to work with attributes, devices, and channels.

```c#
using var context = Context.Default;

var phy = context.Devices["ad9361-phy"];
var lpc = context.Devices["cf-ad9361-lpc"];

phy.Attributes["ensm_mode"].Value = "alert";
phy.Channels[("RX_LO", true)].Attributes["frequency"].Float = 2.4e9;

phy.Channels[("voltage0", false)].Attributes["sampling_frequency"].Float = 2.1e6;
lpc.Channels[("voltage0", false)].Attributes["sampling_frequency"].Float = 2.1e6 / 8;

lpc.Channels[("voltage0", false)].Enabled = true;
lpc.Channels[("voltage1", false)].Enabled = true;

using var buffer = lpc.CreateBuffer(4096, false);

phy.Attributes["ensm_mode"].Value = "fdd";

var sw = Stopwatch.StartNew();
var data = buffer.Refill();

Console.WriteLine("Read {0} bytes in {1} seconds", data.Length, sw.Elapsed.TotalSeconds);

phy.Attributes["ensm_mode"].Value = "alert";
```
