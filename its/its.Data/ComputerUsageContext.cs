using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using its.Data.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace its.Data
{
    public class ComputerUsageContext : IDisposable
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        private IDbConnection _dbConnection = null;

        public ComputerUsageContext(ILogger<ComputerUsageContext> logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private IDbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    var connectionString = _config.GetConnectionString("ComputerUsage");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        _logger.LogCritical("Missing connection string 'ComputerUsage'");
                        throw new MissingConnectionStringException("ComputerUsage");
                    }
                    _dbConnection = new SqlConnection(connectionString);
                }
                return _dbConnection;
            }
        }

        public void Dispose()
        {
            _dbConnection?.Dispose();
        }

        public async Task<IList<UsageSummary>> GetDailyUsageAsync(DateTime startDate,
            DateTime endDate,
            string branch = null)
        {
            var query = new StringBuilder();
            query.Append("SELECT [Date], SUM([Minutes]) [Minutes], SUM([MaxMinutes]) [MaxMinutes]");
            query.Append(" FROM [DailySummaries]");
            query.Append(" WHERE [Date] >= @StartDate AND [Date] <= @EndDate");
            if (!string.IsNullOrEmpty(branch))
            {
                query.Append(" AND [Branch] = @Branch");
            }
            query.Append(" GROUP BY [Date]");

            try
            {
                DbConnection.Open();
                var result =
                    await _dbConnection.QueryAsync<UsageSummary>(query.ToString(),
                        new { StartDate = startDate, EndDate = endDate, Branch = branch })
                        .ConfigureAwait(false);
                return result.ToList();
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public async Task<DateTime?> GetLatestDailyDateAsync(string branch = null)
        {
            var query = new StringBuilder();
            query.Append("SELECT MAX([Date]) FROM [DailySummaries]");
            if (!string.IsNullOrEmpty(branch))
            {
                query.Append(" WHERE [Branch] = @Branch");
            }

            try
            {
                DbConnection.Open();
                return
                    await _dbConnection.QuerySingleOrDefaultAsync<DateTime?>(query.ToString(),
                            new { Branch = branch })
                        .ConfigureAwait(false);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public async Task<IDictionary<string, string>> GetBranchesAsync()
        {
            const string query = "SELECT [Code], [Name] FROM [Branches];";
            try
            {
                DbConnection.Open();
                var result = await _dbConnection.QueryAsync(query).ConfigureAwait(false);
                return result.ToDictionary(_ => (string)_.Code, _ => (string)_.Name);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public async Task<IList<UsageSummary>> GetHourlyUsageAsync(DateTime date,
            string branch = null)
        {
            var query = new StringBuilder();
            query.Append("SELECT [Date], SUM([Minutes]) [Minutes], SUM([MaxMinutes]) [MaxMinutes]");
            query.Append(" FROM [HourlySummaries]");
            query.Append(" WHERE CONVERT(DATE, [Date]) = CONVERT(DATE, @Date)");
            if (!string.IsNullOrEmpty(branch))
            {
                query.Append(" AND [Branch] = @Branch");
            }
            query.Append(" GROUP BY [Date]");

            try
            {
                DbConnection.Open();
                var result =
                    await _dbConnection.QueryAsync<UsageSummary>(query.ToString(),
                            new { Date = date, Branch = branch })
                        .ConfigureAwait(false);
                return result.ToList();
            }
            finally
            {
                DbConnection.Close();
            }
        }
    }
}
