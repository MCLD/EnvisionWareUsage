using Dapper;
using EnvisionwareLoader.Data.EnvisionwareModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EnvisionwareLoader.Data
{
    public class EnvisionwareContext
    {
        private const string EarliestDateQuery = "SELECT MIN(pcrDateTime) FROM tbluserpcrdetail";
        private const string LatestDateQuery = "SELECT MAX(pcrDateTime) FROM tbluserpcrdetail";
        private const string GetDayQuery = "SELECT pcrKey, pcrMinutesUsed, pcrPC, pcrDateTime, pcrBranch, pcrArea FROM tbluserpcrdetail WHERE pcrStatus = 512 AND pcrDateTime >= @StartDate AND pcrDateTime < @endDate";

        // The MySQL connection is not created in this object so this object doesn't dispose of it
        private readonly IDbConnection _mysql;
        private readonly ILogger _log;

        public EnvisionwareContext(ILogger logger, IDbConnection mysqlConnection)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _mysql = mysqlConnection ?? throw new ArgumentNullException(nameof(mysqlConnection));
        }

        public async Task<DateTime> GetEarliestDateAsync()
        {
            try
            {
                _mysql.Open();
                var result = await _mysql
                    .QueryAsync<DateTime>(EarliestDateQuery)
                    .ConfigureAwait(false);
                return result.Single();
            }
            finally
            {
                if (_mysql.State == ConnectionState.Open)
                {
                    _mysql.Close();
                }
            }
        }

        public async Task<DateTime> GetLatestDateAsync()
        {
            try
            {
                _mysql.Open();
                var result = await _mysql
                    .QueryAsync<DateTime>(LatestDateQuery)
                    .ConfigureAwait(false);
                return result.Single();
            }
            finally
            {
                if (_mysql.State == ConnectionState.Open)
                {
                    _mysql.Close();
                }
            }
        }

        public async Task<IEnumerable<PCRDetail>> GetDayDataAsync(DateTime date)
        {
            var startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            var plusOne = startDate.AddDays(1);
            var endDate = new DateTime(plusOne.Year, plusOne.Month, plusOne.Day, 0, 0, 0);

            try
            {
                _mysql.Open();
                var result = await _mysql.QueryAsync<PCRDetail>(GetDayQuery,
                    new { StartDate = startDate, EndDate = endDate })
                    .ConfigureAwait(false);
                return result;
            }
            finally
            {
                if (_mysql.State == ConnectionState.Open)
                {
                    _mysql.Close();
                }
            }
        }
    }
}
