-- ClinicCare Database Password Update Script
-- Update user passwords with proper SHA256 hashes

USE ClinicCareDb;
GO

-- =====================================
-- Update User Passwords with Hashes
-- =====================================

-- Admin User
UPDATE Users SET PasswordHash = '6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=' WHERE Email = 'admin@healthcareplus.com';

-- Doctor Users
UPDATE Users SET PasswordHash = 'Y7vlZKpcCII1X23NbyJBtvjQLW85cllmrlZM0xDweFA=' WHERE Email = 'dr.smith@healthcareplus.com';
UPDATE Users SET PasswordHash = 'Y7vlZKpcCII1X23NbyJBtvjQLW85cllmrlZM0xDweFA=' WHERE Email = 'dr.johnson@healthcareplus.com';
UPDATE Users SET PasswordHash = 'Y7vlZKpcCII1X23NbyJBtvjQLW85cllmrlZM0xDweFA=' WHERE Email = 'dr.williams@healthcareplus.com';

-- Staff Users
UPDATE Users SET PasswordHash = '39SPNjOKo2Io67niBLumtOGNsLYj4lxFiQHtyDH7GOk=' WHERE Email = 'reception1@healthcareplus.com';
UPDATE Users SET PasswordHash = '39SPNjOKo2Io67niBLumtOGNsLYj4lxFiQHtyDH7GOk=' WHERE Email = 'reception2@healthcareplus.com';
UPDATE Users SET PasswordHash = '39SPNjOKo2Io67niBLumtOGNsLYj4lxFiQHtyDH7GOk=' WHERE Email = 'pharmacy1@healthcareplus.com';

-- Patient Users
UPDATE Users SET PasswordHash = 'bf9HgU6S46FMnwIdLT3dNToD5b87D0nVPoCho2BshtY=' WHERE Email = 'patient1@email.com';
UPDATE Users SET PasswordHash = 'bf9HgU6S46FMnwIdLT3dNToD5b87D0nVPoCho2BshtY=' WHERE Email = 'patient2@email.com';
UPDATE Users SET PasswordHash = 'bf9HgU6S46FMnwIdLT3dNToD5b87D0nVPoCho2BshtY=' WHERE Email = 'patient3@email.com';
UPDATE Users SET PasswordHash = 'bf9HgU6FMnwIdLT3dNToD5b87D0nVPoCho2BshtY=' WHERE Email = 'patient4@email.com';
UPDATE Users SET PasswordHash = 'bf9HgU6FMnwIdLT3dNToD5b87D0nVPoCho2BshtY=' WHERE Email = 'patient5@email.com';

-- Verify the updates
SELECT Email, PasswordHash, Role FROM Users WHERE OrganizationId = (SELECT Id FROM Organizations WHERE Subdomain = 'healthcareplus');

PRINT '✅ Password hashes updated successfully!';
PRINT '🔑 Demo credentials are now ready for login:';
PRINT '   Admin: admin@healthcareplus.com / Admin@123';
PRINT '   Doctor: dr.smith@healthcareplus.com / Doctor@123';
PRINT '   Staff: reception1@healthcareplus.com / Staff@123';
PRINT '   Patient: patient1@email.com / Patient@123';
