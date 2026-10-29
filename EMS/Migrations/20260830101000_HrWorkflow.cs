using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.Migrations
{
    public partial class HrWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "EmployeeId", table: "Users", type: "int", nullable: true);
            migrationBuilder.CreateTable(name: "Levels", columns: table => new
            {
                LevelId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                LevelName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
            }, constraints: table => table.PrimaryKey("PK_Levels", x => x.LevelId));
            migrationBuilder.CreateTable(name: "LeaveRequests", columns: table => new
            {
                LeaveRequestId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                LeaveType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_LeaveRequests", x => x.LeaveRequestId); table.ForeignKey("FK_LeaveRequests_Employees_EmployeeId", x => x.EmployeeId, "Employees", "EmployeeId", onDelete: ReferentialAction.Cascade); });
            migrationBuilder.CreateTable(name: "Salaries", columns: table => new
            {
                SalaryId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Allowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Deductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Month = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_Salaries", x => x.SalaryId); table.ForeignKey("FK_Salaries_Employees_EmployeeId", x => x.EmployeeId, "Employees", "EmployeeId", onDelete: ReferentialAction.Cascade); });
            migrationBuilder.CreateTable(name: "EmployeeLevels", columns: table => new
            {
                EmployeeLevelId = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                LevelId = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_EmployeeLevels", x => x.EmployeeLevelId); table.ForeignKey("FK_EmployeeLevels_Employees_EmployeeId", x => x.EmployeeId, "Employees", "EmployeeId", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_EmployeeLevels_Levels_LevelId", x => x.LevelId, "Levels", "LevelId", onDelete: ReferentialAction.Restrict); });
            migrationBuilder.CreateIndex(name: "IX_Users_EmployeeId", table: "Users", column: "EmployeeId", unique: true, filter: "[EmployeeId] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_LeaveRequests_EmployeeId", table: "LeaveRequests", column: "EmployeeId");
            migrationBuilder.CreateIndex(name: "IX_Salaries_EmployeeId", table: "Salaries", column: "EmployeeId");
            migrationBuilder.CreateIndex(name: "IX_EmployeeLevels_EmployeeId", table: "EmployeeLevels", column: "EmployeeId");
            migrationBuilder.CreateIndex(name: "IX_EmployeeLevels_LevelId", table: "EmployeeLevels", column: "LevelId");
            migrationBuilder.AddForeignKey(name: "FK_Users_Employees_EmployeeId", table: "Users", column: "EmployeeId", principalTable: "Employees", principalColumn: "EmployeeId", onDelete: ReferentialAction.SetNull);
            migrationBuilder.InsertData("Levels", new[] { "LevelId", "Description", "LevelName" }, new object[,] { {1,"Entry level","Level 1 - Junior"},{2,"Associate level","Level 2 - Associate"},{3,"Senior level","Level 3 - Senior"},{4,"Team lead level","Level 4 - Lead"},{5,"Management level","Level 5 - Manager"} });
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Users_Employees_EmployeeId", "Users");
            migrationBuilder.DropTable("EmployeeLevels"); migrationBuilder.DropTable("LeaveRequests"); migrationBuilder.DropTable("Salaries"); migrationBuilder.DropTable("Levels");
            migrationBuilder.DropIndex("IX_Users_EmployeeId", "Users");
            migrationBuilder.DropColumn("EmployeeId", "Users");
        }
    }
}
