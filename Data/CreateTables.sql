-- Create Users table for IT Ticketing System
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Role VARCHAR(50) DEFAULT 'User',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt TIMESTAMP NULL
);

-- Create Tickets table for IT Ticketing System
CREATE TABLE IF NOT EXISTS Tickets (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Subject VARCHAR(255) NOT NULL,
    Description TEXT NOT NULL,
    Priority VARCHAR(50) NOT NULL DEFAULT 'Medium',
    Status VARCHAR(50) NOT NULL DEFAULT 'Open',
    Department VARCHAR(50),
    Category VARCHAR(50),
    Attachments VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NULL,
    ResolvedAt TIMESTAMP NULL,
    CreatedById INT NOT NULL,
    AssignedToId INT,
    FOREIGN KEY (CreatedById) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (AssignedToId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Insert a default admin user for testing
-- Password: Admin123!
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, IsActive) 
VALUES ('admin@itticketing.com', 'Admin123!', 'Admin', 'User', 'Admin', TRUE);

-- Insert a manager user for testing manager dashboard
-- Password: Manager123!
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, IsActive) 
VALUES ('manager@itticketing.com', 'Manager123!', 'Alex', 'Rivera', 'Manager', TRUE);

-- Insert sample users for testing
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, IsActive) VALUES
('john.doe@company.com', 'Password123!', 'John', 'Doe', 'User', TRUE),
('jane.smith@company.com', 'Password123!', 'Jane', 'Smith', 'User', TRUE),
('mike.wilson@company.com', 'Password123!', 'Mike', 'Wilson', 'Support', TRUE),
('alex.morgan@company.com', 'Password123!', 'Alex', 'Morgan', 'Engineer', TRUE);

-- Insert sample tickets for testing
INSERT INTO Tickets (Subject, Description, Priority, Status, Department, Category, CreatedById) VALUES
('VPN Access Issue - London Branch', 'Cannot connect to VPN from London office. Getting authentication error.', 'Critical', 'Open', 'Engineering', 'Network', 1),
('Outlook License Renewal', 'Need to renew Outlook license for new employees.', 'Medium', 'In-Progress', 'HR', 'Software', 2),
('New Workstation Setup (HR)', 'Setup new workstation for HR department.', 'Low', 'Resolved', 'HR', 'Hardware', 3),
('Database Connectivity Error', 'Application cannot connect to production database.', 'High', 'Open', 'Engineering', 'Software', 1);
