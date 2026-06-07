# Office Management System

A web-based office management system built with ASP.NET Core MVC and MySQL. It covers the day-to-day stuff — employees, attendance, leave requests, payroll, projects, and meetings — all behind a simple login with admin and employee roles.

---

## Features

- **Authentication** — Session-based login with role separation (Admin / Employee)
- **Employee Management** — Add, edit, and remove employee records (admin only)
- **Attendance Tracking** — View clock-in/out records and attendance status
- **Leave Requests** — Employees submit requests; admins approve or reject them
- **Payroll** — Process payroll via stored procedure; view payment history
- **Projects** — Create and manage projects, track status (Ongoing / Completed)
- **Meetings** — Schedule and manage meetings
- **Dashboard** — At-a-glance attendance summary (present, absent, late, pending leaves)

---

## Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 10)
- **Database:** MySQL
- **ORM:** Raw ADO.NET with `MySql.Data` (v9.7.0)
- **Frontend:** Bootstrap 5, jQuery
- **Session:** ASP.NET Core session middleware

---

## Prerequisites

Make sure you have these installed before setting up:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL Server (5.7+ or 8.x)
- A MySQL client (Workbench, phpMyAdmin, or the CLI)

---

## Setup & Installation

### 1. Clone the repo

```bash
git clone https://github.com/your-username/OfficeManagementSystem.git
cd OfficeManagementSystem
```

### 2. Create the database

Open your MySQL client and run the SQL from the next section to create all the tables, views, and stored procedures.

### 3. Configure the connection string

Open `appsettings.json` and update it with your MySQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OfficeManagementSystem;Uid=root;Pwd=your_password_here;"
  }
}
```

### 4. Restore packages and run

```bash
dotnet restore
dotnet run
```

The app will start at `https://localhost:5001` (or the port shown in your terminal). It will redirect you to the login page.

---

## Database Schema

Run the following SQL to set up the database from scratch.

```sql
CREATE DATABASE IF NOT EXISTS OfficeManagementSystem;
USE OfficeManagementSystem;

-- Departments
CREATE TABLE Departments (
    DepartmentID INT AUTO_INCREMENT PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL
);

-- Roles
CREATE TABLE Roles (
    RoleID INT AUTO_INCREMENT PRIMARY KEY,
    RoleName VARCHAR(100) NOT NULL
);

-- Employees
CREATE TABLE Employees (
    EmployeeID   INT AUTO_INCREMENT PRIMARY KEY,
    FirstName    VARCHAR(50)  NOT NULL,
    LastName     VARCHAR(50)  NOT NULL,
    Email        VARCHAR(100) NOT NULL,
    Phone        VARCHAR(11)  NOT NULL,
    Gender       VARCHAR(10)  NOT NULL,
    DOB          DATE         NOT NULL,
    HireDate     DATE         NOT NULL,
    Status       VARCHAR(20)  NOT NULL DEFAULT 'Active',
    DepartmentID INT,
    RoleID       INT,
    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID),
    FOREIGN KEY (RoleID)       REFERENCES Roles(RoleID)
);

-- Users (login accounts)
CREATE TABLE Users (
    UserID     INT AUTO_INCREMENT PRIMARY KEY,
    UserName   VARCHAR(50)  NOT NULL UNIQUE,
    Password   VARCHAR(255) NOT NULL,
    EmployeeID INT,
    IsActive   VARCHAR(3)   NOT NULL DEFAULT 'Yes',
    IsAdmin    VARCHAR(3)   NOT NULL DEFAULT 'No',
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- Leave Types
CREATE TABLE LeaveTypes (
    LeaveTypeID  INT AUTO_INCREMENT PRIMARY KEY,
    LeaveTypeName VARCHAR(50) NOT NULL,
    AllowedDays  INT NOT NULL
);

-- Leave Requests
CREATE TABLE LeaveRequests (
    LeaveID     INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeID  INT          NOT NULL,
    LeaveTypeID INT          NOT NULL,
    Reason      TEXT,
    StartDate   DATE         NOT NULL,
    EndDate     DATE         NOT NULL,
    Approved    VARCHAR(10)  NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (EmployeeID)  REFERENCES Employees(EmployeeID),
    FOREIGN KEY (LeaveTypeID) REFERENCES LeaveTypes(LeaveTypeID)
);

-- Attendance
CREATE TABLE Attendance (
    AttendanceID   INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeID     INT  NOT NULL,
    AttendanceDate DATE NOT NULL,
    ClockInTime    TIME,
    ClockOutTime   TIME,
    Status         VARCHAR(10) NOT NULL DEFAULT 'Present',
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- Salary
CREATE TABLE Salary (
    SalaryID   INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeID INT     NOT NULL,
    BasicSalary DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- Payroll
CREATE TABLE Payroll (
    PayrollID   INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeID  INT            NOT NULL,
    Bonus       DECIMAL(10,2)  NOT NULL DEFAULT 0,
    Deduction   DECIMAL(10,2)  NOT NULL DEFAULT 0,
    NetSalary   DECIMAL(10,2)  NOT NULL,
    PaymentDate DATE           NOT NULL,
    Status      VARCHAR(10)    NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- Projects
CREATE TABLE Projects (
    ProjectID   INT AUTO_INCREMENT PRIMARY KEY,
    ProjectName VARCHAR(100) NOT NULL,
    Description TEXT,
    StartDate   DATE,
    EndDate     DATE,
    Status      VARCHAR(20) NOT NULL DEFAULT 'Ongoing'
);

-- Employee-Project assignments
CREATE TABLE EmployeeProjects (
    EmployeeProjectID INT AUTO_INCREMENT PRIMARY KEY,
    EmployeeID        INT  NOT NULL,
    ProjectID         INT  NOT NULL,
    AssignDate        DATE NOT NULL,
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (ProjectID)  REFERENCES Projects(ProjectID)
);

-- Meetings
CREATE TABLE Meetings (
    MeetingID   INT AUTO_INCREMENT PRIMARY KEY,
    Title       VARCHAR(100) NOT NULL,
    MeetingDate DATE         NOT NULL,
    Location    VARCHAR(100),
    Description TEXT,
    OrganizerID INT,
    FOREIGN KEY (OrganizerID) REFERENCES Employees(EmployeeID)
);

-- Meeting Participants
CREATE TABLE MeetingParticipants (
    ParticipantID INT AUTO_INCREMENT PRIMARY KEY,
    MeetingID     INT NOT NULL,
    EmployeeID    INT NOT NULL,
    FOREIGN KEY (MeetingID)  REFERENCES Meetings(MeetingID),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
```

