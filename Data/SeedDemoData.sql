-- ============================================================
-- MediCare HMS - Demo Data Seed (v2 - safe & idempotent)
-- ============================================================
SET NOCOUNT ON;

-- 1. ADD MISSING DEPARTMENTS (only ones not already present)
INSERT INTO Departments (Name, Description, IsActive, IsDeleted, CreatedAt)
SELECT v.Name, v.Description, 1, 0, GETDATE()
FROM (VALUES
  ('Gastroenterology', 'Digestive system and gastrointestinal tract'),
  ('Oncology',         'Cancer diagnosis and treatment'),
  ('Urology',          'Urinary tract and male reproductive system'),
  ('Psychiatry',       'Mental health and behavioral disorders'),
  ('Ophthalmology',    'Eye care and vision disorders'),
  ('Endocrinology',    'Hormonal and metabolic disorders'),
  ('Nephrology',       'Kidney diseases and renal care')
) AS v(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM Departments WHERE Name = v.Name);

-- 2. ADD MISSING SPECIALIZATIONS (only ones not already present)
INSERT INTO Specializations (Name, IsDeleted, CreatedAt)
SELECT v.Name, 0, GETDATE()
FROM (VALUES
  ('Interventional Cardiology'),
  ('Pediatric Neurology'),
  ('Spine Surgery'),
  ('Neonatology'),
  ('Maternal-Fetal Medicine'),
  ('Cosmetic Dermatology'),
  ('Hepatology'),
  ('Radiation Oncology'),
  ('Laparoscopic Urology'),
  ('Child Psychiatry'),
  ('Retinal Surgery'),
  ('Rhinology'),
  ('Diabetology'),
  ('Renal Transplant'),
  ('Internal Medicine')
) AS v(Name)
WHERE NOT EXISTS (SELECT 1 FROM Specializations WHERE Name = v.Name);

-- 3. LOAD DEPARTMENT IDs (guaranteed to exist now)
DECLARE @dCard  INT = (SELECT Id FROM Departments WHERE Name='Cardiology');
DECLARE @dNeuro INT = (SELECT Id FROM Departments WHERE Name='Neurology');
DECLARE @dOrtho INT = (SELECT Id FROM Departments WHERE Name='Orthopedics');
DECLARE @dPed   INT = (SELECT Id FROM Departments WHERE Name='Pediatrics');
DECLARE @dGyn   INT = (SELECT Id FROM Departments WHERE Name='Gynecology');
DECLARE @dDerm  INT = (SELECT Id FROM Departments WHERE Name='Dermatology');
DECLARE @dGastro INT = (SELECT Id FROM Departments WHERE Name='Gastroenterology');
DECLARE @dOnco  INT = (SELECT Id FROM Departments WHERE Name='Oncology');
DECLARE @dUro   INT = (SELECT Id FROM Departments WHERE Name='Urology');
DECLARE @dPsych INT = (SELECT Id FROM Departments WHERE Name='Psychiatry');
DECLARE @dOph   INT = (SELECT Id FROM Departments WHERE Name='Ophthalmology');
DECLARE @dENT   INT = (SELECT Id FROM Departments WHERE Name='ENT');
DECLARE @dEndo  INT = (SELECT Id FROM Departments WHERE Name='Endocrinology');
DECLARE @dNeph  INT = (SELECT Id FROM Departments WHERE Name='Nephrology');
DECLARE @dGen   INT = (SELECT Id FROM Departments WHERE Name='General Medicine');

-- 4. LOAD SPECIALIZATION IDs
DECLARE @sCard  INT = (SELECT Id FROM Specializations WHERE Name='Interventional Cardiology');
DECLARE @sNeuro INT = (SELECT Id FROM Specializations WHERE Name='Pediatric Neurology');
DECLARE @sSpine INT = (SELECT Id FROM Specializations WHERE Name='Spine Surgery');
DECLARE @sNeo   INT = (SELECT Id FROM Specializations WHERE Name='Neonatology');
DECLARE @sMFM   INT = (SELECT Id FROM Specializations WHERE Name='Maternal-Fetal Medicine');
DECLARE @sCosm  INT = (SELECT Id FROM Specializations WHERE Name='Cosmetic Dermatology');
DECLARE @sHep   INT = (SELECT Id FROM Specializations WHERE Name='Hepatology');
DECLARE @sRadio INT = (SELECT Id FROM Specializations WHERE Name='Radiation Oncology');
DECLARE @sLapUro INT = (SELECT Id FROM Specializations WHERE Name='Laparoscopic Urology');
DECLARE @sChildPsy INT = (SELECT Id FROM Specializations WHERE Name='Child Psychiatry');
DECLARE @sRetina INT = (SELECT Id FROM Specializations WHERE Name='Retinal Surgery');
DECLARE @sRhino INT = (SELECT Id FROM Specializations WHERE Name='Rhinology');
DECLARE @sDiab  INT = (SELECT Id FROM Specializations WHERE Name='Diabetology');
DECLARE @sRenal INT = (SELECT Id FROM Specializations WHERE Name='Renal Transplant');
DECLARE @sIntMed INT = (SELECT Id FROM Specializations WHERE Name='Internal Medicine');

