-- Add more users to make the project more use case friendly
-- This script adds users for different roles in a university setting

-- Add Bazaar user (for managing university bazaar/shop payments)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('bazaar.admin', 'bazaar@xyzuniversity.edu', 'QWRtaW4xMjMh', 1, GETDATE());

-- Add Registrar user (for managing student registrations and academic records)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('registrar', 'registrar@xyzuniversity.edu', 'QWRtaW4xMjMh', 1, GETDATE());

-- Add Accounts user (for managing financial accounts and payments)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('accounts', 'accounts@xyzuniversity.edu', 'QWRtaW4xMjMh', 1, GETDATE());

-- Add Finance Officer user
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('finance.officer', 'finance@xyzuniversity.edu', 'QWRtaW4xMjMh', 1, GETDATE());

-- Add Academic Advisor user
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('academic.advisor', 'advisor@xyzuniversity.edu', 'QWRtaW4xMjMh', 1, GETDATE());

-- Add Library user (for managing library fees)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('library', 'library@xyzuniversity.edu', 'QWRtaW4xMjMh', 1, GETDATE());

-- Add more student users for testing
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('john.student', 'john.student@xyzuniversity.edu', 'U3R1ZGVudDEyMyE=', 1, GETDATE());

INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('sarah.student', 'sarah.student@xyzuniversity.edu', 'U3R1ZGVudDEyMyE=', 1, GETDATE());

INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('mike.student', 'mike.student@xyzuniversity.edu', 'U3R1ZGVudDEyMyE=', 1, GETDATE());

-- Assign roles to new users
-- Get role IDs first
DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');
DECLARE @ManagerRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Manager');
DECLARE @StaffRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Staff');
DECLARE @StudentRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Student');

-- Assign Bazaar Admin role (Manager level)
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy)
SELECT Id, @ManagerRoleId, GETDATE(), 'System'
FROM Users WHERE Username = 'bazaar.admin';

-- Assign Registrar role (Manager level)
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy)
SELECT Id, @ManagerRoleId, GETDATE(), 'System'
FROM Users WHERE Username = 'registrar';

-- Assign Accounts role (Staff level)
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy)
SELECT Id, @StaffRoleId, GETDATE(), 'System'
FROM Users WHERE Username = 'accounts';

-- Assign Finance Officer role (Manager level)
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy)
SELECT Id, @ManagerRoleId, GETDATE(), 'System'
FROM Users WHERE Username = 'finance.officer';

-- Assign Academic Advisor role (Staff level)
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy)
SELECT Id, @StaffRoleId, GETDATE(), 'System'
FROM Users WHERE Username = 'academic.advisor';

-- Assign Library role (Staff level)
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy)
SELECT Id, @StaffRoleId, GETDATE(), 'System'
FROM Users WHERE Username = 'library';

-- Assign Student roles
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy)
SELECT Id, @StudentRoleId, GETDATE(), 'System'
FROM Users WHERE Username IN ('john.student', 'sarah.student', 'mike.student');

-- Add more students to the Students table
INSERT INTO Students (StudentNumber, FullName, Program, Email, PhoneNumber, DateOfBirth, Address, IsActive, CreatedAt)
VALUES 
('S66002', 'John Doe', 'Computer Science', 'john.student@xyzuniversity.edu', '+254 700 000 001', '2000-05-15', 'Nairobi, Kenya', 1, GETDATE()),
('S66003', 'Sarah Johnson', 'Business Administration', 'sarah.student@xyzuniversity.edu', '+254 700 000 002', '1999-08-22', 'Mombasa, Kenya', 1, GETDATE()),
('S66004', 'Mike Wilson', 'Engineering', 'mike.student@xyzuniversity.edu', '+254 700 000 003', '2001-03-10', 'Kisumu, Kenya', 1, GETDATE());

PRINT 'Additional users and students added successfully!';
PRINT 'New users created:';
PRINT '- bazaar.admin (Manager role)';
PRINT '- registrar (Manager role)';
PRINT '- accounts (Staff role)';
PRINT '- finance.officer (Manager role)';
PRINT '- academic.advisor (Staff role)';
PRINT '- library (Staff role)';
PRINT '- john.student (Student role)';
PRINT '- sarah.student (Student role)';
PRINT '- mike.student (Student role)';
PRINT '';
PRINT 'New students added:';
PRINT '- John Doe (S66002) - Computer Science';
PRINT '- Sarah Johnson (S66003) - Business Administration';
PRINT '- Mike Wilson (S66004) - Engineering'; 