-- ClinicCare Database Password Update Script (PBKDF2)
-- Update user passwords with proper PBKDF2 hashes

USE ClinicCareDb;
GO

-- =====================================
-- Update User Passwords with PBKDF2 Hashes
-- =====================================

-- Admin User
UPDATE Users SET PasswordHash = 'PjuL/vMV0GQHPPIe0GZnGdTI3/8gWDAOrfdVxPm/BmUVwYhUENaK3kyQB1T3dz04f1q8RBnKXn8pLslkKMThZQ==' WHERE Email = 'admin@healthcareplus.com';

-- Doctor Users
UPDATE Users SET PasswordHash = 'OzeDgABaZtqPArgmNvx64kJyLHcugyDLTTf+Mz3wjPgqfGJUwMAkSFIEWzV0lEPqV3pZj7Nb0V/6O9T5GgNo5g==' WHERE Email = 'dr.smith@healthcareplus.com';
UPDATE Users SET PasswordHash = '6nOyORqMAZGgO7tiHedQOROfike2F0y3jp7d8GYyPP3tXZvMAlZpuz4Ppx5NxtN1qUYlQthTK2XvQ4/hdODI7Q==' WHERE Email = 'dr.johnson@healthcareplus.com';
UPDATE Users SET PasswordHash = 'In3YGEnBHSqJnrnUMl1LrrRC6kl2xIAObKWKhCXuhFqXayLq/ieqYAYuZSdKIJdP3z/h0DCNexodVYpt6AUXtw==' WHERE Email = 'dr.williams@healthcareplus.com';

-- Staff Users
UPDATE Users SET PasswordHash = 'zuQfdiYivXGDr9naY+uIQKhSIE2k3rmVOWEexyLoYWttuICUXvRJ7oo/FYw4COK6wJGUXlUhpuDnUBxM6U2VmQ==' WHERE Email = 'reception1@healthcareplus.com';
UPDATE Users SET PasswordHash = 'hJ6tt9lhVuGBpQaYw5/mQlMw+KWTpPNHNA3RChABOooiW9gvXYpEU+llTJr7azvswr/30AvWaJyUfN4oj75+BQ==' WHERE Email = 'reception2@healthcareplus.com';
UPDATE Users SET PasswordHash = 'JlQRpPga4LwaCq5XZONo3NQAINPg+SZ0qEYlYQvS9Xv5wtdRCrLHpQ000U243crQXLQZe0QUDSTwmjwH6DAZQQ==' WHERE Email = 'pharmacy1@healthcareplus.com';

-- Patient Users
UPDATE Users SET PasswordHash = 'mMSKv/wMrZXPxT1Oh2h23IUK/vx0RT9yr7iVM01XNhQQL08fak96Jc8My7JFU0HUE6FuB2sRrl6HNLVJ0QstYw==' WHERE Email = 'patient1@email.com';
UPDATE Users SET PasswordHash = 'up5XXclJzBUH3g4KbdNeftygeBNIJQ8FWwB9AG7jufBCkM+fMoDTB1MH8rurdwA1OP50svEK/dqBzAsQRdrrLg==' WHERE Email = 'patient2@email.com';
UPDATE Users SET PasswordHash = 'S+4KmjZ9qbQDBWjlZpDYRbT9IUKeHgCJ5PixEsgmIzzrhLPOsgblMgoTU2Lg9DvCjlZUIaPFavx1p3bG/N0MIA==' WHERE Email = 'patient3@email.com';
UPDATE Users SET PasswordHash = 'V+om8FR1S8XatUKNlnx5KUIcb7rAaE+Fm/gJT1NkaOKg3S0cxaxJk1l7aDqiV78g+SfxgT00mbxNsRdUIb8Ofg==' WHERE Email = 'patient4@email.com';
UPDATE Users SET PasswordHash = 'SvZ0PhSPvyiWzqsMBlaG4KKL6c1YOIpSdbWAcdBKodmliNugi3URbwM8AwS0CNr3V9I1if2ncmcAxtnwS0Uwng==' WHERE Email = 'patient5@email.com';

-- Verify the updates
SELECT Email, PasswordHash, Role FROM Users WHERE OrganizationId = (SELECT Id FROM Organizations WHERE Subdomain = 'healthcareplus');

PRINT '✅ PBKDF2 Password hashes updated successfully!';
PRINT '🔑 Demo credentials are now ready for login:';
PRINT '   Admin: admin@healthcareplus.com / Admin@123';
PRINT '   Doctor: dr.smith@healthcareplus.com / Doctor@123';
PRINT '   Staff: reception1@healthcareplus.com / Staff@123';
PRINT '   Patient: patient1@email.com / Patient@123';

