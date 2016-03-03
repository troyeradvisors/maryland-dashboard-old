using System;

namespace Dashboard.Web
{
    public partial class Description
    {
        public DateTime FiscalStartDate { get { return FiscalEndDate.Subtract(TimeSpan.FromDays(OperatingDays)); }}
        public string FiscalDateRange { get { return FiscalStartDate.ToString("MM/dd/yy") + " - " + FiscalEndDate.ToString("MM/dd/yy"); } }
    }
}
