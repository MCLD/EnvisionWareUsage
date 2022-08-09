# its

Front-end to display EnvisionWare computer usage by library and time.

## Required environment settings

- `DistributedCacheInstanceDiscriminator` - if set, appends the string to distributed cache keys in order to isolate them from other instances (e.g. if multiple developers are using the same distributed cache)
- `DistributedCache.RedisConfiguration` - Redis configuration information, see the [RedisCacheOptions.Configuration property](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.redis.rediscacheoptions.configuration)
- `ConnectionStrings:ComputerUsage`: Connection string with read access to the ComputerUsage database
- `ConnectionStrings:SerilogSoftwareLogs`: if provided, SQL Server database to write logging messages into

## Development

Platform: .NET Core 2.1

- Dapper
- Serilog
