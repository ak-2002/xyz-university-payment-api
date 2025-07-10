-- Add more users to make the project more use case friendly
-- This script adds users for different roles in a university setting

-- Add Bazaar user (for managing university bazaar/shop payments)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('bazaar.admin', 'bazaar@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

-- Add Registrar user (for managing student registrations and academic records)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('registrar', 'registrar@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

-- Add Accounts user (for managing financial accounts and payments)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('accounts', 'accounts@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

-- Add Finance Officer user
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('finance.officer', 'finance@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

-- Add Academic Advisor user
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('academic.advisor', 'advisor@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

-- Add Library user (for managing library fees)
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('library', 'library@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

-- Add more student users for testing
INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('john.student', 'john.student@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('sarah.student', 'sarah.student@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt) 
VALUES ('mike.student', 'mike.student@xyzuniversity.edu', '$2a$11$YourHashedPasswordHere', 1, GETDATE());

-- Get the user IDs for role assignment
DECLARE @BazaarUserId INT = (SELECT Id FROM Users WHERE Username = 'bazaar.admin');
DECLARE @RegistrarUserId INT = (SELECT Id FROM Users WHERE Username = 'registrar');
DECLARE @AccountsUserId INT = (SELECT Id FROM Users WHERE Username = 'accounts');
DECLARE @FinanceOfficerUserId INT = (SELECT Id FROM Users WHERE Username = 'finance.officer');
DECLARE @AcademicAdvisorUserId INT = (SELECT Id FROM Users WHERE Username = 'academic.advisor');
DECLARE @LibraryUserId INT = (SELECT Id FROM Users WHERE Username = 'library');
DECLARE @JohnStudentUserId INT = (SELECT Id FROM Users WHERE Username = 'john.student');
DECLARE @SarahStudentUserId INT = (SELECT Id FROM Users WHERE Username = 'sarah.student');
DECLARE @MikeStudentUserId INT = (SELECT Id FROM Users WHERE Username = 'mike.student');

-- Get role IDs
DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');
DECLARE @ManagerRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Manager');
DECLARE @StaffRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Staff');
DECLARE @StudentRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Student');

-- Assign roles to users
-- Bazaar admin gets Manager role
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@BazaarUserId, @ManagerRoleId, GETDATE(), 'System');

-- Registrar gets Manager role
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@RegistrarUserId, @ManagerRoleId, GETDATE(), 'System');

-- Accounts gets Staff role
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@AccountsUserId, @StaffRoleId, GETDATE(), 'System');

-- Finance Officer gets Manager role
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@FinanceOfficerUserId, @ManagerRoleId, GETDATE(), 'System');

-- Academic Advisor gets Staff role
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@AcademicAdvisorUserId, @StaffRoleId, GETDATE(), 'System');

-- Library gets Staff role
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@LibraryUserId, @StaffRoleId, GETDATE(), 'System');

-- Student users get Student role
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@JohnStudentUserId, @StudentRoleId, GETDATE(), 'System');

INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@SarahStudentUserId, @StudentRoleId, GETDATE(), 'System');

INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy) 
VALUES (@MikeStudentUserId, @StudentRoleId, GETDATE(), 'System');

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