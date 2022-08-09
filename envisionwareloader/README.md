# EnvisionwareLoader

Command line application (.NET Core) to extract data from the Envisionware MySQL server, place relevant data for reporting into a SQL Server database, and then aggregate daily and hourly.

## Required environment settings

- `ConnectionStrings:Envisionware`: MySQL connection string to the production Envisionware data (or a restored backup of it)
- `ConnectionStrings:ComputerUsage`: SQL Server connection string to the destination database. User should be configured as database owner so that migrations can be applied
- `SerilogSoftwareLogs` (optional): SQL Server connection string to a database where logging can be saved

## Development

Platform: .NET Core 2.1

- Dapper
- Entity Framework Core 2.1
- Serilog
