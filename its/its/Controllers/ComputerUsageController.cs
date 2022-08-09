using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using its.Controllers.Abstract;
using its.Data;
using its.Filters;
using its.Models;
using its.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace its.Controllers
{
    [Route("")]
    [Route("[controller]")]
    [ServiceFilter(typeof(CachedDataFilter), Order = 1)]
    public class ComputerUsageController : BaseController
    {
        private const int DaysOneYear = 7 * 52;
        private const int DaysTwoYears = 7 * 104;

        private readonly ILogger _logger;
        private readonly IDistributedCache _cache;
        private readonly ComputerUsageContext _context;

        public ComputerUsageController(ILogger<ComputerUsageController> logger,
            IDistributedCache cache,
            ComputerUsageContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [Route("")]
        [Route("[action]")]
        [Route("[action]/{year:int}")]
        [Route("[action]/{branch}")]
        [Route("[action]/{year:int}/{branch}")]
        public async Task<IActionResult> Summary(int? year, string branch)
        {
            DateTime latestData = DateTime.Now;
            string validBranch = null;

            var branchList = (IDictionary<string, string>)HttpContext.Items[Keys.Item.BranchList];
            if (branchList != null
                && !string.IsNullOrEmpty(branch)
                && branchList.ContainsKey(branch))
            {
                validBranch = branch.Trim();
            }

            if (year != null)
            {
                try
                {
                    latestData = new DateTime((int)year, 12, 1);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Invalid date range requested: {year}, using {latestData}");
                }
            }
            else
            {
                latestData = await _context.GetLatestDailyDateAsync(branch).ConfigureAwait(false)
                                ?? DateTime.Now;
            }

            var firstOfMonth = new DateTime(latestData.Year, latestData.Month, 1);

            var viewModel = new SummaryViewModel
            {
                StartDate = firstOfMonth.AddMonths(-11),
                EndDate = firstOfMonth.AddMonths(1).AddDays(-1),
                Branch = validBranch
            };

            // get all data for the last 12 months
            IList<UsageSummary> usage = null;

            if (string.IsNullOrEmpty(branch))
            {
                usage = await _context
                    .GetDailyUsageAsync(viewModel.StartDate, viewModel.EndDate)
                    .ConfigureAwait(false);
            }
            else
            {
                usage = await _context
                    .GetDailyUsageAsync(viewModel.StartDate, viewModel.EndDate, branch)
                    .ConfigureAwait(false);
            }

            if (usage.Count > 0)
            {
                var minMinutes = usage.Min(_ => _.Minutes);
                // if we have a branch we can use MaxMinutes, otherwise compute it
                var maxMinutes = !string.IsNullOrEmpty(branch)
                    ? usage.Last().MaxMinutes
                    : usage.Max(_ => _.Minutes);

                viewModel.Stats = usage.Select(_ => new DisplayDailySummary(_, minMinutes, maxMinutes));
            }

            return View(viewModel);
        }

        [Route("[action]/{date:int}")]
        [Route("[action]/{branch}")]
        [Route("[action]/{date:int}/{branch}")]
        public async Task<IActionResult> Detail(int date, string branch)
        {
            var viewModel = new DetailViewModel();

            var branchList = (IDictionary<string, string>)HttpContext.Items[Keys.Item.BranchList];
            if (branchList != null
                && !string.IsNullOrEmpty(branch)
                && branchList.ContainsKey(branch))
            {
                viewModel.Branch = branch.Trim();
                viewModel.BranchName = branchList[branch];
            }

            DateTime detailDate = DateTime.Now.AddDays(-1).Date;

            var dateString = date.ToString();
            if (!string.IsNullOrEmpty(dateString))
            {
                try
                {
                    detailDate = DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Invalid date range requested: {date}, using {detailDate}");
                }
            }

            viewModel.Date = detailDate;

            viewModel.HistoricalDate = detailDate.AddDays(0 - DaysOneYear);
            viewModel.Historical2Date = detailDate.AddDays(0 - DaysTwoYears);

            long minimumGlobalHour = long.MaxValue;
            long maximumGlobalHour = 0;

            // get all data
            var globalHourly = await _context.GetHourlyUsageAsync(detailDate).ConfigureAwait(false);

            if (globalHourly.Count > 0)
            {
                minimumGlobalHour = Math.Min(minimumGlobalHour,
                    globalHourly.Min(_ => _.Date.Ticks));
                maximumGlobalHour = Math.Max(maximumGlobalHour,
                    globalHourly.Max(_ => _.Date.Ticks));
            }

            viewModel.GlobalStats = globalHourly.Select(_ => new DisplayHourlySummary(_));

            // get all past data
            var globalHistorical = await _context
                .GetHourlyUsageAsync(viewModel.HistoricalDate)
                .ConfigureAwait(false);

            if (globalHistorical.Count > 0)
            {
                minimumGlobalHour = Math.Min(minimumGlobalHour,
                    globalHistorical.Min(_ => _.Date.AddDays(DaysOneYear).Ticks));
                maximumGlobalHour = Math.Max(maximumGlobalHour,
                    globalHistorical.Max(_ => _.Date.AddDays(DaysOneYear).Ticks));
            }

            viewModel.GlobalHistorical = globalHistorical.Select(_ => new DisplayHourlySummary(_));

            var globalHistorical2 = await _context
                .GetHourlyUsageAsync(viewModel.Historical2Date)
                .ConfigureAwait(false);

            if (globalHistorical2.Count > 0)
            {
                minimumGlobalHour = Math.Min(minimumGlobalHour,
                    globalHistorical2.Min(_ => _.Date.AddDays(DaysTwoYears).Ticks));
                maximumGlobalHour = Math.Max(maximumGlobalHour,
                    globalHistorical2.Max(_ => _.Date.AddDays(DaysTwoYears).Ticks));
            }

            viewModel.GlobalHistorical2 = globalHistorical2.Select(_ => new DisplayHourlySummary(_));

            if (minimumGlobalHour != long.MaxValue)
            {
                viewModel.MinimumGlobalHour = new DateTime(minimumGlobalHour);
            }

            if (maximumGlobalHour != 0)
            {
                if (maximumGlobalHour != minimumGlobalHour)
                {
                    viewModel.MaximumGlobalHour = new DateTime(maximumGlobalHour);
                }
                else
                {
                    viewModel.MaximumGlobalHour = viewModel.MinimumGlobalHour.AddHours(1);
                }
            }

            // get branch data
            if (!string.IsNullOrEmpty(viewModel.Branch))
            {
                long minimumBranchHour = long.MaxValue;
                long maximumBranchHour = 0;

                var hourly = await _context
                    .GetHourlyUsageAsync(detailDate, viewModel.Branch)
                    .ConfigureAwait(false);

                if (hourly.Count > 0)
                {
                    minimumBranchHour = Math.Min(minimumBranchHour,
                        hourly.Min(_ => _.Date.Ticks));
                    maximumBranchHour = Math.Max(maximumBranchHour,
                        hourly.Max(_ => _.Date.Ticks));
                }

                viewModel.Stats = hourly.Select(_ => new DisplayHourlySummary(_));

                // get past branch data

                var historical = await _context
                    .GetHourlyUsageAsync(viewModel.HistoricalDate, viewModel.Branch)
                    .ConfigureAwait(false);

                if (historical.Count > 0)
                {
                    minimumBranchHour = Math.Min(minimumBranchHour,
                        historical.Min(_ => _.Date.AddDays(DaysOneYear).Ticks));
                    maximumBranchHour = Math.Max(maximumBranchHour,
                        historical.Max(_ => _.Date.AddDays(DaysOneYear).Ticks));
                }

                viewModel.Historical = historical.Select(_ => new DisplayHourlySummary(_));

                var historical2 = await _context
                    .GetHourlyUsageAsync(viewModel.Historical2Date, viewModel.Branch)
                    .ConfigureAwait(false);

                if (historical2.Count > 0)
                {
                    minimumBranchHour = Math.Min(minimumBranchHour,
                        historical2.Min(_ => _.Date.AddDays(DaysTwoYears).Ticks));
                    maximumBranchHour = Math.Max(maximumBranchHour,
                        historical2.Max(_ => _.Date.AddDays(DaysTwoYears).Ticks));
                }

                viewModel.Historical2 = historical2.Select(_ => new DisplayHourlySummary(_));

                if (minimumBranchHour != long.MaxValue)
                {
                    viewModel.MinimumBranchHour = new DateTime(minimumBranchHour);
                }

                if (maximumBranchHour != 0)
                {
                    if (maximumBranchHour != minimumBranchHour)
                    {
                        viewModel.MaximumBranchHour = new DateTime(maximumBranchHour);
                    }
                    else
                    {
                        viewModel.MaximumBranchHour = viewModel.MinimumBranchHour.AddHours(1);
                    }
                }
            }

            return View(viewModel);
        }

        [Route("[action]")]
        public IActionResult Unauthorized(string returnUrl)
        {
            return View(new UnauthorizedViewModel
            {
                ReturnUrl = returnUrl,
                Username = CurrentUsername
            });
        }

        [Route("[action]")]
        public IActionResult Authenticate(string returnUrl)
        {
            // by the time we get here the user is probably authenticated - if so we can take them
            // back to their initial destination
            if (HttpContext.Items[Keys.Item.Username] != null)
            {
                return Redirect(returnUrl);
            }

            TempData[Keys.TempData.AlertWarning]
                = $"Could not authenticate you for access to {returnUrl}.";
            return RedirectToAction(nameof(Summary));
        }

        [Route("[action]")]
        public async Task<IActionResult> ClearCachedBranches()
        {
            await _cache.RemoveAsync(Keys.Cache.BranchList).ConfigureAwait(false);

            return RedirectToAction(nameof(Summary));
        }
    }
}
