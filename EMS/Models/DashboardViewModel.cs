namespace EMS.Models
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalDesignations { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public decimal TotalMonthlySalary { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int PendingLevelRequests { get; set; }
        public List<string> DepartmentNames { get; set; } = new();
        public List<int> DepartmentEmployeeCounts { get; set; } = new();
        public List<Employee> RecentEmployees { get; set; } = new();
    }
}
