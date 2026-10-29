using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using EMS.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "EMS.Auth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Non-destructive database upgrade for existing EMS databases.
// This keeps existing data and adds the Employee/Salary/Leave/Level workflow
// schema when the database was created with an older version of the project.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EnsureEmsSchemaAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();


static async Task EnsureEmsSchemaAsync(ApplicationDbContext db)
{
    // Add the column in its own SQL batch. SQL Server compiles a batch before
    // executing ALTER TABLE, so later statements in the same batch must not
    // reference a column that is being added here.
    const string addEmployeeIdColumnSql = @"
IF COL_LENGTH('dbo.Users','EmployeeId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD EmployeeId INT NULL;
END;
";

    await db.Database.ExecuteSqlRawAsync(addEmployeeIdColumnSql);

    const string sql = @"
IF OBJECT_ID('dbo.Levels','U') IS NULL
BEGIN
    CREATE TABLE dbo.Levels (
        LevelId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LevelName NVARCHAR(50) NOT NULL,
        Description NVARCHAR(250) NULL
    );
END;

IF OBJECT_ID('dbo.Salaries','U') IS NULL
BEGIN
    CREATE TABLE dbo.Salaries (
        SalaryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeId INT NOT NULL,
        BasicSalary DECIMAL(18,2) NOT NULL,
        Allowances DECIMAL(18,2) NOT NULL DEFAULT 0,
        Deductions DECIMAL(18,2) NOT NULL DEFAULT 0,
        Month NVARCHAR(20) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        ApprovedDate DATETIME2 NULL
    );
END;

IF OBJECT_ID('dbo.LeaveRequests','U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveRequests (
        LeaveRequestId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeId INT NOT NULL,
        LeaveType NVARCHAR(40) NOT NULL,
        FromDate DATE NOT NULL,
        ToDate DATE NOT NULL,
        Reason NVARCHAR(500) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        AppliedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        ActionDate DATETIME2 NULL
    );
END;

IF OBJECT_ID('dbo.EmployeeLevels','U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeLevels (
        EmployeeLevelId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeId INT NOT NULL,
        LevelId INT NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        Remarks NVARCHAR(500) NULL,
        RequestedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        ApprovedDate DATETIME2 NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Employees_EmployeeId')
   AND COL_LENGTH('dbo.Users','EmployeeId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Employees_EmployeeId
        FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE SET NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_EmployeeId' AND object_id = OBJECT_ID('dbo.Users'))
   AND COL_LENGTH('dbo.Users','EmployeeId') IS NOT NULL
BEGIN
    CREATE UNIQUE INDEX IX_Users_EmployeeId ON dbo.Users(EmployeeId) WHERE EmployeeId IS NOT NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Salaries_Employees_EmployeeId')
   AND OBJECT_ID('dbo.Salaries','U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Salaries ADD CONSTRAINT FK_Salaries_Employees_EmployeeId
        FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE CASCADE;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LeaveRequests_Employees_EmployeeId')
   AND OBJECT_ID('dbo.LeaveRequests','U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.LeaveRequests ADD CONSTRAINT FK_LeaveRequests_Employees_EmployeeId
        FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE CASCADE;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeLevels_Employees_EmployeeId')
   AND OBJECT_ID('dbo.EmployeeLevels','U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.EmployeeLevels ADD CONSTRAINT FK_EmployeeLevels_Employees_EmployeeId
        FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE CASCADE;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EmployeeLevels_Levels_LevelId')
   AND OBJECT_ID('dbo.EmployeeLevels','U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.EmployeeLevels ADD CONSTRAINT FK_EmployeeLevels_Levels_LevelId
        FOREIGN KEY (LevelId) REFERENCES dbo.Levels(LevelId) ON DELETE NO ACTION;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Levels)
BEGIN
    SET IDENTITY_INSERT dbo.Levels ON;
    INSERT INTO dbo.Levels (LevelId, LevelName, Description) VALUES
    (1,'Level 1 - Junior','Entry level'),
    (2,'Level 2 - Associate','Associate level'),
    (3,'Level 3 - Senior','Senior level'),
    (4,'Level 4 - Lead','Team lead level'),
    (5,'Level 5 - Manager','Management level');
    SET IDENTITY_INSERT dbo.Levels OFF;
END;


";

    await db.Database.ExecuteSqlRawAsync(sql);

    // Run employee-account seeding in a separate SQL batch. SQL Server compiles
    // a whole batch before executing ALTER TABLE statements, so referencing
    // Users.EmployeeId later in the same batch can produce "Invalid column name"
    // even though the column was just added.
    const string seedEmployeeUsersSql = @"
IF COL_LENGTH('dbo.Users','EmployeeId') IS NOT NULL
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, EmployeeId)
    SELECT e.EmployeeCode,
           'b4bd29480ab196faa782e0d4ecd10c2f4212814105227e5f7992f5bf4b212a64',
           CONCAT(e.FirstName, ' ', e.LastName),
           'Employee', e.EmployeeId
    FROM dbo.Employees e
    WHERE e.IsActive = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Username = e.EmployeeCode)
      AND NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.EmployeeId = e.EmployeeId);
END;
";

    await db.Database.ExecuteSqlRawAsync(seedEmployeeUsersSql);
}
