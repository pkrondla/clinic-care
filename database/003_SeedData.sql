-- ClinicCare Database Seed Data Script
-- Sample data for development and testing

USE ClinicCareDb;
GO

-- =====================================
-- Global Medicine Database (Sample)
-- =====================================

INSERT INTO GlobalMedicines (Name, GenericName, Type, Potency, Manufacturer, Price, Description) VALUES
-- Common Homoeopathic Remedies
('Arnica Montana', 'Arnica Montana', 'Pellets', '30C', 'Boiron', 12.50, 'For trauma, bruises, and muscle soreness'),
('Belladonna', 'Atropa Belladonna', 'Pellets', '30C', 'Boiron', 11.75, 'For sudden onset fever and inflammation'),
('Nux Vomica', 'Strychnos Nux-vomica', 'Pellets', '30C', 'Boiron', 11.50, 'For digestive issues and stress'),
('Pulsatilla', 'Pulsatilla Nigricans', 'Pellets', '30C', 'Boiron', 12.00, 'For changeable symptoms and emotional support'),
('Rhus Toxicodendron', 'Rhus Toxicodendron', 'Pellets', '30C', 'Boiron', 13.25, 'For joint stiffness and skin conditions'),
('Sulphur', 'Sulphur', 'Pellets', '30C', 'Boiron', 10.90, 'For skin conditions and chronic complaints'),
('Calcarea Carbonica', 'Calcarea Carbonica', 'Pellets', '30C', 'Boiron', 12.75, 'For constitutional treatment and growth'),
('Lycopodium', 'Lycopodium Clavatum', 'Pellets', '30C', 'Boiron', 13.50, 'For digestive and urinary complaints'),
('Natrum Muriaticum', 'Natrum Muriaticum', 'Pellets', '30C', 'Boiron', 11.25, 'For emotional issues and fluid retention'),
('Phosphorus', 'Phosphorus', 'Pellets', '30C', 'Boiron', 14.00, 'For respiratory and nervous system support'),

-- Liquid Medicines
('Arnica Montana', 'Arnica Montana', 'Liquid', '6C', 'Boiron', 15.50, 'Liquid form for easier administration'),
('Chamomilla', 'Matricaria Chamomilla', 'Liquid', '30C', 'Boiron', 14.25, 'For teething and irritability in children'),
('Calendula', 'Calendula Officinalis', 'Liquid', '30C', 'Boiron', 16.00, 'For wound healing and skin care'),

-- Tablets
('Hypericum', 'Hypericum Perforatum', 'Tablets', '30C', 'Boiron', 18.50, 'For nerve injuries and pain'),
('Apis Mellifica', 'Apis Mellifica', 'Tablets', '30C', 'Boiron', 17.25, 'For swelling and allergic reactions'),

-- Combination Remedies
('Oscillococcinum', 'Anas Barbariae', 'Pellets', '200C', 'Boiron', 25.00, 'For flu-like symptoms'),
('Stress Relief', 'Multiple', 'Tablets', 'Various', 'Boiron', 22.50, 'Combination remedy for stress management'),
('Sleep Aid', 'Multiple', 'Tablets', 'Various', 'Boiron', 20.75, 'Natural sleep support combination'),

-- Higher Potencies
('Arnica Montana', 'Arnica Montana', 'Pellets', '200C', 'Boiron', 18.00, 'Higher potency for acute trauma'),
('Belladonna', 'Atropa Belladonna', 'Pellets', '200C', 'Boiron', 17.50, 'Higher potency for acute fever'),
('Rhus Tox', 'Rhus Toxicodendron', 'Pellets', '200C', 'Boiron', 19.25, 'Higher potency for joint conditions'),

-- External Applications
('Arnica Gel', 'Arnica Montana', 'Gel', '1X', 'Boiron', 24.00, 'Topical gel for bruises and muscle pain'),
('Calendula Cream', 'Calendula Officinalis', 'Cream', '1X', 'Boiron', 21.50, 'Healing cream for cuts and wounds'),
('Hypericum Oil', 'Hypericum Perforatum', 'Oil', '1X', 'Boiron', 26.75, 'Topical oil for nerve pain');

-- =====================================
-- Sample Organization Setup
-- =====================================

-- Create a sample organization
INSERT INTO Organizations (Name, Subdomain, ContactEmail, ContactPhone, Address) VALUES
('HealthCare Plus Clinics', 'healthcareplus', 'admin@healthcareplus.com', '+1-555-0100', '123 Main Street, Medical District, City, State 12345');

