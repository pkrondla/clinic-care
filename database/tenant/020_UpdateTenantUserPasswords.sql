-- ====================================
-- ClinicCare Tenant Database - Update User Password Hashes
-- ====================================
-- This script updates password hashes to match the PasswordHasher format
-- The PasswordHasher expects: Base64(salt(32 bytes) + hash(32 bytes))
-- 
-- IMPORTANT: You must generate the hash first using the API endpoint:
-- POST /api/test/generate-password-hash with body: { "password": "Admin@123" }
-- Then replace the hash value below with the generated hash
-- ====================================

USE ClinicCare_demo;
GO

-- Generate the hash using the API endpoint first, then replace this value
-- Example hash format: "aBcDeFgHiJkLmNoPqRsTuVwXyZ1234567890abcdefghijklmnopqrstuvwxyz=="
DECLARE @CorrectHash NVARCHAR(500) = 'REPLACE_WITH_GENERATED_HASH_FROM_API';

-- Only update if hash is not the placeholder
IF @CorrectHash != 'REPLACE_WITH_GENERATED_HASH_FROM_API'
BEGIN
    UPDATE Users 
    SET PasswordHash = @CorrectHash
    WHERE Email IN ('admin@demo.com', 'doctor@demo.com', 'reception@demo.com');
    
    PRINT 'Password hashes updated successfully.';
    PRINT 'Updated users: admin@demo.com, doctor@demo.com, reception@demo.com';
END
ELSE
BEGIN
    PRINT 'ERROR: Please generate the hash first using the API endpoint!';
    PRINT '1. Start the API';
    PRINT '2. Open Swagger: http://localhost:51537/swagger';
    PRINT '3. Call POST /api/test/generate-password-hash with body: { "password": "Admin@123" }';
    PRINT '4. Copy the hash value and replace REPLACE_WITH_GENERATED_HASH_FROM_API above';
END
GO

-- Verify the update
SELECT Email, 
       CASE 
           WHEN PasswordHash LIKE '1000:%' THEN 'OLD_FORMAT'
           ELSE 'NEW_FORMAT'
       END AS HashFormat,
       LEN(PasswordHash) AS HashLength
FROM Users 
WHERE Email IN ('admin@demo.com', 'doctor@demo.com', 'reception@demo.com');
GO


