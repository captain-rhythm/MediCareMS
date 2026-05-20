USE MediCareMS;
GO

DECLARE @DeptCardiology INT = 2;
DECLARE @DeptNeurology INT = 7;
DECLARE @DeptOrthopedics INT = 3;
DECLARE @DeptDermatology INT = 6;
DECLARE @DeptPediatrics INT = 4;

-- Insert Doctors and capture IDs
DECLARE @Dr1 INT, @Dr2 INT, @Dr3 INT, @Dr4 INT, @Dr5 INT;

INSERT INTO Doctors (DoctorNo, FullName, Email, MobileNumber, DepartmentId, SpecializationId, ConsultationFee, Status, IsDeleted, CreatedAt)
VALUES ('DR-050', 'Dr. Tanvir Ahmed', 'tanvir@medicare.local', '01710000001', @DeptCardiology, 1, 1000, 0, 0, GETDATE());
SET @Dr1 = SCOPE_IDENTITY();

INSERT INTO Doctors (DoctorNo, FullName, Email, MobileNumber, DepartmentId, SpecializationId, ConsultationFee, Status, IsDeleted, CreatedAt)
VALUES ('DR-051', 'Dr. Fahim Hossain', 'fahim@medicare.local', '01710000002', @DeptNeurology, 1, 1000, 0, 0, GETDATE());
SET @Dr2 = SCOPE_IDENTITY();

INSERT INTO Doctors (DoctorNo, FullName, Email, MobileNumber, DepartmentId, SpecializationId, ConsultationFee, Status, IsDeleted, CreatedAt)
VALUES ('DR-052', 'Dr. Arif Rahman', 'arif@medicare.local', '01710000003', @DeptOrthopedics, 1, 1000, 0, 0, GETDATE());
SET @Dr3 = SCOPE_IDENTITY();

INSERT INTO Doctors (DoctorNo, FullName, Email, MobileNumber, DepartmentId, SpecializationId, ConsultationFee, Status, IsDeleted, CreatedAt)
VALUES ('DR-053', 'Dr. Saif Hasan', 'saif@medicare.local', '01710000004', @DeptDermatology, 1, 1000, 0, 0, GETDATE());
SET @Dr4 = SCOPE_IDENTITY();

INSERT INTO Doctors (DoctorNo, FullName, Email, MobileNumber, DepartmentId, SpecializationId, ConsultationFee, Status, IsDeleted, CreatedAt)
VALUES ('DR-054', 'Dr. Imran Siddique', 'imran@medicare.local', '01710000005', @DeptPediatrics, 1, 1000, 0, 0, GETDATE());
SET @Dr5 = SCOPE_IDENTITY();

-- Insert Patients and capture IDs
DECLARE @Pt1 INT, @Pt2 INT, @Pt3 INT, @Pt4 INT, @Pt5 INT, @Pt6 INT, @Pt7 INT, @Pt8 INT;

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-010', 'Rafi Ahmed', 'rafi@gmail.com', '01810000001', 7, 0, '1990-01-01', 0, GETDATE());
SET @Pt1 = SCOPE_IDENTITY();

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-011', 'Saif Hasan', 'saif@gmail.com', '01810000002', 1, 0, '1990-01-01', 0, GETDATE());
SET @Pt2 = SCOPE_IDENTITY();

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-012', 'Arif Rahman', 'arif@gmail.com', '01810000003', 3, 0, '1990-01-01', 0, GETDATE());
SET @Pt3 = SCOPE_IDENTITY();

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-013', 'Ayesha Rahman', 'ayesha@gmail.com', '01810000004', 5, 1, '1990-01-01', 0, GETDATE());
SET @Pt4 = SCOPE_IDENTITY();

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-014', 'Nusrat Jahan', 'nusrat@gmail.com', '01810000005', 8, 1, '1990-01-01', 0, GETDATE());
SET @Pt5 = SCOPE_IDENTITY();

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-015', 'Raisa Karim', 'raisa@gmail.com', '01810000006', 2, 1, '1990-01-01', 0, GETDATE());
SET @Pt6 = SCOPE_IDENTITY();

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-016', 'Sadia Noor', 'sadia@gmail.com', '01810000007', 4, 1, '1990-01-01', 0, GETDATE());
SET @Pt7 = SCOPE_IDENTITY();

INSERT INTO Patients (PatientNo, FullName, Email, MobileNumber, BloodGroup, Gender, DateOfBirth, IsDeleted, CreatedAt)
VALUES ('PAT-017', 'Sumaiya Akter', 'sumaiya@gmail.com', '01810000008', 7, 1, '1990-01-01', 0, GETDATE());
SET @Pt8 = SCOPE_IDENTITY();

-- Insert Appointments
INSERT INTO Appointments (AppointmentNo, PatientId, DoctorId, AppointmentDate, AppointmentTime, Status, TokenNumber, IsDeleted, CreatedAt)
VALUES 
('APT-1001', @Pt1, @Dr1, '2026-05-21', '10:00:00', 0, 101, 0, GETDATE()),
('APT-1002', @Pt2, @Dr2, '2026-05-21', '10:30:00', 1, 102, 0, GETDATE()),
('APT-1003', @Pt3, @Dr3, '2026-05-21', '11:00:00', 2, 103, 0, GETDATE()),
('APT-1004', @Pt4, @Dr4, '2026-05-22', '09:30:00', 0, 104, 0, GETDATE()),
('APT-1005', @Pt5, @Dr5, '2026-05-22', '10:00:00', 3, 105, 0, GETDATE()),
('APT-1006', @Pt6, @Dr1, '2026-05-22', '11:00:00', 1, 106, 0, GETDATE()),
('APT-1007', @Pt7, @Dr2, '2026-05-23', '09:00:00', 0, 107, 0, GETDATE()),
('APT-1008', @Pt8, @Dr3, '2026-05-23', '09:30:00', 1, 108, 0, GETDATE());