DECLARE @OrgId INT = SCOPE_IDENTITY();

-- Create sample clinics
INSERT INTO Clinics (OrganizationId, Name, Code, Address, ContactPhone, ContactEmail) VALUES
(@OrgId, 'Downtown Clinic', 'DTC', '456 Downtown Ave, City, State 12345', '+1-555-0101', 'downtown@healthcareplus.com'),
(@OrgId, 'Northside Branch', 'NSB', '789 North Road, City, State 12345', '+1-555-0102', 'northside@healthcareplus.com'),
(@OrgId, 'Westend Center', 'WEC', '321 West Boulevard, City, State 12345', '+1-555-0103', 'westend@healthcareplus.com');

DECLARE @Clinic1Id INT = (SELECT Id FROM Clinics WHERE Code = 'DTC' AND OrganizationId = @OrgId);
DECLARE @Clinic2Id INT = (SELECT Id FROM Clinics WHERE Code = 'NSB' AND OrganizationId = @OrgId);
DECLARE @Clinic3Id INT = (SELECT Id FROM Clinics WHERE Code = 'WEC' AND OrganizationId = @OrgId);

-- =====================================
-- Sample Users Setup
-- =====================================

-- Admin User
INSERT INTO Users (OrganizationId, Email, PasswordHash, FirstName, LastName, Phone, Role) VALUES
(@OrgId, 'admin@healthcareplus.com', 'hashed_password_here', 'Admin', 'User', '+1-555-0110', 2); -- Admin

DECLARE @AdminUserId INT = SCOPE_IDENTITY();

-- Doctor Users
INSERT INTO Users (OrganizationId, Email, PasswordHash, FirstName, LastName, Phone, Role) VALUES
(@OrgId, 'dr.smith@healthcareplus.com', 'hashed_password_here', 'John', 'Smith', '+1-555-0201', 3), -- Doctor
(@OrgId, 'dr.johnson@healthcareplus.com', 'hashed_password_here', 'Sarah', 'Johnson', '+1-555-0202', 3), -- Doctor
(@OrgId, 'dr.williams@healthcareplus.com', 'hashed_password_here', 'Michael', 'Williams', '+1-555-0203', 3); -- Doctor

DECLARE @Doctor1Id INT = (SELECT Id FROM Users WHERE Email = 'dr.smith@healthcareplus.com');
DECLARE @Doctor2Id INT = (SELECT Id FROM Users WHERE Email = 'dr.johnson@healthcareplus.com');
DECLARE @Doctor3Id INT = (SELECT Id FROM Users WHERE Email = 'dr.williams@healthcareplus.com');

-- Staff Users
INSERT INTO Users (OrganizationId, Email, PasswordHash, FirstName, LastName, Phone, Role) VALUES
(@OrgId, 'reception1@healthcareplus.com', 'hashed_password_here', 'Emily', 'Davis', '+1-555-0301', 4), -- Staff
(@OrgId, 'reception2@healthcareplus.com', 'hashed_password_here', 'James', 'Wilson', '+1-555-0302', 4), -- Staff
(@OrgId, 'pharmacy1@healthcareplus.com', 'hashed_password_here', 'Lisa', 'Brown', '+1-555-0303', 4); -- Staff

-- Patient Users
INSERT INTO Users (OrganizationId, Email, PasswordHash, FirstName, LastName, Phone, Role) VALUES
(@OrgId, 'patient1@email.com', 'hashed_password_here', 'Robert', 'Anderson', '+1-555-1001', 5), -- Patient
(@OrgId, 'patient2@email.com', 'hashed_password_here', 'Jennifer', 'Taylor', '+1-555-1002', 5), -- Patient
(@OrgId, 'patient3@email.com', 'hashed_password_here', 'David', 'Martinez', '+1-555-1003', 5), -- Patient
(@OrgId, 'patient4@email.com', 'hashed_password_here', 'Susan', 'Garcia', '+1-555-1004', 5), -- Patient
(@OrgId, 'patient5@email.com', 'hashed_password_here', 'Christopher', 'Lee', '+1-555-1005', 5); -- Patient

-- =====================================
-- Doctor Profiles Setup
-- =====================================