### Views

```sql
-- Attendance summary used by the dashboard
CREATE VIEW AttendanceSummary AS
SELECT
    e.EmployeeID,
    CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
    SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) AS TotalPresent,
    SUM(CASE WHEN a.Status = 'Absent'  THEN 1 ELSE 0 END) AS TotalAbsent,
    SUM(CASE WHEN a.Status = 'Late'    THEN 1 ELSE 0 END) AS TotalLate,
    (
        SELECT COUNT(*) FROM LeaveRequests lr
        WHERE lr.EmployeeID = e.EmployeeID AND lr.Approved = 'Pending'
    ) AS PendingLeaves
FROM Employees e
LEFT JOIN Attendance a ON e.EmployeeID = a.EmployeeID
GROUP BY e.EmployeeID, e.FirstName, e.LastName;
```

### Stored Procedures

```sql
-- Approve or reject a leave request
DELIMITER //
CREATE PROCEDURE ApproveLeave(
    IN in_LeaveID INT,
    IN in_Status  VARCHAR(10)
)
BEGIN
    UPDATE LeaveRequests
    SET Approved = in_Status
    WHERE LeaveID = in_LeaveID;
END //
DELIMITER ;

-- Calculate and insert a payroll record
DELIMITER //
CREATE PROCEDURE CalculatePayroll(
    IN in_EmployeeID  INT,
    IN in_Bonus       DECIMAL(10,2),
    IN in_Deduction   DECIMAL(10,2),
    IN in_PaymentDate DATE
)
BEGIN
    DECLARE base DECIMAL(10,2);
    DECLARE net  DECIMAL(10,2);

    SELECT BasicSalary INTO base
    FROM Salary
    WHERE EmployeeID = in_EmployeeID
    LIMIT 1;

    SET net = base + in_Bonus - in_Deduction;

    INSERT INTO Payroll (EmployeeID, Bonus, Deduction, NetSalary, PaymentDate, Status)
    VALUES (in_EmployeeID, in_Bonus, in_Deduction, net, in_PaymentDate, 'Paid');
END //
DELIMITER ;
```

### Seed Data (optional)

```sql
-- Leave types
INSERT INTO LeaveTypes (LeaveTypeName, AllowedDays) VALUES
('Annual Leave',  15),
('Sick Leave',    10),
('Casual Leave',   7),
('Unpaid Leave',   0);

-- Create an admin user (password stored as plain text here — hash it in production)
INSERT INTO Departments (DepartmentName) VALUES ('HR');
INSERT INTO Roles (RoleName) VALUES ('Manager');
INSERT INTO Employees (FirstName, LastName, Email, Phone, Gender, DOB, HireDate, DepartmentID, RoleID)
    VALUES ('Admin', 'User', 'admin@company.com', '03001234567', 'Male', '1990-01-01', '2020-01-01', 1, 1);
INSERT INTO Users (UserName, Password, EmployeeID, IsActive, IsAdmin)
    VALUES ('admin', 'admin123', 1, 'Yes', 'Yes');
```

---

## How to Use

### Logging in

Go to the app URL — it redirects to `/Account/Login`. Use the credentials from your `Users` table.

### Admin vs Employee

| Feature | Employee | Admin |
|---|---|---|
| View employees | ✅ | ✅ |
| Add / Edit / Delete employees | ❌ | ✅ |
| Submit leave requests | ✅ | ✅ |
| Approve / Reject leaves | ❌ | ✅ |
| View attendance | ✅ | ✅ |
| Process payroll | ❌ | ✅ |
| Create / Edit projects & meetings | ❌ | ✅ |

### Promoting a user to admin

```sql
UPDATE Users SET IsAdmin = 'Yes' WHERE UserName = 'their_username';
```

### Deactivating a user

```sql
UPDATE Users SET IsActive = 'No' WHERE UserName = 'their_username';
```

---

## Project Structure

```
OfficeManagementSystem/
├── Controllers/        # MVC controllers for each module
├── Data/               # Repository classes (raw ADO.NET queries)
├── Models/             # Plain C# model classes
├── Services/           # PayrollService (business logic layer)
├── Utilities/          # ValidationCheck, Logging
├── Views/              # Razor views (.cshtml)
│   ├── Account/
│   ├── Attendance/
│   ├── Dashboard/
│   ├── Employees/
│   ├── Leave/
│   ├── Meetings/
│   ├── Payroll/
│   ├── Projects/
│   └── Shared/         # _Layout, Error
├── wwwroot/            # Static files (Bootstrap, jQuery, CSS)
├── appsettings.json    # Connection string config
└── Program.cs          # App setup and DI registration
```

---

## Notes

- Passwords are stored as plain text in this version — for any real deployment, hash them with something like BCrypt before storing.
- The `appsettings.json` connection string should be moved to environment variables or user secrets before going to production.
- Error logs are written to a `logs/errors.txt` file in the app's base directory.
