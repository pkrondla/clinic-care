-- ====================================
-- Add ConsultationType to Consultations
-- ====================================
-- Matches Consultation.ConsultationType (1 = InPerson, 2 = Teleconsultation)
-- Backfills from Appointments.Type when available.
-- ====================================

USE HomoeoDesk_demo;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Consultations')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'ConsultationType')
BEGIN
    ALTER TABLE Consultations ADD ConsultationType INT NOT NULL CONSTRAINT DF_Consultations_ConsultationType DEFAULT 1;
    PRINT 'Added ConsultationType column.';
END
ELSE
BEGIN
    PRINT 'ConsultationType already exists or Consultations table missing — skipped.';
END
GO

-- Separate batch so the new column is visible to the parser
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Consultations') AND name = 'ConsultationType')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'Type')
BEGIN
    UPDATE c
    SET c.ConsultationType = a.[Type]
    FROM Consultations c
    INNER JOIN Appointments a ON a.Id = c.AppointmentId
    WHERE a.[Type] IS NOT NULL;

    PRINT 'Backfilled ConsultationType from Appointments.Type.';
END
GO
