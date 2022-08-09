# EnvisionWareUsage

This is an archived project that's being uploaded so the code doesn't just disappear. It may not run out-of-the-box without some work.

This project is not related to EnvisonWare, Inc. beyond its reading data from a computer usage database in their software. It is provided without warranty or guarantee. Here be dragons.

EnvisionWareUsage is two .NET Core 2.1 projects:

- envisionwareloader - a console app to read usage data out of an EnvisionWare MySQL database
- its - an ASP.NET Web site to show the aggregated usage data

Notes:

- It's unknown which specific versions of EnvisionWare this application will support, it relies on the table `tbluserpcrdetail` which was, for our installation, in the EnvisionWare MySQL database.
- Both projects can be run in Docker, there are included functional Dockerfiles.
- .NET Core 2.1 is no longer supported by Microsoft. This should be updated to user a newer platform if it's to be used in a production environment.
- The its Web site has integration with the authentication portion of [MCLD/Ocuda](https://github.com/MCLD/ocuda) but could be reworked for another system.
- The its project does do a cool heatmap-ish calendar display, that code might be workable in other projects (see below).

![its project screenshot](https://github.com/MCLD/EnvisionWareUsage/blob/main/its-screenshot.png?raw=true)

## License

EnvisionWareUsage is Copyright 2018 by the Maricopa County Library District and is distributed under the [MIT License](http://opensource.org/licenses/MIT).
