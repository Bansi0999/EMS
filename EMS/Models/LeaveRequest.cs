using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.Models
{
    public class LeaveRequest
    {
        public int LeaveRequestId { get; set; }
        [Required] public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")] public Employee? Employee { get; set; }
        [Required, StringLength(40), Display(Name="Leave Type")] public string LeaveType { get; set; } = "Casual Leave";
        [Required, DataType(DataType.Date), Display(Name="From Date")] public DateTime FromDate { get; set; } = DateTime.Today;
        [Required, DataType(DataType.Date), Display(Name="To Date")] public DateTime ToDate { get; set; } = DateTime.Today;
        [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
        [Required, StringLength(20)] public string Status { get; set; } = "Pending";
        public DateTime AppliedDate { get; set; } = DateTime.Now;
        public DateTime? ActionDate { get; set; }
    }
}