-- 5. INSERT PATIENTS (50 demo patients, skip if already seeded)
DECLARE @yr NVARCHAR(4) = CAST(YEAR(GETDATE()) AS NVARCHAR(4));

INSERT INTO Patients (PatientNo,UserId,FullName,DateOfBirth,Gender,BloodGroup,MobileNumber,Email,Address,Nationality,EmergencyContactName,EmergencyContactPhone,EmergencyContactRelation,KnownAllergies,ChronicConditions,IsDeleted,CreatedAt)
SELECT v.PatientNo,NULL,v.FullName,v.DOB,v.Gender,v.BloodGroup,v.Mobile,v.Email,v.Address,'Bangladeshi',v.ECName,v.ECPhone,v.ECRel,v.Allergies,v.Conditions,0,GETDATE()
FROM (VALUES
('PAT-'+@yr+'-DEMO01','Arif Hossain',       '1985-03-12',0,1,'01711001001','arif@demo.com',   'Dhaka, BD',           'Rina Hossain', '01711001099','Wife',   NULL,       'Hypertension'),
('PAT-'+@yr+'-DEMO02','Fatima Begum',        '1990-07-22',1,3,'01811002002','fatima@demo.com', 'Chittagong, BD',      'Karim Uddin',  '01811002099','Husband','Penicillin','Diabetes'),
('PAT-'+@yr+'-DEMO03','Mohammad Kamal',      '1975-11-05',0,5,'01911003003','kamal@demo.com',  'Sylhet, BD',          'Saba Kamal',   '01911003099','Wife',   NULL,       'Asthma'),
('PAT-'+@yr+'-DEMO04','Nusrat Jahan',        '1995-02-18',1,7,'01611004004','nusrat@demo.com', 'Rajshahi, BD',        'Reza Jahan',   '01611004099','Husband','Sulfa',     NULL),
('PAT-'+@yr+'-DEMO05','Rahim Mia',           '1968-09-30',0,2,'01511005005','rahim@demo.com',  'Khulna, BD',          'Amina Rahim',  '01511005099','Wife',   NULL,       'Arthritis'),
('PAT-'+@yr+'-DEMO06','Sumaiya Akter',       '1998-04-14',1,4,'01711006006','sumaiya@demo.com','Barishal, BD',        'Delwar Akter', '01711006099','Father', 'Aspirin',  NULL),
('PAT-'+@yr+'-DEMO07','Nazrul Islam',        '1980-06-25',0,6,'01811007007','nazrul@demo.com', 'Comilla, BD',         'Mitu Islam',   '01811007099','Wife',   NULL,       'High Cholesterol'),
('PAT-'+@yr+'-DEMO08','Roksana Parvin',      '1992-12-01',1,8,'01911008008','roksana@demo.com','Narayanganj, BD',     'Jahir Parvin', '01911008099','Husband',NULL,       'Migraine'),
('PAT-'+@yr+'-DEMO09','Abdul Mannan',        '1962-08-17',0,1,'01611009009','mannan@demo.com', 'Mymensingh, BD',      'Renu Mannan',  '01611009099','Wife',   'Ibuprofen','COPD'),
('PAT-'+@yr+'-DEMO10','Sharmin Sultana',     '1987-05-29',1,3,'01511010010','sharmin@demo.com','Gazipur, BD',         'Sohel Sultana','01511010099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO11','Jamal Uddin',         '1973-01-08',0,5,'01711011011','jamal@demo.com',  'Tangail, BD',         'Nasrin Jamal', '01711011099','Wife',   NULL,       'Diabetes'),
('PAT-'+@yr+'-DEMO12','Kamrun Nahar',        '1996-10-15',1,7,'01811012012','kamrun@demo.com', 'Noakhali, BD',        'Rafiq Nahar',  '01811012099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO13','Selim Reza',          '1978-03-22',0,2,'01911013013','selim@demo.com',  'Jessore, BD',         'Poly Reza',    '01911013099','Wife',   'Penicillin','Hypertension'),
('PAT-'+@yr+'-DEMO14','Tahmina Khatun',      '1991-07-11',1,4,'01611014014','tahmina@demo.com','Dinajpur, BD',        'Masud Khatun', '01611014099','Husband',NULL,       'Thyroid'),
('PAT-'+@yr+'-DEMO15','Mizanur Rahman',      '1984-11-19',0,6,'01511015015','mizan@demo.com',  'Pabna, BD',           'Lovely Rahman','01511015099','Wife',   NULL,       NULL),
('PAT-'+@yr+'-DEMO16','Shamima Akter',       '1993-02-28',1,8,'01711016016','shamima@demo.com','Bogura, BD',          'Ripon Akter',  '01711016099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO17','Hafizur Rahman',      '1969-09-04',0,1,'01811017017','hafiz@demo.com',  'Sirajganj, BD',       'Minara Rahman','01811017099','Wife',   'Sulfa',    'COPD'),
('PAT-'+@yr+'-DEMO18','Rokeya Begum',        '1986-06-13',1,3,'01911018018','rokeya@demo.com', 'Faridpur, BD',        'Hasan Begum',  '01911018099','Husband',NULL,       'Asthma'),
('PAT-'+@yr+'-DEMO19','Alamgir Hossain',     '1977-04-07',0,5,'01611019019','alamgir@demo.com','Narsingdi, BD',       'Kohinoor H.',  '01611019099','Wife',   NULL,       'Arthritis'),
('PAT-'+@yr+'-DEMO20','Nasrin Akter',        '1994-12-21',1,7,'01511020020','nasrin@demo.com', 'Habiganj, BD',        'Rafal Akter',  '01511020099','Husband','Aspirin',  NULL),
('PAT-'+@yr+'-DEMO21','Shafiqul Islam',      '1971-08-16',0,2,'01711021021','shafiq@demo.com', 'Moulvibazar, BD',     'Bilkis Islam', '01711021099','Wife',   NULL,       'High Cholesterol'),
('PAT-'+@yr+'-DEMO22','Hosne Ara Begum',     '1988-05-03',1,4,'01811022022','hosne@demo.com',  'Sunamganj, BD',       'Masum Begum',  '01811022099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO23','Babul Akter',         '1965-01-25',0,6,'01911023023','babul@demo.com',  'Netrokona, BD',       'Rumu Akter',   '01911023099','Wife',   'Ibuprofen','Diabetes'),
('PAT-'+@yr+'-DEMO24','Rubina Yesmin',       '1997-10-08',1,8,'01611024024','rubina@demo.com', 'Kishoreganj, BD',     'Jewel Yesmin', '01611024099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO25','Anisur Rahman',       '1982-03-17',0,1,'01511025025','anis@demo.com',   'Munshiganj, BD',      'Rubi Rahman',  '01511025099','Wife',   NULL,       'Hypertension'),
('PAT-'+@yr+'-DEMO26','Halima Khatun',       '1989-07-29',1,3,'01711026026','halima@demo.com', 'Chandpur, BD',        'Riaz Khatun',  '01711026099','Husband',NULL,       'Migraine'),
('PAT-'+@yr+'-DEMO27','Nurul Amin',          '1974-11-12',0,5,'01811027027','nurul@demo.com',  'Lakshmipur, BD',      'Josna Amin',   '01811027099','Wife',   NULL,       NULL),
('PAT-'+@yr+'-DEMO28','Ferdousi Begum',      '1992-02-06',1,7,'01911028028','ferdousi@demo.com','Feni, BD',           'Hanif Begum',  '01911028099','Husband','Penicillin','Thyroid'),
('PAT-'+@yr+'-DEMO29','Lutfur Rahman',       '1967-09-23',0,2,'01611029029','lutfur@demo.com', 'Brahmanbaria, BD',    'Morzina R.',   '01611029099','Wife',   NULL,       'Asthma'),
('PAT-'+@yr+'-DEMO30','Sabina Yesmin',       '1995-06-18',1,4,'01511030030','sabina@demo.com', 'Gaibandha, BD',       'Monir Yesmin', '01511030099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO31','Zahirul Haque',       '1979-04-01',0,6,'01711031031','zahir@demo.com',  'Nilphamari, BD',      'Rekha Haque',  '01711031099','Wife',   'Sulfa',    'Arthritis'),
('PAT-'+@yr+'-DEMO32','Champa Begum',        '1986-12-14',1,8,'01811032032','champa@demo.com', 'Lalmonirhat, BD',     'Dulal Begum',  '01811032099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO33','Rezaul Karim',        '1972-08-09',0,1,'01911033033','rezaul@demo.com', 'Kurigram, BD',        'Saju Karim',   '01911033099','Wife',   NULL,       'Diabetes'),
('PAT-'+@yr+'-DEMO34','Shirin Akter',        '1998-05-27',1,3,'01611034034','shirin@demo.com', 'Panchagarh, BD',      'Momin Akter',  '01611034099','Husband','Aspirin',  NULL),
('PAT-'+@yr+'-DEMO35','Billal Hossain',      '1963-01-15',0,5,'01511035035','billal@demo.com', 'Thakurgaon, BD',      'Roji Hossain', '01511035099','Wife',   NULL,       'COPD'),
('PAT-'+@yr+'-DEMO36','Mousumi Akter',       '1991-10-03',1,7,'01711036036','mousumi@demo.com','Joypurhat, BD',       'Jony Akter',   '01711036099','Husband',NULL,       'High Cholesterol'),
('PAT-'+@yr+'-DEMO37','Mozammel Haq',        '1976-03-20',0,2,'01811037037','mozammel@demo.com','Naogaon, BD',        'Anjali Haq',   '01811037099','Wife',   'Ibuprofen',NULL),
('PAT-'+@yr+'-DEMO38','Rimi Akter',          '1993-07-07',1,4,'01911038038','rimi@demo.com',   'Natore, BD',          'Sojib Akter',  '01911038099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO39','Shahidul Islam',      '1981-11-24',0,6,'01611039039','shahid@demo.com', 'Chapai, BD',          'Konika Islam', '01611039099','Wife',   NULL,       'Hypertension'),
('PAT-'+@yr+'-DEMO40','Asma Akter',          '1996-02-10',1,8,'01511040040','asma@demo.com',   'Sirajganj, BD',       'Farhan Akter', '01511040099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO41','Tariqul Islam',       '1970-09-13',0,1,'01711041041','tariq@demo.com',  'Sherpur, BD',         'Popi Islam',   '01711041099','Wife',   'Penicillin','Asthma'),
('PAT-'+@yr+'-DEMO42','Nargis Sultana',      '1988-06-02',1,3,'01811042042','nargis@demo.com', 'Jamalpur, BD',        'Polash S.',    '01811042099','Husband',NULL,       'Migraine'),
('PAT-'+@yr+'-DEMO43','Abul Kalam',          '1964-04-19',0,5,'01911043043','kalam@demo.com',  'Netrokona, BD',       'Reba Kalam',   '01911043099','Wife',   NULL,       'Arthritis'),
('PAT-'+@yr+'-DEMO44','Sharifa Khatun',      '1997-12-31',1,7,'01611044044','sharifa@demo.com','Kishoreganj, BD',     'Samim Khatun', '01611044099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO45','Golam Mostafa',       '1983-08-06',0,2,'01511045045','golam@demo.com',  'Mymensingh, BD',      'Rosy Mostafa', '01511045099','Wife',   'Sulfa',    'Diabetes'),
('PAT-'+@yr+'-DEMO46','Sadia Islam',         '1994-05-22',1,4,'01711046046','sadia@demo.com',  'Tangail, BD',         'Akash Islam',  '01711046099','Husband',NULL,       NULL),
('PAT-'+@yr+'-DEMO47','Faruk Ahmed',         '1978-01-28',0,6,'01811047047','faruk@demo.com',  'Manikganj, BD',       'Nipa Ahmed',   '01811047099','Wife',   NULL,       'Thyroid'),
('PAT-'+@yr+'-DEMO48','Ismat Ara',           '1990-10-15',1,8,'01911048048','ismat@demo.com',  'Munshiganj, BD',      'Babu Ara',     '01911048099','Husband','Aspirin',  NULL),
('PAT-'+@yr+'-DEMO49','Mahbubur Rahman',     '1966-03-11',0,1,'01611049049','mahbub@demo.com', 'Rajbari, BD',         'Lipa Rahman',  '01611049099','Wife',   NULL,       'COPD'),
('PAT-'+@yr+'-DEMO50','Jesmin Akter',        '1999-07-04',1,3,'01511050050','jesmin@demo.com', 'Madaripur, BD',       'Jahangir A.',  '01511050099','Husband',NULL,       NULL)
) AS v(PatientNo,FullName,DOB,Gender,BloodGroup,Mobile,Email,Address,ECName,ECPhone,ECRel,Allergies,Conditions)
WHERE NOT EXISTS (SELECT 1 FROM Patients WHERE PatientNo = v.PatientNo);

