using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs.Report;

namespace Timesheet.Application.Interfaces
{
    public interface IReportService
    {
        Task<List<EmployeeHoursSummaryDto>> GetEmployeeHoursSummary(DateOnly? startDate = null, DateOnly? endDate = null);
        Task<List<ProjectHoursSummaryDto>> GetProjectHoursSummary(DateOnly? startDate = null, DateOnly? endDate = null);
        Task<List<BillableHoursSummaryDto>> GetBillableHoursSummary(DateOnly? startDate = null, DateOnly? endDate = null);

    }
}
