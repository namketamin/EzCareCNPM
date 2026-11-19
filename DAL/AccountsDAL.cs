using System;
using System.Data;

public sealed class AccountsDAL
{
    private readonly Db db;
    public AccountsDAL(Db db) { this.db = db; }

    public DataTable GetActiveUser(object id)
    {
        return db.Query("SELECT * FROM Users WHERE UserID=@id AND IsActive=1",
            Db.P("@id", id));
    }

    public DataTable FindLogin(object name)
    {
        return db.Query("SELECT * FROM Users WHERE Username=@name AND IsActive=1",
            Db.P("@name", name));
    }

    public object FindUsername(object name, object id)
    {
        return db.Value("SELECT 1 FROM Users WHERE Username=@name AND UserID<>@id",
            Db.P("@name", name), Db.P("@id", id));
    }

    public object FindEmail(object email, object id)
    {
        return db.Value("SELECT 1 FROM Users WHERE Email=@email AND UserID<>@id",
            Db.P("@email", email), Db.P("@id", id));
    }

    public DataTable GetPatientProfile(object id)
    {
        return db.Query(@"SELECT p.*,u.Email FROM Patients p JOIN Users u ON u.UserID=p.UserID
            WHERE p.UserID=@id",
            Db.P("@id", id));
    }

    public DataTable GetAccountProfile(int id)
    {
        return db.Query("SELECT UserID,FullName,Phone,Email FROM Users WHERE UserID=@id AND IsActive=1",Db.P("@id",id));
    }

    public object GetPasswordHash(object id)
    {
        return db.Value("SELECT PasswordHash FROM Users WHERE UserID=@id",
            Db.P("@id", id));
    }

    public void SaveReadablePassword(int user, string password)
    {
        db.Execute(@"UPDATE dbo.[Password] SET [Password]=@password,UpdatedAt=SYSDATETIME() WHERE UserID=@user
                AND ([Password] COLLATE Latin1_General_100_BIN2<>@password COLLATE Latin1_General_100_BIN2
                    OR DATALENGTH([Password])<>DATALENGTH(@password));
            IF NOT EXISTS(SELECT 1 FROM dbo.[Password] WHERE UserID=@user)
                INSERT dbo.[Password](UserID,[Password]) VALUES(@user,@password)",
            Db.P("@user",user),Db.P("@password",password));
    }

    public DataTable ListPasswords()
    {
        return db.Query("SELECT * FROM dbo.PasswordList ORDER BY RoleID,Username");
    }

