# ChargePointNet

Bridge project to serve EV chargers with a simple REST API.

This project is currently focused on EVBox chargers but has been created with supporting other chargers in the future.

## Installation

1. Download the [latest release](https://github.com/AeonLucid/ChargePointNet) or [latest nightly](https://nightly.link/AeonLucid/ChargePointNet/workflows/build/main?preview).
2. Configure `appsettings.json`.
3. Run `ChargePointNet.exe`.

When the API is running, you can access the following urls.

| Item | Url |
|-|-|
| API | http://localhost:8080/api |
| API Docs | http://localhost:8080/docs |
| Demo | http://localhost:8080/demo |

## Development

If you wish to develop the project further.

1. Download [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
2. Open `src/ChargePointNet.sln` with [Rider](https://www.jetbrains.com/rider/) or [Visual Studio](https://visualstudio.microsoft.com/).
3. Run `ChargePointNet`.

## Issues

If you get the error on Windows that the device is malfunctioning, you need to replace the CH340 driver by installing the CH340 3.4 driver from https://sparks.gogo.co.nz/ch340.html.

## Credits

Thanks to Maarten Tromp for laying the ground work on reverse engineering the EVBox chargers.

- https://www.geekabit.nl/
- https://www.geekabit.nl/projects/managed-ev-charger-to-stand-alone
- https://www.geekabit.nl/projects/managed-ev-charger-to-stand-alone/protocol/