INSERT INTO DoctorProfiles (OrganizationId, UserId, RegistrationNumber, Qualification, ExperienceYears, Specialization, ConsultationFeeInPerson, ConsultationFeeTele, FollowupFeeInPerson, FollowupFeeTele) VALUES
(@OrgId, @Doctor1Id, 'HOM001', 'BHMS, MD (Homoeopathy)', 15, 'Classical Homoeopathy', 150.00, 100.00, 75.00, 50.00),
(@OrgId, @Doctor2Id, 'HOM002', 'BHMS, PhD (Homoeopathy)', 12, 'Pediatric Homoeopathy', 175.00, 125.00, 85.00, 60.00),
(@OrgId, @Doctor3Id, 'HOM003', 'BHMS, MD (Homoeopathy)', 8, 'Constitutional Treatment', 140.00, 90.00, 70.00, 45.00);

DECLARE @DoctorProfile1Id INT = (SELECT Id FROM DoctorProfiles WHERE UserId = @Doctor1Id);
DECLARE @DoctorProfile2Id INT = (SELECT Id FROM DoctorProfiles WHERE UserId = @Doctor2Id);
DECLARE @DoctorProfile3Id INT = (SELECT Id FROM DoctorProfiles WHERE UserId = @Doctor3Id);

-- =====================================
-- Patient Profiles Setup
-- =====================================

INSERT INTO Patients (OrganizationId, UserId, PatientCode, DateOfBirth, Gender, BloodGroup, Address, EmergencyContact, MedicalHistory) VALUES
(@OrgId, (SELECT Id FROM Users WHERE Email = 'patient1@email.com'), 'P001', '1985-03-15', 'Male', 'O+', '123 Elm Street, City, State', '+1-555-2001', 'Chronic headaches, stress-related issues'),
(@OrgId, (SELECT Id FROM Users WHERE Email = 'patient2@email.com'), 'P002', '1990-07-22', 'Female', 'A-', '456 Oak Avenue, City, State', '+1-555-2002', 'Digestive issues, allergies'),
(@OrgId, (SELECT Id FROM Users WHERE Email = 'patient3@email.com'), 'P003', '1978-11-08', 'Male', 'B+', '789 Pine Road, City, State', '+1-555-2003', 'Joint pain, arthritis'),
(@OrgId, (SELECT Id FROM Users WHERE Email = 'patient4@email.com'), 'P004', '1992-05-30', 'Female', 'AB+', '321 Maple Lane, City, State', '+1-555-2004', 'Skin conditions, eczema'),
(@OrgId, (SELECT Id FROM Users WHERE Email = 'patient5@email.com'), 'P005', '1988-09-12', 'Male', 'O-', '654 Cedar Street, City, State', '+1-555-2005', 'Respiratory issues, asthma');

-- =====================================
-- Doctor Availability Setup
-- =====================================

-- Dr. Smith availability
INSERT INTO DoctorAvailabilities (OrganizationId, DoctorId, ClinicId, AvailableDate, StartTime, EndTime) VALUES
(@OrgId, @DoctorProfile1Id, @Clinic1Id, DATEADD(day, 0, CAST(GETDATE() AS DATE)), '09:00:00', '13:00:00'),
(@OrgId, @DoctorProfile1Id, @Clinic1Id, DATEADD(day, 1, CAST(GETDATE() AS DATE)), '09:00:00', '13:00:00'),
(@OrgId, @DoctorProfile1Id, @Clinic2Id, DATEADD(day, 0, CAST(GETDATE() AS DATE)), '15:00:00', '19:00:00'),
(@OrgId, @DoctorProfile1Id, @Clinic2Id, DATEADD(day, 2, CAST(GETDATE() AS DATE)), '09:00:00', '13:00:00');

-- Dr. Johnson availability
INSERT INTO DoctorAvailabilities (OrganizationId, DoctorId, ClinicId, AvailableDate, StartTime, EndTime) VALUES
(@OrgId, @DoctorProfile2Id, @Clinic2Id, DATEADD(day, 0, CAST(GETDATE() AS DATE)), '10:00:00', '14:00:00'),
(@OrgId, @DoctorProfile2Id, @Clinic3Id, DATEADD(day, 1, CAST(GETDATE() AS DATE)), '10:00:00', '14:00:00'),
(@OrgId, @DoctorProfile2Id, @Clinic1Id, DATEADD(day, 2, CAST(GETDATE() AS DATE)), '15:00:00', '18:00:00');

