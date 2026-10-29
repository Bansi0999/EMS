# EMS - Employee Management System

## Run
1. Open `EMS.sln` in Visual Studio.
2. Make sure SQL Server LocalDB `(localdb)\\MSSQLLocalDB` is running.
3. The connection string uses database `EMS`.
4. Run the project. On startup, EMS performs a **non-destructive schema upgrade** for older EMS databases. It does not drop your existing data.
5. Employee login accounts are created automatically for active employees that do not already have an account.

## Login
- Admin: `admin` / `Admin@123`
- Employee: `EMP0001` / `Employee@123`

## Role-based access
- `Admin` role -> Admin Dashboard and management/approval pages.
- `Employee` role -> Employee Dashboard, My Salary, Apply Leave, My Leaves, My Level.


## Final college-ready enhancements
- Role-based Admin/Employee access with inactive employee login protection.
- Non-destructive startup schema upgrade for older EMS databases.
- Employee photo upload validation (supported image formats, 2 MB limit).
- Employee data validation for dates and department/designation consistency.
- Leave overlap, past-date and 30-day duration validation.
- Level request duplicate/current-level validation.
- Salary duplicate-month and non-negative net-salary validation.
- Change Password module for authenticated users.
- Dashboard pending Leave and Level approval counters with quick links.
- Confirmation prompts for approval/rejection actions.

### Demo accounts
- Admin: `admin` / `Admin@123`
- Employee: `EMP0001` / `Employee@123`

### Database
- SQL Server LocalDB: `(localdb)\MSSQLLocalDB`
- Database: `EMS`
- Connection string is stored in `appsettings.json`.