-- 6. INSERT DOCTORS (50 demo doctors, skip if already seeded)
INSERT INTO Doctors (DoctorNo,UserId,FullName,DepartmentId,SpecializationId,Qualification,ExperienceYears,BmdcRegNo,MobileNumber,Email,ConsultationFee,ChamberAddress,Bio,Status,IsDeleted,CreatedAt)
SELECT v.DoctorNo,NULL,v.FullName,v.DeptId,v.SpecId,v.Qual,v.Exp,v.Bmdc,v.Mobile,v.Email,v.Fee,v.Chamber,v.Bio,0,0,GETDATE()
FROM (VALUES
('DOC-DEMO001','Dr. Aminul Islam',    @dCard, @sCard,   'MBBS, MD (Cardiology)',          18,'BMDC-D001','01700100001','dr.aminul@medicare.com',   800,'Dhaka Medical Complex','Expert in interventional cardiology with 18 years experience.'),
('DOC-DEMO002','Dr. Rabeya Sultana',  @dNeuro,@sNeuro,  'MBBS, MD (Neurology)',           12,'BMDC-D002','01700100002','dr.rabeya@medicare.com',    700,'Apollo Hospital, Dhaka','Specializes in pediatric neurology and epilepsy management.'),
('DOC-DEMO003','Dr. Shahadat Hossain',@dOrtho,@sSpine,  'MBBS, MS (Orthopedics)',         15,'BMDC-D003','01700100003','dr.shahadat@medicare.com',  900,'National Orthopedic Hospital','Spine surgery expert with minimally invasive techniques.'),
('DOC-DEMO004','Dr. Farhana Alam',    @dPed,  @sNeo,    'MBBS, DCH, MD (Pediatrics)',      9,'BMDC-D004','01700100004','dr.farhana@medicare.com',   600,'Shishu Hospital, Dhaka','Neonatologist caring for premature and critically ill newborns.'),
('DOC-DEMO005','Dr. Nazmul Hasan',    @dGyn,  @sMFM,    'MBBS, FCPS (Gynecology)',        14,'BMDC-D005','01700100005','dr.nazmul@medicare.com',    750,'Dhaka Medical Center','Maternal-fetal medicine specialist for high-risk pregnancies.'),
('DOC-DEMO006','Dr. Salma Khatun',    @dDerm, @sCosm,   'MBBS, DDV (Dermatology)',         8,'BMDC-D006','01700100006','dr.salma@medicare.com',     550,'Skin & Laser Clinic, Gulshan','Cosmetic and medical dermatologist with laser expertise.'),
('DOC-DEMO007','Dr. Mahfuzur Rahman', @dGastro,@sHep,   'MBBS, MD (Gastroenterology)',    16,'BMDC-D007','01700100007','dr.mahfuz@medicare.com',    850,'BSMMU, Dhaka','Hepatologist specializing in liver diseases and transplantation.'),
('DOC-DEMO008','Dr. Afroza Begum',    @dOnco, @sRadio,  'MBBS, MD (Oncology)',            11,'BMDC-D008','01700100008','dr.afroza@medicare.com',   1000,'NICRH, Dhaka','Radiation oncologist treating solid tumors and lymphomas.'),
('DOC-DEMO009','Dr. Sohel Rana',      @dUro,  @sLapUro, 'MBBS, MS (Urology)',             13,'BMDC-D009','01700100009','dr.sohel@medicare.com',     800,'Urological Institute, Dhaka','Expert in laparoscopic urology and kidney stone management.'),
('DOC-DEMO010','Dr. Tania Akter',     @dPsych,@sChildPsy,'MBBS, MD (Psychiatry)',           7,'BMDC-D010','01700100010','dr.tania@medicare.com',     650,'Mental Health Center, Dhaka','Child and adolescent psychiatrist with CBT expertise.'),
('DOC-DEMO011','Dr. Kamrul Islam',    @dOph,  @sRetina, 'MBBS, DO (Ophthalmology)',       10,'BMDC-D011','01700100011','dr.kamrul@medicare.com',    700,'National Eye Hospital, Dhaka','Retinal surgeon performing vitreoretinal procedures.'),
('DOC-DEMO012','Dr. Nasima Parvin',   @dENT,  @sRhino,  'MBBS, MS (ENT)',                  6,'BMDC-D012','01700100012','dr.nasima@medicare.com',     600,'ENT Institute, Chittagong','Rhinologist specializing in sinus surgery and allergic rhinitis.'),
('DOC-DEMO013','Dr. Anwar Hossain',   @dEndo, @sDiab,   'MBBS, MD (Endocrinology)',       20,'BMDC-D013','01700100013','dr.anwar@medicare.com',     900,'BIRDEM, Dhaka','Senior diabetologist managing complex metabolic disorders.'),
('DOC-DEMO014','Dr. Gulshan Ara',     @dNeph, @sRenal,  'MBBS, MD (Nephrology)',          17,'BMDC-D014','01700100014','dr.gulshan@medicare.com',   850,'KidneyFoundation Hospital','Renal transplant specialist with over 500 successful surgeries.'),
('DOC-DEMO015','Dr. Zahir Uddin',     @dGen,  @sIntMed, 'MBBS, FCPS (Medicine)',          22,'BMDC-D015','01700100015','dr.zahir@medicare.com',     500,'General Hospital, Dhaka','Senior internist with expertise in tropical and infectious diseases.'),
('DOC-DEMO016','Dr. Razia Khanam',    @dCard, @sCard,   'MBBS, MD (Cardiology)',           9,'BMDC-D016','01700100016','dr.razia@medicare.com',     750,'Heart Foundation, Mirpur','Cardiologist specializing in heart failure and valvular disease.'),
('DOC-DEMO017','Dr. Mostak Ahmed',    @dNeuro,@sNeuro,  'MBBS, MD (Neurology)',           14,'BMDC-D017','01700100017','dr.mostak@medicare.com',    700,'NINS, Dhaka','Neurologist treating stroke and neurodegenerative diseases.'),
('DOC-DEMO018','Dr. Bilkis Begum',    @dOrtho,@sSpine,  'MBBS, MS (Orthopedics)',         11,'BMDC-D018','01700100018','dr.bilkis@medicare.com',    850,'NITOR, Dhaka','Orthopedic surgeon specializing in joint replacement.'),
('DOC-DEMO019','Dr. Shamsul Huda',    @dPed,  @sNeo,    'MBBS, FCPS (Pediatrics)',        16,'BMDC-D019','01700100019','dr.shamsul@medicare.com',   650,'ICMH, Dhaka','Pediatric specialist in infectious diseases and immunization.'),
('DOC-DEMO020','Dr. Murshida Khatun', @dGyn,  @sMFM,    'MBBS, FCPS (Gynecology)',         8,'BMDC-D020','01700100020','dr.murshida@medicare.com',  700,'DMCH Gynecology Unit','Obstetrician managing normal and complicated deliveries.'),
('DOC-DEMO021','Dr. Rubel Hossain',   @dDerm, @sCosm,   'MBBS, MD (Dermatology)',          5,'BMDC-D021','01700100021','dr.rubel@medicare.com',     500,'DermaCare Clinic, Sylhet','Dermatologist treating eczema, psoriasis and skin infections.'),
('DOC-DEMO022','Dr. Taslima Akter',   @dGastro,@sHep,   'MBBS, MD (Gastroenterology)',    13,'BMDC-D022','01700100022','dr.taslima@medicare.com',   800,'BSMMU Gastroenterology','Gastroenterologist performing endoscopy and colonoscopy.'),
('DOC-DEMO023','Dr. Imran Khan',      @dOnco, @sRadio,  'MBBS, MD (Oncology)',             7,'BMDC-D023','01700100023','dr.imran@medicare.com',     950,'Cancer Center, Chittagong','Medical oncologist specializing in breast and lung cancers.'),
('DOC-DEMO024','Dr. Rahela Akter',    @dUro,  @sLapUro, 'MBBS, MS (Urology)',             10,'BMDC-D024','01700100024','dr.rahela@medicare.com',    750,'Urology Center, Sylhet','Urologist managing bladder and prostate disorders.'),
('DOC-DEMO025','Dr. Farhan Iqbal',    @dPsych,@sChildPsy,'MBBS, MD (Psychiatry)',          12,'BMDC-D025','01700100025','dr.farhan@medicare.com',    600,'Kaan Pete Roi, Dhaka','Psychiatrist treating anxiety, depression and schizophrenia.'),
('DOC-DEMO026','Dr. Monira Begum',    @dOph,  @sRetina, 'MBBS, MS (Ophthalmology)',        8,'BMDC-D026','01700100026','dr.monira@medicare.com',    650,'Eye Institute, Rajshahi','Ophthalmologist performing cataract and glaucoma surgeries.'),
('DOC-DEMO027','Dr. Alamin Sarkar',   @dENT,  @sRhino,  'MBBS, MS (ENT)',                  9,'BMDC-D027','01700100027','dr.alamin@medicare.com',    600,'ENT Clinic, Khulna','ENT surgeon specializing in cochlear implants and hearing loss.'),
('DOC-DEMO028','Dr. Kohinoor Akter',  @dEndo, @sDiab,   'MBBS, MD (Endocrinology)',       11,'BMDC-D028','01700100028','dr.kohinoor@medicare.com',  850,'Hormone Clinic, Dhaka','Endocrinologist managing thyroid and pituitary disorders.'),
('DOC-DEMO029','Dr. Moshiur Rahman',  @dNeph, @sRenal,  'MBBS, MD (Nephrology)',           8,'BMDC-D029','01700100029','dr.moshiur@medicare.com',   800,'Kidney Hospital, Dhaka','Nephrologist managing chronic kidney disease and dialysis.'),
('DOC-DEMO030','Dr. Sultana Razia',   @dGen,  @sIntMed, 'MBBS, FCPS (Medicine)',          19,'BMDC-D030','01700100030','dr.sultana@medicare.com',   500,'General Medicine OPD','General physician experienced in multi-system diseases.'),
('DOC-DEMO031','Dr. Habibur Rahman',  @dCard, @sCard,   'MBBS, MD (Cardiology)',           7,'BMDC-D031','01700100031','dr.habibur@medicare.com',   750,'Cardiology Clinic, Rajshahi','Cardiologist treating arrhythmia and coronary artery disease.'),
('DOC-DEMO032','Dr. Nipa Akter',      @dNeuro,@sNeuro,  'MBBS, MD (Neurology)',            6,'BMDC-D032','01700100032','dr.nipa@medicare.com',      650,'Neurology OPD, Khulna','Neurologist specializing in headache disorders and vertigo.'),
('DOC-DEMO033','Dr. Sirajul Islam',   @dOrtho,@sSpine,  'MBBS, MS (Orthopedics)',          8,'BMDC-D033','01700100033','dr.sirajul@medicare.com',   800,'Ortho Clinic, Barishal','Orthopedic specialist in fracture management and sports injury.'),
('DOC-DEMO034','Dr. Runa Laila',      @dPed,  @sNeo,    'MBBS, DCH (Pediatrics)',          4,'BMDC-D034','01700100034','dr.runa@medicare.com',      550,'Children Hospital, Comilla','Pediatrician providing preventive and curative child care.'),
('DOC-DEMO035','Dr. Delwar Hossain',  @dGyn,  @sMFM,    'MBBS, FCPS (Gynecology)',        10,'BMDC-D035','01700100035','dr.delwar@medicare.com',    700,'Gynae Clinic, Mymensingh','Gynecologist managing PCOS, fibroids and infertility.'),
('DOC-DEMO036','Dr. Sabikun Nahar',   @dDerm, @sCosm,   'MBBS, DDV (Dermatology)',         3,'BMDC-D036','01700100036','dr.sabikun@medicare.com',   500,'Skin Clinic, Sylhet','Dermatologist specializing in acne, pigmentation and vitiligo.'),
('DOC-DEMO037','Dr. Motiur Rahman',   @dGastro,@sHep,   'MBBS, MD (Gastroenterology)',     5,'BMDC-D037','01700100037','dr.motiur@medicare.com',    750,'GI Center, Narayanganj','Gastroenterologist treating peptic ulcer and IBS.'),
('DOC-DEMO038','Dr. Sadia Afrin',     @dOnco, @sRadio,  'MBBS, MD (Oncology)',             6,'BMDC-D038','01700100038','dr.sadia@medicare.com',     900,'Oncology Wing, Rajshahi','Oncologist specializing in leukemia and lymphoma treatment.'),
('DOC-DEMO039','Dr. Aminur Islam',    @dUro,  @sLapUro, 'MBBS, MS (Urology)',             15,'BMDC-D039','01700100039','dr.aminur@medicare.com',    800,'Urological Center, Barishal','Senior urologist with expertise in kidney transplantation.'),
('DOC-DEMO040','Dr. Nasreen Jahan',   @dPsych,@sChildPsy,'MBBS, MD (Psychiatry)',           9,'BMDC-D040','01700100040','dr.nasreen@medicare.com',   600,'Psychiatry OPD, Sylhet','Psychiatrist focusing on PTSD and personality disorders.'),
('DOC-DEMO041','Dr. Shakil Ahmed',    @dOph,  @sRetina, 'MBBS, DO (Ophthalmology)',        6,'BMDC-D041','01700100041','dr.shakil@medicare.com',    650,'Eye Clinic, Comilla','Ophthalmologist treating diabetic retinopathy and dry eye.'),
('DOC-DEMO042','Dr. Morsheda Khanam', @dENT,  @sRhino,  'MBBS, MS (ENT)',                  4,'BMDC-D042','01700100042','dr.morsheda@medicare.com',  550,'ENT OPD, Gazipur','ENT specialist focusing on tonsil and adenoid diseases.'),
('DOC-DEMO043','Dr. Harunur Rashid',  @dEndo, @sDiab,   'MBBS, MD (Endocrinology)',       13,'BMDC-D043','01700100043','dr.harun@medicare.com',     900,'BIRDEM Annex, Dhaka','Diabetologist and obesity specialist with lifestyle medicine expertise.'),
('DOC-DEMO044','Dr. Shahnaz Parvin',  @dNeph, @sRenal,  'MBBS, MD (Nephrology)',          11,'BMDC-D044','01700100044','dr.shahnaz@medicare.com',   800,'Renal Center, Khulna','Nephrologist handling acute kidney injuries and dialysis.'),
('DOC-DEMO045','Dr. Mamunur Rashid',  @dGen,  @sIntMed, 'MBBS, FCPS (Medicine)',          14,'BMDC-D045','01700100045','dr.mamun@medicare.com',     500,'Medicine OPD, Chittagong','General physician specializing in fever, infections and NCD.'),
('DOC-DEMO046','Dr. Rina Akter',      @dCard, @sCard,   'MBBS, MD (Cardiology)',           5,'BMDC-D046','01700100046','dr.rina@medicare.com',      700,'Cardio Clinic, Narayanganj','Cardiologist managing hypertension and lipid disorders.'),
('DOC-DEMO047','Dr. Zahidul Karim',   @dNeuro,@sNeuro,  'MBBS, MD (Neurology)',            8,'BMDC-D047','01700100047','dr.zahidul@medicare.com',   700,'Brain & Spine Center, Dhaka','Neurologist specializing in Parkinson and dementia management.'),
('DOC-DEMO048','Dr. Aklima Khatun',   @dOrtho,@sSpine,  'MBBS, MS (Orthopedics)',         12,'BMDC-D048','01700100048','dr.aklima@medicare.com',    850,'Bone & Joint Clinic, Dhaka','Orthopedic surgeon specializing in hip and knee replacement.'),
('DOC-DEMO049','Dr. Iftekharul Alam', @dPed,  @sNeo,    'MBBS, DCH, FCPS (Pediatrics)',   11,'BMDC-D049','01700100049','dr.iftekhar@medicare.com',  650,'SHISHU Pediatrics, Dhaka','Pediatrician specializing in nutrition and growth disorders.'),
('DOC-DEMO050','Dr. Mahbuba Akter',   @dGyn,  @sMFM,    'MBBS, FCPS (Gynecology)',        16,'BMDC-D050','01700100050','dr.mahbuba@medicare.com',   750,'Women Health Clinic, Dhaka','Senior gynecologist managing uterine and ovarian disorders.')
) AS v(DoctorNo,FullName,DeptId,SpecId,Qual,Exp,Bmdc,Mobile,Email,Fee,Chamber,Bio)
WHERE NOT EXISTS (SELECT 1 FROM Doctors WHERE DoctorNo = v.DoctorNo);

-- 7. FINAL COUNT
SELECT 'Departments'     AS [Table], COUNT(*) AS Total FROM Departments     WHERE IsDeleted=0;
SELECT 'Specializations' AS [Table], COUNT(*) AS Total FROM Specializations WHERE IsDeleted=0;
SELECT 'Patients'        AS [Table], COUNT(*) AS Total FROM Patients        WHERE IsDeleted=0;
SELECT 'Doctors'         AS [Table], COUNT(*) AS Total FROM Doctors         WHERE IsDeleted=0;
PRINT 'Demo seed completed successfully.';
