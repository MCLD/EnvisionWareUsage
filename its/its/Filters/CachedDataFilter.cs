using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using its.Controllers;
using its.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace its.Filters
{
    public class CachedDataFilter : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger _logger;
        private readonly IDistributedCache _cache;
        private readonly ComputerUsageContext _context;

        public CachedDataFilter(ILogger<CachedDataFilter> logger,
            IDistributedCache cache,
            ComputerUsageContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(_cache));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            var branchesJson = await _cache.GetStringAsync(Keys.Cache.BranchList)
                .ConfigureAwait(false);
            IDictionary<string, string> branches = null;

            if (string.IsNullOrEmpty(branchesJson))
            {
                branches = await _context.GetBranchesAsync().ConfigureAwait(false);
                branchesJson = JsonConvert.SerializeObject(branches);
                await _cache.SetStringAsync(Keys.Cache.BranchList,
                    branchesJson,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = new TimeSpan(8, 0, 0)
                    })
                    .ConfigureAwait(false);
            }
            else
            {
                branches = JsonConvert.DeserializeObject<IDictionary<string, string>>(branchesJson);
            }

            context.HttpContext.Items.Add(Keys.Item.BranchList, branches);

            var selectedBranch = (string)context.RouteData.Values["branch"];
            if (!string.IsNullOrEmpty(selectedBranch) && branches.ContainsKey(selectedBranch))
            {
                context.HttpContext.Items.Add(Keys.Item.SelectedBranchName,
                    branches[selectedBranch]);
                context.HttpContext.Items.Add(Keys.Item.CurrentBranch, selectedBranch);
                context.HttpContext.Items.Add(Keys.Item.CurrentBranchActive, "active");
            }
            else
            {
                context.HttpContext.Items.Add(Keys.Item.SelectedBranchName, "All Branches");
            }

            string usageController = nameof(ComputerUsageController).Substring(0,
                nameof(ComputerUsageController).Length - "Controller".Length);

            var onSummaryPage = (string)context.RouteData.Values["controller"] == usageController
                                && (string)context.RouteData.Values["action"]
                                == nameof(ComputerUsageController.Summary);

            if (onSummaryPage)
            {
                context.HttpContext.Items.Add(Keys.Item.OnSummaryPage, true);
            }

            var selectedYear = (string)context.RouteData.Values["year"];
            if (!string.IsNullOrEmpty(selectedYear))
            {
                var year = new DateTime(int.Parse(selectedYear), 12, 31);
                context.HttpContext.Items.Add(Keys.Item.CurrentYear, selectedYear);
                context.HttpContext.Items.Add(Keys.Item.DateNext,
                    year.AddYears(1).ToString("yyyy"));
                context.HttpContext.Items.Add(Keys.Item.DatePrevious,
                    year.AddYears(-1).ToString("yyyy"));
            }
            else
            {
                if (onSummaryPage)
                {
                    context.HttpContext.Items.Add(Keys.Item.YtdActive, "active");
                }
            }

            var selectedDate = (string)context.RouteData.Values["date"];
            if (!string.IsNullOrEmpty(selectedDate))
            {
                var date = DateTime.ParseExact(selectedDate, "yyyyMMdd",
                    CultureInfo.InvariantCulture);
                context.HttpContext.Items.Add(Keys.Item.CurrentDate, date.ToShortDateString());
                context.HttpContext.Items.Add(Keys.Item.DateNext,
                    date.AddDays(1).ToString("yyyyMMdd"));
                context.HttpContext.Items.Add(Keys.Item.DatePrevious,
                    date.AddDays(-1).ToString("yyyyMMdd"));
            }

            await next().ConfigureAwait(false);
        }
    }
}
