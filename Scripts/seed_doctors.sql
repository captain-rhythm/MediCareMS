-- ============================================================
-- Seed Script: Insert 39 Doctors into DoctorProfiles
-- Run against: MediCareMS database on RHYTHM\SQLEXPRESS
-- ============================================================
USE MediCareMS;
GO

-- Departments: 1=General Medicine, 2=Cardiology, 3=Orthopedics,
--              4=Pediatrics, 5=Gynecology, 6=Dermatology,
--              7=Neurology, 8=ENT
-- Specializations mirror department IDs 1-8

INSERT INTO Doctors
    (DoctorNo, UserId, FullName, DepartmentId, SpecializationId, Qualification, ExperienceYears, BmdcRegNo, MobileNumber, Email, ConsultationFee, Status, IsDeleted, CreatedAt)
VALUES
    ('DR-001', NULL, 'Dr. A. M. Salman Hasan',        1, 1, 'MBBS, FCPS', 8,  'BMDC-10001', '01700000001', 'salman.hasan@medicare.bd',        600,  0, 0, GETDATE()),
    ('DR-002', NULL, 'Dr. Dipta Dhar',                 2, 2, 'MBBS, MD',   6,  'BMDC-10002', '01700000002', 'dipta.dhar@medicare.bd',           700,  0, 0, GETDATE()),
    ('DR-003', NULL, 'Dr. Mohammad Imtiaz Royhan Arbi',3, 3, 'MBBS, MS',   5,  'BMDC-10003', '01700000003', 'imtiaz.arbi@medicare.bd',          650,  0, 0, GETDATE()),
    ('DR-004', NULL, 'Dr. Nurul Islam Ayman',          4, 4, 'MBBS, DCH',  7,  'BMDC-10004', '01700000004', 'nurul.ayman@medicare.bd',          550,  0, 0, GETDATE()),
    ('DR-005', NULL, 'Dr. Mohammad Ibrahim Khalilullah',5,5, 'MBBS, FCPS', 10, 'BMDC-10005', '01700000005', 'ibrahim.khalil@medicare.bd',       750,  0, 0, GETDATE()),
    ('DR-006', NULL, 'Dr. Montaherul Islam',            6, 6, 'MBBS, DDV', 4,  'BMDC-10006', '01700000006', 'montaherul.islam@medicare.bd',     500,  0, 0, GETDATE()),
    ('DR-007', NULL, 'Dr. Sajid Mahmud',                7, 7, 'MBBS, MD',  9,  'BMDC-10007', '01700000007', 'sajid.mahmud@medicare.bd',         800,  0, 0, GETDATE()),
    ('DR-008', NULL, 'Dr. Mohammad Shamsuddoha Shams',  8, 8, 'MBBS, DLO', 6,  'BMDC-10008', '01700000008', 'shams.doha@medicare.bd',           600,  0, 0, GETDATE()),
    ('DR-009', NULL, 'Dr. Mohammad Rakibul Islam',      1, 1, 'MBBS, FCPS', 5, 'BMDC-10009', '01700000009', 'rakibul.islam@medicare.bd',        600,  0, 0, GETDATE()),
    ('DR-010', NULL, 'Dr. Mohammed Ashraful Islam',     2, 2, 'MBBS, MD',  8,  'BMDC-10010', '01700000010', 'ashraful.islam@medicare.bd',       700,  0, 0, GETDATE()),
    ('DR-011', NULL, 'Dr. Mohammed Musa Khan',          3, 3, 'MBBS, MS',  7,  'BMDC-10011', '01700000011', 'musa.khan@medicare.bd',            650,  0, 0, GETDATE()),
    ('DR-012', NULL, 'Dr. Sami Ishmam Chowdhury',       4, 4, 'MBBS, DCH', 3,  'BMDC-10012', '01700000012', 'sami.chowdhury@medicare.bd',       550,  0, 0, GETDATE()),
    ('DR-013', NULL, 'Dr. Zayed Bin Nasim',             5, 5, 'MBBS, FCPS',12, 'BMDC-10013', '01700000013', 'zayed.nasim@medicare.bd',          750,  0, 0, GETDATE()),
    ('DR-014', NULL, 'Dr. Khalid Saifullah Tahmid',     6, 6, 'MBBS, DDV', 6,  'BMDC-10014', '01700000014', 'khalid.tahmid@medicare.bd',        500,  0, 0, GETDATE()),
    ('DR-015', NULL, 'Dr. Jashiful Alam Jehan',         7, 7, 'MBBS, MD',  4,  'BMDC-10015', '01700000015', 'jashiful.jehan@medicare.bd',       800,  0, 0, GETDATE()),
    ('DR-016', NULL, 'Dr. Md. Amzad Hosen Pinso',       8, 8, 'MBBS, DLO', 9,  'BMDC-10016', '01700000016', 'amzad.pinso@medicare.bd',          600,  0, 0, GETDATE()),
    ('DR-017', NULL, 'Dr. Shaharia Hossen',             1, 1, 'MBBS, FCPS', 7, 'BMDC-10017', '01700000017', 'shaharia.hossen@medicare.bd',      600,  0, 0, GETDATE()),
    ('DR-018', NULL, 'Dr. Sayed Mohammed Omar Anan',    2, 2, 'MBBS, MD',  5,  'BMDC-10018', '01700000018', 'omar.anan@medicare.bd',            700,  0, 0, GETDATE()),
    ('DR-019', NULL, 'Dr. Shalman Ahmed Nizum',         3, 3, 'MBBS, MS',  6,  'BMDC-10019', '01700000019', 'shalman.nizum@medicare.bd',        650,  0, 0, GETDATE()),
    ('DR-020', NULL, 'Dr. Romen Paul',                  4, 4, 'MBBS, DCH', 8,  'BMDC-10020', '01700000020', 'romen.paul@medicare.bd',           550,  0, 0, GETDATE()),
    ('DR-021', NULL, 'Dr. Osama',                       5, 5, 'MBBS, FCPS',4,  'BMDC-10021', '01700000021', 'osama@medicare.bd',                750,  0, 0, GETDATE()),
    ('DR-022', NULL, 'Dr. Maruf Islam',                 6, 6, 'MBBS, DDV', 3,  'BMDC-10022', '01700000022', 'maruf.islam@medicare.bd',          500,  0, 0, GETDATE()),
    ('DR-023', NULL, 'Dr. Asif Md. Nahim',              7, 7, 'MBBS, MD',  5,  'BMDC-10023', '01700000023', 'asif.nahim@medicare.bd',           800,  0, 0, GETDATE()),
    ('DR-024', NULL, 'Dr. Md. Mahmudur Rahman',         8, 8, 'MBBS, DLO', 7,  'BMDC-10024', '01700000024', 'mahmudur.rahman@medicare.bd',      600,  0, 0, GETDATE()),
    ('DR-025', NULL, 'Dr. Mohammad Imran Faruk',        1, 1, 'MBBS, FCPS',6,  'BMDC-10025', '01700000025', 'imran.faruk@medicare.bd',          600,  0, 0, GETDATE()),
    ('DR-026', NULL, 'Dr. Md. Arefin Mostofa',          2, 2, 'MBBS, MD',  4,  'BMDC-10026', '01700000026', 'arefin.mostofa@medicare.bd',       700,  0, 0, GETDATE()),
    ('DR-027', NULL, 'Dr. Md. Asaf Jahan Chowdhury',   3, 3, 'MBBS, MS',  8,  'BMDC-10027', '01700000027', 'asaf.chowdhury@medicare.bd',       650,  0, 0, GETDATE()),
    ('DR-028', NULL, 'Dr. Abu Hiaet Adnan',             4, 4, 'MBBS, DCH', 5,  'BMDC-10028', '01700000028', 'abu.adnan@medicare.bd',            550,  0, 0, GETDATE()),
    ('DR-029', NULL, 'Dr. Papon Chowdhury',             5, 5, 'MBBS, FCPS',9,  'BMDC-10029', '01700000029', 'papon.chowdhury@medicare.bd',      750,  0, 0, GETDATE()),
    ('DR-030', NULL, 'Dr. Mohammad Mohtasim Farzin',    6, 6, 'MBBS, DDV', 4,  'BMDC-10030', '01700000030', 'mohtasim.farzin@medicare.bd',      500,  0, 0, GETDATE()),
    ('DR-031', NULL, 'Dr. Mainul Islam Fahim',          7, 7, 'MBBS, MD',  6,  'BMDC-10031', '01700000031', 'mainul.fahim@medicare.bd',         800,  0, 0, GETDATE()),
    ('DR-032', NULL, 'Dr. Mohammad Iman Uddin',         8, 8, 'MBBS, DLO', 3,  'BMDC-10032', '01700000032', 'iman.uddin@medicare.bd',           600,  0, 0, GETDATE()),
    ('DR-033', NULL, 'Dr. Md. Insirat Islam',           1, 1, 'MBBS, FCPS',7,  'BMDC-10033', '01700000033', 'insirat.islam@medicare.bd',        600,  0, 0, GETDATE()),
    ('DR-034', NULL, 'Dr. Mohammad Jamiul Hossain',     2, 2, 'MBBS, MD',  5,  'BMDC-10034', '01700000034', 'jamiul.hossain@medicare.bd',       700,  0, 0, GETDATE()),
    ('DR-035', NULL, 'Dr. Raktim Chowdhury',            3, 3, 'MBBS, MS',  9,  'BMDC-10035', '01700000035', 'raktim.chowdhury@medicare.bd',     650,  0, 0, GETDATE()),
    ('DR-036', NULL, 'Dr. Md. Rakib Hossain',           4, 4, 'MBBS, DCH', 4,  'BMDC-10036', '01700000036', 'rakib.hossain@medicare.bd',        550,  0, 0, GETDATE()),
    ('DR-037', NULL, 'Dr. Shoaib Ibne Faruk',           5, 5, 'MBBS, FCPS',6,  'BMDC-10037', '01700000037', 'shoaib.faruk@medicare.bd',         750,  0, 0, GETDATE()),
    ('DR-038', NULL, 'Dr. Ather Uddin Rabbanee',        6, 6, 'MBBS, DDV', 8,  'BMDC-10038', '01700000038', 'ather.rabbanee@medicare.bd',       500,  0, 0, GETDATE()),
    ('DR-039', NULL, 'Dr. Syed Mohammad Takeeur Rahman',7, 7, 'MBBS, MD',  10, 'BMDC-10039', '01700000039', 'takeeur.rahman@medicare.bd',       800,  0, 0, GETDATE());

PRINT 'Successfully inserted 39 doctors.';
SELECT COUNT(*) AS TotalDoctors FROM Doctors WHERE IsDeleted = 0;
GO
