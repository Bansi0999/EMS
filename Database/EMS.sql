/* ==========================================================================
   Employee Management System - SQL Server Database Script
   Creates the database, all tables, relationships, and seed data.
   ========================================================================== */

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EMS')
BEGIN
    CREATE DATABASE EMS;
END
GO

USE EMS;
GO

/* Drop existing tables in dependency order */
IF OBJECT_ID('dbo.EmployeeLevels', 'U') IS NOT NULL DROP TABLE dbo.EmployeeLevels;
IF OBJECT_ID('dbo.LeaveRequests', 'U') IS NOT NULL DROP TABLE dbo.LeaveRequests;
IF OBJECT_ID('dbo.Salaries', 'U') IS NOT NULL DROP TABLE dbo.Salaries;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DROP TABLE dbo.Employees;
IF OBJECT_ID('dbo.Designations', 'U') IS NOT NULL DROP TABLE dbo.Designations;
IF OBJECT_ID('dbo.Levels', 'U') IS NOT NULL DROP TABLE dbo.Levels;
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

/* ==========================================================================
   Table: Departments
   ========================================================================== */
CREATE TABLE dbo.Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NULL,
    CONSTRAINT UQ_Departments_Name UNIQUE (DepartmentName)
);
GO

/* ==========================================================================
   Table: Designations
   ========================================================================== */
CREATE TABLE dbo.Designations (
    DesignationId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    DepartmentId INT NOT NULL,
    CONSTRAINT FK_Designations_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId) ON DELETE CASCADE
);
GO

/* ==========================================================================
   Table: Employees
   ========================================================================== */
CREATE TABLE dbo.Employees (
    EmployeeId      INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode    NVARCHAR(20) NOT NULL,
    FirstName       NVARCHAR(50) NOT NULL,
    LastName        NVARCHAR(50) NOT NULL,
    Email           NVARCHAR(100) NOT NULL,
    Phone           NVARCHAR(20) NOT NULL,
    Address         NVARCHAR(250) NULL,
    Gender          NVARCHAR(10) NOT NULL,
    DateOfBirth     DATE NOT NULL,
    HireDate        DATE NOT NULL,
    Salary          DECIMAL(18,2) NOT NULL,
    DepartmentId    INT NOT NULL,
    DesignationId   INT NOT NULL,
    PhotoPath       NVARCHAR(300) NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Employees_Code UNIQUE (EmployeeCode),
    CONSTRAINT UQ_Employees_Email UNIQUE (Email),
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(DepartmentId),
    CONSTRAINT FK_Employees_Designations FOREIGN KEY (DesignationId)
        REFERENCES dbo.Designations(DesignationId)
);
GO

/* ==========================================================================
   Table: Levels
   ========================================================================== */
CREATE TABLE dbo.Levels (
    LevelId       INT IDENTITY(1,1) PRIMARY KEY,
    LevelName     NVARCHAR(50) NOT NULL,
    Description   NVARCHAR(250) NULL
);
GO

/* ==========================================================================
   Table: Salaries
   ========================================================================== */
CREATE TABLE dbo.Salaries (
    SalaryId       INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId     INT NOT NULL,
    BasicSalary    DECIMAL(18,2) NOT NULL,
    Allowances     DECIMAL(18,2) NOT NULL DEFAULT 0,
    Deductions     DECIMAL(18,2) NOT NULL DEFAULT 0,
    Month          NVARCHAR(20) NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedDate    DATETIME2 NOT NULL DEFAULT GETDATE(),
    ApprovedDate   DATETIME2 NULL,
    CONSTRAINT FK_Salaries_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE CASCADE
);
GO

/* ==========================================================================
   Table: LeaveRequests
   ========================================================================== */
CREATE TABLE dbo.LeaveRequests (
    LeaveRequestId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId     INT NOT NULL,
    LeaveType      NVARCHAR(40) NOT NULL,
    FromDate       DATE NOT NULL,
    ToDate         DATE NOT NULL,
    Reason         NVARCHAR(500) NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    AppliedDate    DATETIME2 NOT NULL DEFAULT GETDATE(),
    ActionDate     DATETIME2 NULL,
    CONSTRAINT FK_LeaveRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE CASCADE
);
GO

/* ==========================================================================
   Table: EmployeeLevels
   ========================================================================== */
CREATE TABLE dbo.EmployeeLevels (
    EmployeeLevelId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId      INT NOT NULL,
    LevelId         INT NOT NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    Remarks         NVARCHAR(500) NULL,
    RequestedDate   DATETIME2 NOT NULL DEFAULT GETDATE(),
    ApprovedDate    DATETIME2 NULL,
    CONSTRAINT FK_EmployeeLevels_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE CASCADE,
    CONSTRAINT FK_EmployeeLevels_Levels FOREIGN KEY (LevelId) REFERENCES dbo.Levels(LevelId)
);
GO

/* ==========================================================================
   Table: Users  (application login accounts)
   ========================================================================== */
CREATE TABLE dbo.Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL,
    PasswordHash    NVARCHAR(256) NOT NULL,
    FullName        NVARCHAR(100) NOT NULL,
    Role            NVARCHAR(20) NOT NULL DEFAULT 'Admin',
    EmployeeId      INT NULL,
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);
GO