-- Dr. Williams availability
INSERT INTO DoctorAvailabilities (OrganizationId, DoctorId, ClinicId, AvailableDate, StartTime, EndTime) VALUES
(@OrgId, @DoctorProfile3Id, @Clinic3Id, DATEADD(day, 0, CAST(GETDATE() AS DATE)), '09:00:00', '17:00:00'),
(@OrgId, @DoctorProfile3Id, @Clinic1Id, DATEADD(day, 1, CAST(GETDATE() AS DATE)), '14:00:00', '18:00:00');

-- =====================================
-- Sample Clinic Medicines Setup
-- =====================================

-- Add some global medicines to clinic inventories
INSERT INTO ClinicMedicines (OrganizationId, ClinicId, GlobalMedicineId, Name, GenericName, Type, Potency, Manufacturer, PurchasePrice, SellingPrice, Description)
SELECT 
    @OrgId,
    @Clinic1Id,
    gm.Id,
    gm.Name,
    gm.GenericName,
    gm.Type,
    gm.Potency,
    gm.Manufacturer,
    gm.Price * 0.7, -- 30% markup from global price as purchase price
    gm.Price,
    gm.Description
FROM GlobalMedicines gm
WHERE gm.Id <= 10; -- First 10 medicines

-- Add inventory for these medicines
INSERT INTO Inventories (OrganizationId, ClinicId, MedicineId, CurrentStock, MinimumStock, MaximumStock, PurchasePrice, SellingPrice, ExpiryDate, BatchNumber)
SELECT 
    cm.OrganizationId,
    cm.ClinicId,
    cm.Id,
    CAST(RAND() * 100 + 20 AS INT), -- Random stock between 20-120
    10, -- Minimum stock
    200, -- Maximum stock
    cm.PurchasePrice,
    cm.SellingPrice,
    DATEADD(year, 2, GETDATE()), -- 2 years expiry
    'BATCH' + RIGHT('000' + CAST(cm.Id AS VARCHAR), 3)
FROM ClinicMedicines cm
WHERE cm.ClinicId = @Clinic1Id;

-- =====================================
-- Sample Appointments for Today
-- =====================================

DECLARE @TodayDate DATE = CAST(GETDATE() AS DATE);

-- Appointments for Dr. Smith at Downtown Clinic
INSERT INTO Appointments (OrganizationId, ClinicId, DoctorId, PatientId, AppointmentDate, TokenNumber, Type, Status, Notes) VALUES
(@OrgId, @Clinic1Id, @DoctorProfile1Id, (SELECT Id FROM Patients WHERE PatientCode = 'P001'), @TodayDate, 1, 1, 2, 'Follow-up consultation'), -- InProgress
(@OrgId, @Clinic1Id, @DoctorProfile1Id, (SELECT Id FROM Patients WHERE PatientCode = 'P002'), @TodayDate, 2, 1, 1, 'First visit'), -- Scheduled
(@OrgId, @Clinic1Id, @DoctorProfile1Id, (SELECT Id FROM Patients WHERE PatientCode = 'P003'), @TodayDate, 3, 2, 1, 'Teleconsultation'), -- Scheduled
(@OrgId, @Clinic1Id, @DoctorProfile1Id, (SELECT Id FROM Patients WHERE PatientCode = 'P004'), @TodayDate, 4, 1, 1, 'Regular check-up'); -- Scheduled

-- Appointments for Dr. Johnson at Northside Branch
INSERT INTO Appointments (OrganizationId, ClinicId, DoctorId, PatientId, AppointmentDate, TokenNumber, Type, Status, Notes) VALUES
(@OrgId, @Clinic2Id, @DoctorProfile2Id, (SELECT Id FROM Patients WHERE PatientCode = 'P005'), @TodayDate, 1, 1, 3, 'Treatment completed'), -- Completed
(@OrgId, @Clinic2Id, @DoctorProfile2Id, (SELECT Id FROM Patients WHERE PatientCode = 'P001'), @TodayDate, 2, 1, 1, 'Constitutional treatment'); -- Scheduled

PRINT 'Sample data inserted successfully!';
PRINT 'Organization: HealthCare Plus Clinics (Subdomain: healthcareplus)';
PRINT 'Admin Login: admin@healthcareplus.com';
PRINT 'Doctor Logins: dr.smith@healthcareplus.com, dr.johnson@healthcareplus.com, dr.williams@healthcareplus.com';
PRINT 'Staff Logins: reception1@healthcareplus.com, pharmacy1@healthcareplus.com';
PRINT 'Patient Logins: patient1@email.com through patient5@email.com';
PRINT 'Note: All passwords are hashed and need to be set properly in the application.';

GO
