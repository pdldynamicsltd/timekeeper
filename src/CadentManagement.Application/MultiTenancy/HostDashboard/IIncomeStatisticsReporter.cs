using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CadentManagement.MultiTenancy.HostDashboard.Dto;

namespace CadentManagement.MultiTenancy.HostDashboard;

public interface IIncomeStatisticsService
{
    Task<List<IncomeStastistic>> GetIncomeStatisticsData(DateTime startDate, DateTime endDate,
        ChartDateInterval dateInterval);
}
