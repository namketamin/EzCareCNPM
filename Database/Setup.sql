IF DB_ID(N'EzCare') IS NULL EXEC(N'CREATE DATABASE EzCare');
GO
USE EzCare;
GO
IF DB_NAME() <> N'EzCare'
    THROW 50001, N'Hãy tạo và chọn cơ sở dữ liệu EzCare trước khi tiếp tục.', 1;
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL
    THROW 50000, N'EzCare đã được khởi tạo. Hãy dùng cơ sở dữ liệu hiện có.', 1;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

CREATE TABLE Roles (
    RoleID int IDENTITY PRIMARY KEY,
    RoleName nvarchar(50) NOT NULL UNIQUE
);
CREATE TABLE Users (
    UserID int IDENTITY PRIMARY KEY,
    Username varchar(50) NOT NULL UNIQUE,
    PasswordHash varchar(255) NOT NULL,
    FullName nvarchar(100) NOT NULL,
    Phone varchar(15),
    Email varchar(100),
    RoleID int NOT NULL REFERENCES Roles(RoleID),
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_Users_Email ON Users(Email) WHERE Email IS NOT NULL;

CREATE TABLE dbo.[Password] (
    UserID int NOT NULL PRIMARY KEY REFERENCES dbo.Users(UserID),
    [Password] nvarchar(128) NOT NULL,
    UpdatedAt datetime2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Patients (
    PatientID int IDENTITY PRIMARY KEY,
    UserID int REFERENCES Users(UserID),
    FullName nvarchar(100) NOT NULL,
    Phone varchar(15) NOT NULL,
    DateOfBirth date,
    Gender nvarchar(10) CHECK (Gender IN (N'Nam', N'Nữ', N'Khác')),
    Address nvarchar(200)
);
CREATE UNIQUE INDEX UX_Patients_User ON Patients(UserID) WHERE UserID IS NOT NULL;
CREATE INDEX IX_Patients_Phone ON Patients(Phone);

CREATE TABLE Specialties (
    SpecialtyID int IDENTITY PRIMARY KEY,
    SpecialtyName nvarchar(100) NOT NULL UNIQUE,
    Description nvarchar(500)
);
CREATE TABLE Doctors (
    DoctorID int IDENTITY PRIMARY KEY,
    UserID int NOT NULL UNIQUE REFERENCES Users(UserID),
    SpecialtyID int NOT NULL REFERENCES Specialties(SpecialtyID),
    Degree nvarchar(100),
    ExperienceYears int CHECK (ExperienceYears >= 0),
    Biography nvarchar(max)
);
CREATE TABLE WorkSchedules (
    ScheduleID int IDENTITY PRIMARY KEY,
    DoctorID int NOT NULL REFERENCES Doctors(DoctorID),
    WorkDate date NOT NULL,
    StartTime time(0) NOT NULL,
    EndTime time(0) NOT NULL,
    SlotDuration int NOT NULL CHECK (SlotDuration IN (15,30)),
    Room nvarchar(50) NOT NULL,
    Status nvarchar(20) NOT NULL DEFAULT 'Open' CHECK (Status IN ('Open','Locked','Cancelled')),
    CHECK (StartTime < EndTime)
);
CREATE INDEX IX_WorkSchedules_Doctor_Date ON WorkSchedules(DoctorID, WorkDate);

CREATE TABLE TimeSlots (
    SlotID int IDENTITY PRIMARY KEY,
    ScheduleID int NOT NULL REFERENCES WorkSchedules(ScheduleID),
    StartTime time(0) NOT NULL,
    EndTime time(0) NOT NULL,
    IsBooked bit NOT NULL DEFAULT 0,
    CHECK (StartTime < EndTime),
    UNIQUE (ScheduleID, StartTime)
);
CREATE TABLE Appointments (
    AppointmentID int IDENTITY PRIMARY KEY,
    BookingCode varchar(20) NOT NULL UNIQUE,
    PatientID int NOT NULL REFERENCES Patients(PatientID),
    SlotID int NOT NULL REFERENCES TimeSlots(SlotID),
    Reason nvarchar(500),
    Status nvarchar(30) NOT NULL DEFAULT 'Confirmed'
        CHECK (Status IN ('Confirmed','CheckedIn','Completed','Cancelled','NoShow')),
    CreatedByUserID int NOT NULL REFERENCES Users(UserID),
    CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_Appointments_ActiveSlot ON Appointments(SlotID)
    WHERE Status IN ('Confirmed','CheckedIn','Completed','NoShow');
CREATE INDEX IX_Appointments_Patient ON Appointments(PatientID, Status);

CREATE TABLE DoctorShiftRequests (
    RequestID int IDENTITY PRIMARY KEY,
    ScheduleID int NOT NULL REFERENCES WorkSchedules(ScheduleID),
    RequestType nvarchar(20) NOT NULL CHECK (RequestType IN ('NghiDotXuat','DoiCa')),
    Reason nvarchar(500) NOT NULL,
    Status nvarchar(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending','Approved','Rejected')),
    CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_ShiftRequests_Pending ON DoctorShiftRequests(ScheduleID) WHERE Status='Pending';

CREATE TABLE RescheduleLogs (
    LogID int IDENTITY PRIMARY KEY,
    AppointmentID int NOT NULL REFERENCES Appointments(AppointmentID),
    RequestID int REFERENCES DoctorShiftRequests(RequestID),
    HandledByUserID int NOT NULL REFERENCES Users(UserID),
    OldSlotID int REFERENCES TimeSlots(SlotID),
    NewSlotID int REFERENCES TimeSlots(SlotID),
    ActionStatus nvarchar(30) NOT NULL,
    Notes nvarchar(500),
    UpdatedAt datetime2 NOT NULL DEFAULT SYSDATETIME()
);
EXEC(N'CREATE VIEW AppointmentDetails AS
SELECT a.AppointmentID, a.BookingCode, a.PatientID, a.SlotID, a.Status, a.Reason,
       p.FullName AS PatientName, p.Phone, p.UserID AS PatientUserID,
       d.DoctorID, d.UserID AS DoctorUserID, u.FullName AS DoctorName,
       s.SpecialtyID, s.SpecialtyName, w.WorkDate, w.Room, w.ScheduleID,
       w.Status AS ScheduleStatus, t.StartTime, t.EndTime
FROM Appointments a
JOIN Patients p ON p.PatientID=a.PatientID
JOIN TimeSlots t ON t.SlotID=a.SlotID
JOIN WorkSchedules w ON w.ScheduleID=t.ScheduleID
JOIN Doctors d ON d.DoctorID=w.DoctorID
JOIN Users u ON u.UserID=d.UserID
JOIN Specialties s ON s.SpecialtyID=d.SpecialtyID');
EXEC(N'CREATE VIEW SlotDetails AS
SELECT t.SlotID, w.ScheduleID, w.WorkDate, w.Room, w.Status, t.StartTime, t.EndTime, t.IsBooked,
       d.DoctorID, d.SpecialtyID, s.SpecialtyName, u.FullName AS DoctorName, u.IsActive,
       DATEADD(second,DATEDIFF(second,CAST(''00:00'' AS time),t.StartTime),CAST(w.WorkDate AS datetime2)) AS StartsAt
FROM TimeSlots t JOIN WorkSchedules w ON t.ScheduleID=w.ScheduleID
JOIN Doctors d ON d.DoctorID=w.DoctorID JOIN Users u ON u.UserID=d.UserID
JOIN Specialties s ON s.SpecialtyID=d.SpecialtyID');
INSERT Roles(RoleName) VALUES(N'Admin'),(N'Bác sĩ'),(N'Tiếp nhận'),(N'Bệnh nhân');
INSERT Specialties(SpecialtyName,Description) VALUES
(N'Nội tổng quát',N'Khám nội khoa và tư vấn sức khỏe tổng quát.'),
(N'Tai Mũi Họng',N'Khám các vấn đề về tai, mũi và họng.'),
(N'Da liễu',N'Khám các vấn đề về da, tóc và móng.');
INSERT Users(Username, PasswordHash, FullName, Phone, RoleID) VALUES
('admin','DwIFj5CHEqFyQqcvg4LnOA==.OSzkbTiI+eMfpzW1Ki0e3ClUnhUb52Z5nvxkxO0LtBk=',N'Quản trị EzCare',NULL,1),
('doctor','XgqD4j3biFJkKcOzws/5vg==.nfubdq7sH3TVg8t55K1OR8TDISAHfF9SgS6YRnu0W9E=',N'Nguyễn Minh Anh',NULL,2),
('doctor2','RzriZXxmO67DLH8DDrvC2Q==.IkEy0gMEgWQVVcfSwV3sBqAdzXDrMtzCWqO6lQ9L4sA=',N'Trần Hoàng Nam',NULL,2),
('doctor3','0slB4s/d8ctEx5xCr04RHw==.+qUpltd49eM+Gy4OfpXJ6K783JwvksKQjGico8gmHHg=',N'Lê Thu Hà',NULL,2),
('reception','MvXrvuVaUlHlFCs6bmlF6Q==.k7ib8KMUTnp7YC5/J34cAC5CT9f3NXC1k195LpA7jU4=',N'Phạm Linh Chi',NULL,3),
('patient','m93GweJjb8f/ShwkriFXiA==.QsUHqfP6COkTOibOCxbXl/MR0bSsCjgI0iUrjIt7kCo=',N'Võ Bảo An','0900000001',4);

INSERT dbo.[Password](UserID,[Password]) SELECT UserID,N'EzCare@123' FROM Users;
EXEC(N'CREATE VIEW dbo.PasswordList AS SELECT u.UserID,u.Username,u.FullName,u.RoleID,r.RoleName,u.IsActive,p.[Password],p.UpdatedAt
FROM dbo.Users u JOIN dbo.Roles r ON r.RoleID=u.RoleID
LEFT JOIN dbo.[Password] p ON p.UserID=u.UserID');

INSERT Patients(UserID,FullName,Phone,DateOfBirth) VALUES (6,N'Võ Bảo An','0900000001','2000-08-12');
INSERT Doctors(UserID,SpecialtyID,Degree,ExperienceYears) VALUES
(2,1,N'ThS.BS',8),(3,2,N'BS.CKI',10),(4,1,N'BS.CKI',6);

INSERT WorkSchedules(DoctorID,WorkDate,StartTime,EndTime,SlotDuration,Room)
SELECT d.DoctorID, DATEADD(day, dates.Days, CAST(SYSDATETIME() AS date)),
       '08:00','11:00',30, CONCAT(N'P.',100+d.DoctorID)
FROM Doctors d CROSS JOIN (VALUES (1),(2),(3),(4),(5),(6),(7)) dates(Days);

INSERT TimeSlots(ScheduleID,StartTime,EndTime)
SELECT ScheduleID, DATEADD(minute, x.Minutes, StartTime), DATEADD(minute, x.Minutes+30, StartTime)
FROM WorkSchedules CROSS JOIN (VALUES (0),(30),(60),(90),(120),(150)) x(Minutes);

COMMIT TRANSACTION;
GO

USE EzCare;
GO
IF DB_NAME()<>N'EzCare' OR OBJECT_ID(N'dbo.[Password]',N'U') IS NULL
    THROW 50002,N'Không tìm thấy cấu trúc EzCare. Hãy khởi tạo bằng Setup.sql.',1;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @catalogLock int;
EXEC @catalogLock=sp_getapplock @Resource='EzCare.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=5000;
IF @catalogLock<0 THROW 50003,N'Hệ thống đang ghi dữ liệu. Hãy thử lại.',1;

DECLARE @specialties TABLE(SpecialtyName nvarchar(100),Description nvarchar(500));
INSERT @specialties VALUES
(N'Nội tổng quát',N'Khám nội khoa và tư vấn sức khỏe tổng quát.'),
(N'Tai Mũi Họng',N'Khám các vấn đề về tai, mũi và họng.'),
(N'Da liễu',N'Khám các vấn đề về da, tóc và móng.'),
(N'Tim mạch',N'Khám tim mạch, theo dõi huyết áp và sức khỏe tim.'),
(N'Tiêu hóa',N'Khám các vấn đề về dạ dày, đường ruột và gan mật.'),
(N'Hô hấp',N'Khám các vấn đề về đường hô hấp và phổi.'),
(N'Nội tiết',N'Khám nội tiết, theo dõi đái tháo đường và tuyến giáp.'),
(N'Thần kinh',N'Khám và tư vấn các vấn đề về thần kinh.'),
(N'Cơ xương khớp',N'Khám các vấn đề về cơ, xương, khớp và vận động.'),
(N'Nhi khoa',N'Khám sức khỏe và theo dõi sự phát triển của trẻ.'),
(N'Sản phụ khoa',N'Khám phụ khoa, theo dõi thai kỳ và tư vấn sức khỏe phụ nữ.'),
(N'Mắt',N'Khám mắt, kiểm tra thị lực và tư vấn chăm sóc mắt.'),
(N'Răng Hàm Mặt',N'Khám răng miệng và tư vấn chăm sóc răng, nướu.');
INSERT Specialties(SpecialtyName,Description)
SELECT seed.SpecialtyName,seed.Description FROM @specialties seed
WHERE NOT EXISTS(SELECT 1 FROM Specialties s WHERE s.SpecialtyName=seed.SpecialtyName);

DECLARE @doctors TABLE(Username varchar(50),FullName nvarchar(100),SpecialtyName nvarchar(100),Degree nvarchar(100),ExperienceYears int,Biography nvarchar(2000),PasswordHash varchar(255));
INSERT @doctors VALUES
(N'doctor',N'Nguyễn Minh Anh',N'Nội tổng quát',N'ThS.BS',8,N'Khám nội khoa và tư vấn kiểm tra sức khỏe định kỳ.',N'r7plkv/wqmT4NEf8L8WJhw==.LGJys4nOSsbbPmJ6p9hwcimUuQSDikVWnkxG+ko0LKA='),
(N'doctor2',N'Trần Hoàng Nam',N'Tai Mũi Họng',N'BS.CKI',10,N'Khám tai, mũi, họng cho người lớn và trẻ em.',N'uW2P+b4bdx2k5gLh5c7EkA==.qbpg5OlZVUmulkFL8jgeDIn3Sz87l1y6Aeueo1Liv84='),
(N'doctor3',N'Lê Thu Hà',N'Nội tổng quát',N'BS.CKI',6,N'Khám sức khỏe tổng quát và theo dõi các bệnh nội khoa thường gặp.',N'DakeREX22w3XKwYky4v0Zg==.MdaKIQE8dVuZZnZMEyOQLYJxa9nTy1sSzZ/ypHEBcfU='),
(N'doctor4',N'Phạm Quang Huy',N'Tai Mũi Họng',N'ThS.BS',12,N'Khám và tư vấn các vấn đề về tai, mũi, họng và giọng nói.',N'WCPHyhO2Qz8q92lGbA41eA==.WZRFDxMCCtS3/kYfnKBFxbUAIqXtNnXYL330qim7UKU='),
(N'doctor5',N'Nguyễn Ngọc Mai',N'Da liễu',N'BS.CKI',9,N'Khám các vấn đề về da và tư vấn chăm sóc da.',N'R6SdOy9zOMS3tItvT71yAw==.YbkprUuQlY3+7mYA7PLkyWdikJGivDWKDH6u8uo5J2A='),
(N'doctor6',N'Trần Minh Khôi',N'Da liễu',N'ThS.BS',11,N'Khám da, tóc, móng và theo dõi tình trạng da theo lịch hẹn.',N'dKN4ku79utMY9P/XWjGfCw==.qxo3+aCGFD15k0uLBRjEtT70meJCMe979dfH5ev1s6g='),
(N'doctor7',N'Bùi Đức Long',N'Tim mạch',N'TS.BS',16,N'Khám tim mạch, kiểm tra huyết áp và tư vấn theo dõi sức khỏe tim.',N'92AeOOy4iiRssfkUIfKskg==.sSRCqsO6TDevH3QJJdgOw72MGbuUXj97VQ/d5gJv8uw='),
(N'doctor8',N'Lê Thanh Tâm',N'Tim mạch',N'BS.CKII',14,N'Khám và theo dõi các vấn đề tim mạch ở người trưởng thành.',N'Sez9sIbHuPBkezOkyvEh5w==.3HtcJwskeyGN8eg3kipWZtULe1Pa5OD37AMYvlCRgtk='),
(N'doctor9',N'Vũ Quốc Bảo',N'Tiêu hóa',N'ThS.BS',10,N'Khám các vấn đề về dạ dày, đường ruột và gan mật.',N'OHXZjens/9Nrk/lDvCHSnw==.LwjuEZ4NPXyOhJCFKTy9/+BQX/aBOR7D4jMC7BNuEOc='),
(N'doctor10',N'Đặng Thu Hương',N'Tiêu hóa',N'BS.CKI',8,N'Khám tiêu hóa và tư vấn theo dõi sức khỏe đường ruột.',N'wGYRBGsJuB1mB00uBXWamg==.09xocUlP3nAx90d1aCRp7/qaC3Ixygw1niCkxMcoeiM='),
(N'doctor11',N'Trịnh Minh Đức',N'Hô hấp',N'BS.CKII',15,N'Khám các vấn đề về đường hô hấp và theo dõi sức khỏe phổi.',N'69nOHWGM1660u++3jMZmFQ==.WhzjxAUz+JeRt/jlg0t/lZqxVRIxSi51cMXiynOOZVc='),
(N'doctor12',N'Ngô Hải Yến',N'Hô hấp',N'ThS.BS',9,N'Khám hô hấp và tư vấn chăm sóc sức khỏe đường thở.',N'V9P0nFi/rk4eHhgialyAAA==.glxJQqRYpCU+sk6TU9/fNeuYtmKyQoqyBbj8pTRtSRQ='),
(N'doctor13',N'Đỗ Phương Linh',N'Nội tiết',N'ThS.BS',12,N'Khám nội tiết và theo dõi đái tháo đường, tuyến giáp.',N'Lvb35pXoGcAZ1SzaK7RuGA==.Yp/Of9vv40jZNWopWqNwI71H6P/B5DOSZVRAiqIindo='),
(N'doctor14',N'Hoàng Văn Phúc',N'Nội tiết',N'BS.CKI',10,N'Khám và tư vấn theo dõi các vấn đề nội tiết, chuyển hóa.',N'BEAK/xxz/0OncomoU3iS8A==.T7dvItHIUTMje5OjBtra2erCA37jwje7fgytgaYPO+g='),
(N'doctor15',N'Phan Anh Tuấn',N'Thần kinh',N'TS.BS',18,N'Khám và theo dõi các vấn đề về thần kinh theo lịch hẹn.',N'BGI8XRdG7jvLH6iXh1TURw==.6kJ3+06WDli+iIgAVu5klRyDvgyCaX3H5RUNdcFOFh8='),
(N'doctor16',N'Nguyễn Khánh Vy',N'Thần kinh',N'ThS.BS',8,N'Khám thần kinh và tư vấn theo dõi các triệu chứng thường gặp.',N'NYKZBgupgvGxhd/JdGNNPw==.St16kPOpmjLgG/M1ATgs4zJvLYz6d85VFr10AgGL6LI='),
(N'doctor17',N'Trần Quốc Khánh',N'Cơ xương khớp',N'BS.CKII',16,N'Khám các vấn đề về cơ, xương, khớp và cột sống.',N'4PLCgEp7OVpvF299nY3XAQ==.YFN/Gm9O8FbBgLsbuQOg7azE8utA3hO86JV+i9TK3Ns='),
(N'doctor18',N'Lê Ngọc Trâm',N'Cơ xương khớp',N'BS.CKI',11,N'Khám cơ xương khớp và tư vấn theo dõi khả năng vận động.',N'+aqs4FlBY/XdoZnKgYHnUA==./qBFW4rFNhE4OgKoEu+ED7+2UP4gLHKDwCztE5b2lNA='),
(N'doctor19',N'Võ Thảo Nhi',N'Nhi khoa',N'ThS.BS',9,N'Khám nhi và theo dõi sức khỏe, sự phát triển của trẻ.',N'PhSH17PBRorwdNbCqfYP3g==.HFJWoiFkquiI65aCT5oIL9CJFGjpkEIDSGn2DCncqJQ='),
(N'doctor20',N'Mai Hữu Phước',N'Nhi khoa',N'BS.CKII',17,N'Khám sức khỏe trẻ em và tư vấn chăm sóc trẻ theo độ tuổi.',N'RoSk6FehEWBHt7ufyXFM2Q==.3r+zfzdTMatGe4smON4ueyW28Nk+Fv+3muJSBANq3dA='),
(N'doctor21',N'Nguyễn Bảo Châu',N'Sản phụ khoa',N'BS.CKII',14,N'Khám phụ khoa và theo dõi thai kỳ theo lịch hẹn.',N'k355rEe2Pux0rFSSwGwexg==.TFb78eakYP9zLm9psywtWi4rLBQWMY9lRNRC8Pl/KPE='),
(N'doctor22',N'Phạm Thị Minh Tâm',N'Sản phụ khoa',N'ThS.BS',10,N'Khám phụ khoa và tư vấn chăm sóc sức khỏe phụ nữ.',N'mWm0k9QwY9HnD4CcQQUt4A==.DN3zBwSVotdUfKoISmQMNfkFxm1SleW8JGDym3+vbFY='),
(N'doctor23',N'Đinh Hoàng An',N'Mắt',N'BS.CKI',12,N'Khám mắt, kiểm tra thị lực và tư vấn chăm sóc mắt.',N'f3KV5IAJlb8QS7ftnLD/Vw==.Pwq4T48QWYzvPoI+jlLmWLokgUre/7kWveCGJG4twhs='),
(N'doctor24',N'Lý Kim Ngân',N'Mắt',N'ThS.BS',8,N'Khám các vấn đề về mắt và theo dõi thị lực.',N'J2w92XXvYk4dLtbboOeQjQ==.oP9gCC2HuhFDHBoTRG7LZGXwfFwJmaZW6kf9bCQiRp4='),
(N'doctor25',N'Trương Minh Nhật',N'Răng Hàm Mặt',N'BS.CKII',15,N'Khám răng miệng và tư vấn chăm sóc răng, nướu.',N'kAqPjsMqtbqMuO90YYsO1Q==.c9IHsNaGrnSRhcCFAd3KKSZlEAg+57wvCUeQpT/DXps='),
(N'doctor26',N'Đoàn Thu Uyên',N'Răng Hàm Mặt',N'BS.CKI',9,N'Khám răng, hàm, mặt và theo dõi sức khỏe răng miệng.',N'B0pDUAMBazAVoMqsUd2fXA==.UB0EJaPdA9s3GaAxr/Ybf0kmXDuIxpBESc3D5vu63BY=');

DECLARE @newUsers TABLE(UserID int);
INSERT Users(Username,PasswordHash,FullName,Phone,Email,RoleID)
OUTPUT INSERTED.UserID INTO @newUsers
SELECT seed.Username,seed.PasswordHash,seed.FullName,
       '0908'+RIGHT('000000'+SUBSTRING(seed.Username,7,10),6),seed.Username+'@ezcare.example',2
FROM @doctors seed WHERE NOT EXISTS(SELECT 1 FROM Users u WHERE u.Username=seed.Username);
INSERT dbo.[Password](UserID,[Password]) SELECT UserID,N'EzCare@123' FROM @newUsers;
INSERT Doctors(UserID,SpecialtyID,Degree,ExperienceYears,Biography)
SELECT u.UserID,s.SpecialtyID,seed.Degree,seed.ExperienceYears,seed.Biography
FROM @newUsers created JOIN Users u ON u.UserID=created.UserID
JOIN @doctors seed ON seed.Username=u.Username JOIN Specialties s ON s.SpecialtyName=seed.SpecialtyName;

UPDATE d SET Biography=seed.Biography
FROM Doctors d JOIN Users u ON u.UserID=d.UserID
JOIN @doctors seed ON seed.Username=u.Username AND seed.FullName=u.FullName
JOIN Specialties s ON s.SpecialtyID=d.SpecialtyID AND s.SpecialtyName=seed.SpecialtyName
WHERE NULLIF(LTRIM(RTRIM(d.Biography)),N'') IS NULL;

DECLARE @newSchedules TABLE(ScheduleID int,StartTime time(0));
;WITH days AS (
    SELECT DATEADD(day,dayNumbers.n,CAST(SYSDATETIME() AS date)) AS WorkDate
    FROM (VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14)) dayNumbers(n)
), shifts AS (
    SELECT d.DoctorID,days.WorkDate,
        CAST(CASE WHEN (DATEDIFF(day,'20000103',days.WorkDate)+d.DoctorID)%2=0 THEN '08:00' ELSE '13:30' END AS time(0)) AS StartTime,
        CAST(CASE WHEN (DATEDIFF(day,'20000103',days.WorkDate)+d.DoctorID)%2=0 THEN '11:00' ELSE '16:30' END AS time(0)) AS EndTime
    FROM Doctors d JOIN Users u ON u.UserID=d.UserID
    JOIN @doctors seed ON seed.Username=u.Username AND seed.FullName=u.FullName
    JOIN Specialties s ON s.SpecialtyID=d.SpecialtyID AND s.SpecialtyName=seed.SpecialtyName
    CROSS JOIN days
    WHERE u.IsActive=1 AND DATEDIFF(day,'20000103',days.WorkDate)%7<>6
)
INSERT WorkSchedules(DoctorID,WorkDate,StartTime,EndTime,SlotDuration,Room)
OUTPUT INSERTED.ScheduleID,INSERTED.StartTime INTO @newSchedules
SELECT seed.DoctorID,seed.WorkDate,seed.StartTime,seed.EndTime,30,CONCAT(N'P.',100+seed.DoctorID)
FROM shifts seed WHERE NOT EXISTS(
    SELECT 1 FROM WorkSchedules w WHERE w.DoctorID=seed.DoctorID AND w.WorkDate=seed.WorkDate
        AND w.StartTime<seed.EndTime AND w.EndTime>seed.StartTime
);
INSERT TimeSlots(ScheduleID,StartTime,EndTime)
SELECT seed.ScheduleID,DATEADD(minute,steps.n*30,seed.StartTime),DATEADD(minute,(steps.n+1)*30,seed.StartTime)
FROM @newSchedules seed CROSS JOIN (VALUES(0),(1),(2),(3),(4),(5)) steps(n);
COMMIT TRANSACTION;
GO