/* ==========================================================================
   Seed Data: Departments
   ========================================================================== */
INSERT INTO dbo.Departments (DepartmentName, Description) VALUES
('Human Resources', 'Handles recruitment, employee relations and policies'),
('Information Technology', 'Manages software, hardware and IT infrastructure'),
('Finance', 'Handles accounting, budgeting and payroll'),
('Marketing', 'Handles branding, advertising and market research');
GO

/* ==========================================================================
   Seed Data: Designations
   ========================================================================== */
INSERT INTO dbo.Designations (Title, DepartmentId) VALUES
('HR Manager', 1),
('HR Executive', 1),
('Software Engineer', 2),
('Senior Software Engineer', 2),
('System Administrator', 2),
('Accountant', 3),
('Finance Manager', 3),
('Marketing Executive', 4);
GO

/* ==========================================================================
   Seed Data: Default Admin User
   Username: admin   |   Password: Admin@123   (SHA-256 hashed)
   ========================================================================== */
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role) VALUES
('admin', 'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7', 'System Administrator', 'Admin');
GO

/* ==========================================================================
   Seed Data: Sample Employees (optional - for demo purposes)
   ========================================================================== */
INSERT INTO dbo.Employees (EmployeeCode, FirstName, LastName, Email, Phone, Address, Gender, DateOfBirth, HireDate, Salary, DepartmentId, DesignationId, IsActive) VALUES
('EMP0001', 'Aarav',  'Shah',    'aarav.shah@example.com',   '9876543210', 'Rajkot, Gujarat',   'Male',   '1995-04-12', '2021-06-01', 45000.00, 2, 3, 1),
('EMP0002', 'Priya',  'Mehta',   'priya.mehta@example.com',  '9876543211', 'Ahmedabad, Gujarat','Female', '1993-11-23', '2020-02-15', 65000.00, 2, 4, 1),
('EMP0003', 'Rohan',  'Patel',   'rohan.patel@example.com',  '9876543212', 'Surat, Gujarat',    'Male',   '1990-01-05', '2019-08-20', 55000.00, 3, 7, 1),
('EMP0004', 'Ishita', 'Joshi',   'ishita.joshi@example.com', '9876543213', 'Vadodara, Gujarat', 'Female', '1996-07-30', '2022-01-10', 32000.00, 1, 2, 1),
('EMP0005', 'Kunal',  'Desai',   'kunal.desai@example.com',  '9876543214', 'Rajkot, Gujarat',   'Male',   '1994-09-18', '2021-11-05', 38000.00, 4, 8, 0);
GO

ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(EmployeeId) ON DELETE SET NULL;
CREATE UNIQUE INDEX IX_Users_EmployeeId ON dbo.Users(EmployeeId) WHERE EmployeeId IS NOT NULL;
GO

/* ==========================================================================
   Seed Employee Login Accounts
   Username: EmployeeCode | Password: Employee@123
   ========================================================================== */
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, EmployeeId) VALUES
('EMP0001', 'b4bd29480ab196faa782e0d4ecd10c2f4212814105227e5f7992f5bf4b212a64', 'Aarav Shah', 'Employee', 1),
('EMP0002', 'b4bd29480ab196faa782e0d4ecd10c2f4212814105227e5f7992f5bf4b212a64', 'Priya Mehta', 'Employee', 2),
('EMP0003', 'b4bd29480ab196faa782e0d4ecd10c2f4212814105227e5f7992f5bf4b212a64', 'Rohan Patel', 'Employee', 3),
('EMP0004', 'b4bd29480ab196faa782e0d4ecd10c2f4212814105227e5f7992f5bf4b212a64', 'Ishita Joshi', 'Employee', 4),
('EMP0005', 'b4bd29480ab196faa782e0d4ecd10c2f4212814105227e5f7992f5bf4b212a64', 'Kunal Desai', 'Employee', 5);
GO

INSERT INTO dbo.Levels (LevelName, Description) VALUES
('Level 1 - Junior', 'Entry level'),
('Level 2 - Associate', 'Associate level'),
('Level 3 - Senior', 'Senior level'),
('Level 4 - Lead', 'Team lead level'),
('Level 5 - Manager', 'Management level');
GO

INSERT INTO dbo.Salaries (EmployeeId, BasicSalary, Allowances, Deductions, Month, Status, ApprovedDate) VALUES
(1, 40000, 5000, 2000, '2026-08', 'Approved', GETDATE()),
(2, 55000, 7000, 3000, '2026-08', 'Approved', GETDATE()),
(3, 48000, 6000, 2500, '2026-08', 'Pending', NULL),
(4, 30000, 4000, 1500, '2026-08', 'Pending', NULL);
GO

INSERT INTO dbo.EmployeeLevels (EmployeeId, LevelId, Status, ApprovedDate) VALUES
(1, 2, 'Approved', GETDATE()),
(2, 3, 'Approved', GETDATE()),
(3, 2, 'Pending', NULL);
GO

PRINT 'EMS created and seeded successfully.';
GO
