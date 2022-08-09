using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EnvisionwareLoader.Data;
using EnvisionwareLoader.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace EnvisionwareLoader
{
    internal static class Program
    {
        private const string ApplicationEnrichment = "Application";
        private const string VersionEnrichment = "Version";
        private const string IdentifierEnrichment = "Identifier";
        private const string InstanceEnrichment = "Instance";
        private const string RemoteAddressEnrichment = "RemoteAddress";

        private static void Main(string[] args)
        {
            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var id = Process.GetCurrentProcess().Id;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var config = builder.Build();

            const string envisionCsName = nameof(ConnectionString.Envisionware);

            if (string.IsNullOrEmpty(config.GetConnectionString(envisionCsName)))
            {
                throw new MissingConnectionStringException(envisionCsName);
            }

            const string reportingCs = nameof(ConnectionString.ComputerUsage);

            if (string.IsNullOrEmpty(config.GetConnectionString(reportingCs)))
            {
                throw new MissingConnectionStringException(reportingCs);
            }

            var logConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty(ApplicationEnrichment, applicationName)
                .Enrich.WithProperty(VersionEnrichment, version)
                .Enrich.WithProperty(IdentifierEnrichment, id)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            var sqlCs = config.GetConnectionString(nameof(ConnectionString.SerilogSoftwareLogs));

            if (!string.IsNullOrEmpty(sqlCs))
            {
                logConfig
                    .WriteTo.Logger(_ => _
                    .WriteTo.MSSqlServer(sqlCs,
                        "Logs",
                        autoCreateSqlTable: true,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        columnOptions: new ColumnOptions {
                            AdditionalDataColumns = new DataColumn[]
                            {
                                new DataColumn(ApplicationEnrichment, typeof(string)),
                                new DataColumn(VersionEnrichment, typeof(string)),
                                new DataColumn(IdentifierEnrichment, typeof(int)),
                                new DataColumn(InstanceEnrichment, typeof(string)),
                                new DataColumn(RemoteAddressEnrichment, typeof(string))
                            }
                        }));
            }

            try
            {
                using (var log = logConfig.CreateLogger())
                {
                    try
                    {
                        LoadDataAsync(log, config).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        log.Fatal(ex.Message);
                        var innerException = ex.InnerException;
                        while (innerException != null)
                        {
                            log.Fatal(innerException.Message);
                            innerException = innerException.InnerException;
                        }
                    }
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }

            PromptIfDebug();
        }

        [Conditional("DEBUG")]
        private static void PromptIfDebug()
        {
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task LoadDataAsync(ILogger log, IConfigurationRoot config)
        {
            var sw = new Stopwatch();
            sw.Start();

            var envisionCs = config.GetConnectionString(nameof(ConnectionString.Envisionware));
            var reportCs = config.GetConnectionString(nameof(ConnectionString.ComputerUsage));
            using (var conn = new MySqlConnection(envisionCs))
            {
                DateTime detailLatest = DateTime.MinValue;
                using (var context = new ReportingContext(reportCs))
                {
                    var migrations = context.GetPendingMigrations();
                    if (migrations.Any())
                    {
                        log.Information("Performing {Count} database migration(s), last is: {Migrations}",
                            migrations.Count(),
                            migrations.Last());
                        context.Migrate();
                    }

                    if (context.Details.Count() > 0)
                    {
                        detailLatest = context.Details.Max(_ => _.Timestamp);
                    }
                }

                var prod = new EnvisionwareContext(log, conn);
                var earliest = await prod.GetEarliestDateAsync().ConfigureAwait(false);
                var latest = await prod.GetLatestDateAsync().ConfigureAwait(false);

                log.Information("Envisionware data runs from {earliest} to {latest} ({TotalDays} days)",
                    earliest,
                    latest,
                    Math.Round((latest.Date - earliest.Date).TotalDays));

                log.Information($"Today is {DateTime.Now.Date.ToShortDateString()} in time zone {TimeZoneInfo.Local.DisplayName}");

                if (latest.Date >= DateTime.Now.Date)
                {
                    latest = DateTime.Now.Date;
                    log.Warning("Found data from today in database, setting end date to {Latest}",
                        latest);
                }

                if (detailLatest.Date > earliest.Date)
                {
                    earliest = detailLatest.Date.AddDays(1);
                    log.Information("Data already processed up to {detailLatest}, using {earliest} as minimum",
                        detailLatest,
                        earliest);
                }

                int dayCounter = 0;
                int minuteCounter = 0;

                double totalDayCount = Math.Round((latest.Date - earliest.Date).TotalDays);

                log.Information("Processing {TotalDays} days worth of data from {StartDate:d} to {EndDate:d}",
                    totalDayCount,
                    earliest.Date,
                    latest.Date);

                var current = earliest.Date;

                using (var context = new ReportingContext(reportCs))
                {
                    var branchCodes = context.Branches.AsNoTracking().Select(_ => _.Code).ToList();

                    var lastProcessedMonth = current.Month;

                    while (DateTime.Compare(current, latest) < 0)
                    {
                        if (lastProcessedMonth != current.Month)
                        {
                            var percent = (double)dayCounter / totalDayCount * 100;
                            log.Debug("Status: {OnDay}/{TotalDays} {Percent:N0}% ~{per:N0}s each, Elapsed: {Elapsed:hh\\:mm\\:ss}, Est remaining: {Estimated:hh\\:mm\\:ss}",
                                dayCounter,
                                totalDayCount,
                                percent,
                                sw.Elapsed.TotalSeconds / dayCounter,
                                sw.Elapsed,
                                TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds / (percent / 100)));
                        }

                        var records = await prod.GetDayDataAsync(current).ConfigureAwait(false);
                        log.Debug("Processing {CurrentDate:d}, branches: {BranchCount}, records: {RecordCount}",
                            current,
                            records.Select(_ => _.PcrBranch).Distinct().Count(),
                            records.Count());

                        dayCounter++;
                        minuteCounter += records.Sum(_ => _.PcrMinutesUsed);

                        var hourly = new List<HourlySummary>();
                        var daily = new List<DailySummary>();

                        foreach (var pcrDetail in records)
                        {
                            var detail = new UsageDetail {
                                Key = pcrDetail.PcrKey,
                                Area = pcrDetail.PcrArea.Trim(),
                                Branch = pcrDetail.PcrBranch.Trim(),
                                Minutes = pcrDetail.PcrMinutesUsed,
                                Timestamp = pcrDetail.PcrDateTime
                            };
                            await context.AddAsync(detail).ConfigureAwait(false);

                            var neatBranch = detail.Branch.Trim().ToUpper();

                            var hourlyResolution = new DateTime(detail.Timestamp.Year,
                                detail.Timestamp.Month,
                                detail.Timestamp.Day,
                                detail.Timestamp.Hour,
                                0,
                                0);

                            var hourlyStat = hourly.SingleOrDefault(_ => _.Branch == neatBranch
                                && _.Date == hourlyResolution);

                            if (hourlyStat != null)
                            {
                                hourlyStat.Minutes += pcrDetail.PcrMinutesUsed;
                            }
                            else
                            {
                                hourly.Add(new HourlySummary {
                                    Date = hourlyResolution,
                                    Branch = neatBranch,
                                    Minutes = pcrDetail.PcrMinutesUsed
                                });
                            }

                            var dailyStat = daily.SingleOrDefault(_ => _.Branch == neatBranch
                                    && _.Date == current.Date);

                            if (dailyStat != null)
                            {
                                dailyStat.Minutes += pcrDetail.PcrMinutesUsed;
                            }
                            else
                            {
                                daily.Add(new DailySummary {
                                    Date = current.Date,
                                    Branch = neatBranch,
                                    Minutes = pcrDetail.PcrMinutesUsed,
                                });
                            }
                        }

                        var todayBranches = daily
                            .Where(_ => _.Date == current.Date)
                            .Select(_ => _.Branch)
                            .Distinct();

                        foreach (var branch in todayBranches)
                        {
                            if (!branchCodes.Contains(branch))
                            {
                                context.Branches.Add(new Branch {
                                    Code = branch
                                });
                                branchCodes.Add(branch);
                            }

                            // find hourly maximums
                            var branchHours = hourly.Where(_ => _.Branch == branch);
                            foreach (var branchHour in branchHours)
                            {
                                var yearHours = context.HourlySummaries
                                    .Where(_ => _.Branch == branch
                                        && _.Date.Date.Year >= current.Date.AddYears(-1).Year
                                        && _.Date.Hour == branchHour.Date.Hour)
                                    .Select(_ => _.Minutes)
                                    .DefaultIfEmpty(0)
                                    .Max();

                                var monthHours = hourly
                                    .Where(_ => _.Branch == branch
                                        && _.Date.Hour == branchHour.Date.Hour)
                                    .Select(_ => _.Minutes)
                                    .DefaultIfEmpty(0)
                                    .Max();

                                branchHour.MaxMinutes = Math.Max(yearHours, monthHours);
                            }

                            // find maximums for day
                            // trailing 12 month daily maximum from database and this month
                            var yearMinutes = context.DailySummaries
                                    .Where(_ => _.Branch == branch
                                        && _.Date >= current.Date.AddYears(-1))
                                    .Select(_ => _.Minutes)
                                    .DefaultIfEmpty(0)
                                    .Max();

                            var monthMinutes = daily
                                .Where(_ => _.Branch == branch)
                                .Select(_ => _.Minutes)
                                .DefaultIfEmpty(0)
                                .Max();

                            var dailySummary = daily.SingleOrDefault(_ => _.Branch == branch
                                && _.Date == current.Date);

                            dailySummary.MaxMinutes = Math.Max(yearMinutes, monthMinutes);
                        }

                        await context.AddRangeAsync(daily).ConfigureAwait(false);
                        await context.AddRangeAsync(hourly).ConfigureAwait(false);

                        await context.SaveChangesAsync().ConfigureAwait(false);

                        lastProcessedMonth = current.Month;
                        current = current.AddDays(1);
                    }
                }
                Log.Information("Total {totalDays:N0} days for {totalMinutes:N0} minutes", dayCounter, minuteCounter);

                sw.Stop();
                log.Information("Finished at {Now}, elapsed time: {TotalSeconds:hh\\:mm\\:ss}",
                    DateTime.Now,
                    sw.Elapsed);
            }
        }
    }
}