    public DataTable SearchAccounts(string search, int role, int? active, int page, int size, bool staffOnly, out int total)
    {
        string pattern = "%" + search.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[") + "%";
        string filter = @" FROM Users u
            LEFT JOIN Doctors d ON d.UserID=u.UserID
            LEFT JOIN Specialties s ON s.SpecialtyID=d.SpecialtyID
            LEFT JOIN dbo.[Password] p ON p.UserID=u.UserID
            WHERE (@staff=0 OR u.RoleID IN (2,3)) AND (@role=0 OR u.RoleID=@role)
              AND (@active IS NULL OR u.IsActive=@active)
              AND (@search=N'' OR u.FullName COLLATE Vietnamese_CI_AI LIKE @pattern ESCAPE N'\'
                   OR u.Username LIKE @pattern ESCAPE N'\' OR u.Phone LIKE @pattern ESCAPE N'\'
                   OR u.Email LIKE @pattern ESCAPE N'\')";
        total = Convert.ToInt32(db.Value("SELECT COUNT(*)" + filter,
            Db.P("@staff",staffOnly),Db.P("@role",role),Db.P("@active",active),Db.P("@search",search),Db.P("@pattern",pattern)));
        int lastPage = total == 0 ? 1 : (total-1)/size+1;
        int offset = (Math.Min(page,lastPage)-1)*size;
        return db.Query(@"SELECT u.UserID,u.Username,u.FullName,u.Phone,u.Email,u.RoleID,u.IsActive,
            d.SpecialtyID,d.Degree,d.ExperienceYears,d.Biography,s.SpecialtyName,p.[Password],p.UpdatedAt" + filter +
            " ORDER BY u.RoleID,u.FullName,u.UserID OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY",
            Db.P("@staff",staffOnly),Db.P("@role",role),Db.P("@active",active),Db.P("@search",search),Db.P("@pattern",pattern),
            Db.P("@offset",offset),Db.P("@size",size));
    }

    public DataTable GetStaff(int id)
    {
        return db.Query(@"SELECT u.UserID,u.Username,u.FullName,u.Phone,u.Email,u.RoleID,u.IsActive,
            d.SpecialtyID,d.Degree,d.ExperienceYears,d.Biography
            FROM Users u LEFT JOIN Doctors d ON d.UserID=u.UserID WHERE u.UserID=@id AND u.RoleID IN (2,3)",Db.P("@id",id));
    }

    public object FindSamePatient(string name, string phone, DateTime? birthday)
    {
        return db.Value(@"SELECT TOP(1) PatientID FROM Patients WHERE FullName=@name AND Phone=@phone
            AND (DateOfBirth=@birth OR (DateOfBirth IS NULL AND @birth IS NULL)) ORDER BY PatientID",
            Db.P("@name",name),Db.P("@phone",phone),Db.P("@birth",birthday));
    }

    public object DoctorSpecialty(int user)
    {
        return db.Value("SELECT SpecialtyID FROM Doctors WHERE UserID=@user",Db.P("@user",user));
    }

    public bool DoctorHasAppointments(int user, bool unresolvedOnly)
    {
        return db.Value(@"SELECT 1 FROM AppointmentDetails WHERE DoctorUserID=@user
            AND (@unresolved=0 OR (Status IN ('Confirmed','CheckedIn') AND WorkDate>=CAST(SYSDATETIME() AS date)))",
            Db.P("@user",user),Db.P("@unresolved",unresolvedOnly))!=null;
    }

    public int UpdateUserProfile(object name, object phone, object email, object hash, object id)
    {
        return db.Execute(@"UPDATE Users SET FullName=@name,Phone=@phone,Email=@email,
                         PasswordHash=COALESCE(@hash,PasswordHash) WHERE UserID=@id",
            Db.P("@name", name), Db.P("@phone", phone), Db.P("@email", email), Db.P("@hash", hash), Db.P("@id", id));
    }

    public int UpdatePatient(PatientDTO patient)
    {
        return db.Execute(@"UPDATE Patients SET FullName=@name,Phone=@phone,DateOfBirth=@birth,Gender=@gender,Address=@address WHERE UserID=@user",
            Db.P("@user",patient.UserID),Db.P("@name",patient.FullName),Db.P("@phone",patient.Phone),
            Db.P("@birth",patient.DateOfBirth),Db.P("@gender",patient.Gender),Db.P("@address",patient.Address));
    }

    public object InsertPatient(PatientDTO patient)
    {
        return db.Value(@"INSERT Patients(UserID,FullName,Phone,DateOfBirth,Gender,Address) OUTPUT INSERTED.PatientID VALUES(@user,@name,@phone,@birth,@gender,@address)",
            Db.P("@user",patient.UserID),Db.P("@name",patient.FullName),Db.P("@phone",patient.Phone),
            Db.P("@birth",patient.DateOfBirth),Db.P("@gender",patient.Gender),Db.P("@address",patient.Address));
    }

    public object InsertUser(object username, object hash, object name, object phone, object email, object role)
    {
        return db.Value(@"INSERT Users(Username,PasswordHash,FullName,Phone,Email,RoleID)
            OUTPUT INSERTED.UserID VALUES(@username,@hash,@name,@phone,@email,@role)",
            Db.P("@username", username), Db.P("@hash", hash), Db.P("@name", name), Db.P("@phone", phone), Db.P("@email", email), Db.P("@role", role));
    }

    public DataTable ListStaff()
    {
        return db.Query(@"SELECT u.UserID,u.Username,u.FullName,u.Phone,u.Email,u.RoleID,u.IsActive,
            d.SpecialtyID,d.Degree,d.ExperienceYears,d.Biography,s.SpecialtyName
            FROM Users u LEFT JOIN Doctors d ON d.UserID=u.UserID
            LEFT JOIN Specialties s ON s.SpecialtyID=d.SpecialtyID
            WHERE u.RoleID IN (2,3) ORDER BY u.RoleID,u.FullName");
    }

    public object FindSpecialty(object id)
    {
        return db.Value("SELECT 1 FROM Specialties WHERE SpecialtyID=@id",
            Db.P("@id", id));
    }

    public int InsertDoctor(object id, object specialty)
    {
        return db.Execute("INSERT Doctors(UserID,SpecialtyID) VALUES(@id,@specialty)",
            Db.P("@id", id), Db.P("@specialty", specialty));
    }

    public object GetRole(object id)
    {
        return db.Value("SELECT RoleID FROM Users WHERE UserID=@id",
            Db.P("@id", id));
    }

    public int UpdateStaff(object username, object name, object phone, object email, object active, object hash, object id)
    {
        return db.Execute(@"UPDATE Users SET Username=@username, FullName=@name, Phone=@phone, Email=@email,
                IsActive=@active, PasswordHash=COALESCE(@hash,PasswordHash) WHERE UserID=@id",
            Db.P("@username", username), Db.P("@name", name), Db.P("@phone", phone), Db.P("@email", email), Db.P("@active", active), Db.P("@hash", hash), Db.P("@id", id));
    }

    public int UpdateDoctor(object specialty, object degree, object experience, object bio, object id)
    {
        return db.Execute(@"UPDATE Doctors SET SpecialtyID=@specialty,Degree=@degree,
                    ExperienceYears=@experience,Biography=@bio WHERE UserID=@id",
            Db.P("@specialty", specialty), Db.P("@degree", degree), Db.P("@experience", experience), Db.P("@bio", bio), Db.P("@id", id));
    }

    public DataTable FindPatients(object phone)
    {
        return db.Query(@"SELECT TOP (50) PatientID, FullName, Phone, DateOfBirth FROM Patients
            WHERE Phone=@phone ORDER BY FullName",
            Db.P("@phone", phone));
    }
}
