using System;
using System.Data;
using System.Text.RegularExpressions;

public sealed class Accounts
{
    private readonly Db db;
    private readonly AccountsDAL data;
    internal Accounts(Db db)
    {
        this.db = db;
        data = new AccountsDAL(db);
    }

    public Actor Current(int id)
    {
        DataTable users = data.GetActiveUser(id);
        return users.Rows.Count == 0 ? null : new Actor(users.Rows[0]);
    }

    public Actor Login(string username, string password)
    {
        username = Rules.Text(username, "tên đăng nhập", 50);
        var users = data.FindLogin(username);
        Rules.Require(users.Rows.Count == 1 && Passwords.Verify(password, (string)users.Rows[0]["PasswordHash"]),
            "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.");
        var actor = new Actor(users.Rows[0]);
        db.Write(() =>
        {
            var current = data.GetActiveUser(actor.Id);
            Rules.Require(current.Rows.Count == 1 && (string)current.Rows[0]["PasswordHash"] == (string)users.Rows[0]["PasswordHash"],
                "Tài khoản vừa được cập nhật. Vui lòng đăng nhập lại.");
            data.SaveReadablePassword(actor.Id,password);
        });
        return actor;
    }

    private string Username(string name)
    {
        name = Rules.Text(name, "tên đăng nhập", 50);
        Rules.Require(Regex.IsMatch(name, @"^[A-Za-z0-9_.-]{3,50}$"),
            "Tên đăng nhập cần ít nhất 3 ký tự và chỉ dùng chữ không dấu, số hoặc . _ -");
        return name;
    }

    private string Email(string email)
    {
        email = Rules.Text(email, "email", 100, false);
        Rules.Require(email == null || Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"), "Email không hợp lệ.");
        return email;
    }

    private void Unique(int id, string username, string email)
    {
        if (username != null)
            Rules.Require(data.FindUsername(username, id) == null, "Tên đăng nhập đã được sử dụng.");
        if (email != null)
            Rules.Require(data.FindEmail(email, id) == null, "Email đã được sử dụng.");
    }

    public void Register(string username, string password, string fullName, string phone, string email,
        string birthday, string gender, string address)
    {
        username = Username(username);
        string hash = Passwords.Hash(password);
        fullName = Rules.Text(fullName, "họ tên", 100);
        phone = Rules.Phone(phone, true);
        email = Email(email);
        db.Write(() =>
        {
            Unique(0, username, email);
            int id = InsertUser(username, hash, fullName, phone, email, Actor.Patient);
            data.SaveReadablePassword(id,password);
            SavePatient(id, fullName, phone, birthday, gender, address, false);
        });
    }

    public DataRow Profile(Actor actor)
    {
        actor.Allow(Actor.Admin, Actor.Doctor, Actor.Reception, Actor.Patient);
        return Rules.Row(actor.Role == Actor.Patient ? data.GetPatientProfile(actor.Id) : data.GetAccountProfile(actor.Id), "Không tìm thấy hồ sơ tài khoản.");
    }

    public void UpdateProfile(Actor actor, string name, string phone, string email, string birthday,
        string gender, string address, string password, string oldPassword)
    {
        actor.Allow(Actor.Admin, Actor.Doctor, Actor.Reception, Actor.Patient);
        name = Rules.Text(name, "họ tên", 100);
        phone = Rules.Phone(phone, actor.Role == Actor.Patient);
        email = Email(email);
        string hash = String.IsNullOrEmpty(password) ? null : Passwords.Hash(password);
        db.Write(() =>
        {
            if (hash != null)
                Rules.Require(Passwords.Verify(oldPassword, (string)data.GetPasswordHash(actor.Id)), "Mật khẩu hiện tại không đúng.");
            Unique(actor.Id, null, email);
            data.UpdateUserProfile(name, phone, email, hash, actor.Id);
            if (hash != null) data.SaveReadablePassword(actor.Id,password);
            if (actor.Role == Actor.Patient) SavePatient(actor.Id, name, phone, birthday, gender, address, true);
        });
    }

    public int AddGuest(Actor actor, string name, string phone, string birthday, string gender, string address)
    {
        actor.Allow(Actor.Admin, Actor.Reception);
        name = Rules.Text(name,"họ tên",100);
        phone = Rules.Phone(phone,true);
        DateTime? birth = Rules.Birthday(birthday);
        int id = 0;
        db.Write(() =>
        {
            object existing = data.FindSamePatient(name,phone,birth);
            id = existing is int ? (int)existing : SavePatient(null,name,phone,birthday,gender,address,false);
        });
        return id;
    }

    private int SavePatient(int? user, string name, string phone, string birthday, string gender, string address, bool update)
    {
        gender = Rules.Text(gender, "giới tính", 10, false);
        Rules.Require(gender == null || gender == "Nam" || gender == "Nữ" || gender == "Khác", "Giới tính không hợp lệ.");
        address = Rules.Text(address, "địa chỉ", 200, false);
        DateTime? birth = Rules.Birthday(birthday);
        var patient = new PatientDTO {
            UserID = user, FullName = name, Phone = phone,
            DateOfBirth = birth, Gender = gender, Address = address
        };
        if (update) return data.UpdatePatient(patient);
        return Convert.ToInt32(data.InsertPatient(patient));
    }

    private int InsertUser(string username, string hash, string name, string phone, string email, int role)
    {
        return (int)data.InsertUser(username, hash, name, phone, email, role);
    }

    public DataTable Staff(Actor actor)
    {
        actor.Allow(Actor.Admin);
        return data.ListStaff();
    }

    public DataTable PasswordList(Actor actor)
    {
        actor.Allow(Actor.Admin);
        return data.ListPasswords();
    }

    public DataTable Search(Actor actor, string search, int role, int? active, int page, int size, bool staffOnly, out int total)
    {
        actor.Allow(Actor.Admin);
        search = Rules.Text(search,"từ khóa tìm kiếm",100,false) ?? "";
        Rules.Require(role >= 0 && role <= 4 && (!staffOnly || role == 0 || role == Actor.Doctor || role == Actor.Reception),"Vai trò không hợp lệ.");
        Rules.Require(!active.HasValue || active == 0 || active == 1,"Trạng thái tài khoản không hợp lệ.");
        Rules.Require(page > 0 && (size == 20 || size == 50 || size == 100),"Trang hoặc số dòng không hợp lệ.");
        return data.SearchAccounts(search,role,active,page,size,staffOnly,out total);
    }

    public DataRow StaffMember(Actor actor, int id)
    {
        actor.Allow(Actor.Admin);
        return Rules.Row(data.GetStaff(id),"Không tìm thấy tài khoản nhân sự.");
    }

    public void SaveStaff(Actor actor, int id, int role, string username, string password, string name,
        string phone, string email, bool active, int specialty, string degree, int? experience, string biography)
    {
        actor.Allow(Actor.Admin);
        Rules.Require(role == Actor.Doctor || role == Actor.Reception, "Vai trò không hợp lệ.");
        username = Username(username);
        name = Rules.Text(name, "họ tên", 100);
        phone = Rules.Phone(phone, false);
        email = Email(email);
        degree = Rules.Text(degree, "học vị", 100, false);
        biography = Rules.Text(biography, "tiểu sử", 2000, false);
        Rules.Require(!experience.HasValue || (experience >= 0 && experience <= 80), "Số năm kinh nghiệm không hợp lệ.");
        string hash = id == 0 || !String.IsNullOrEmpty(password) ? Passwords.Hash(password) : null;
        db.Write(() =>
        {
            Unique(id, username, email);
            if (role == Actor.Doctor)
                Rules.Require(data.FindSpecialty(specialty) != null,
                    "Vui lòng chọn chuyên khoa.");
            if (id == 0)
            {
                id = InsertUser(username, hash, name, phone, email, role);
                if (role == Actor.Doctor)
                    data.InsertDoctor(id, specialty);
            }
            else
            {
                var currentRole = data.GetRole(id);
                Rules.Require(currentRole is int && (int)currentRole == role, "Không tìm thấy tài khoản đúng vai trò.");
                if (role == Actor.Doctor)
                {
                    if (Convert.ToInt32(data.DoctorSpecialty(id)) != specialty)
                        Rules.Require(!data.DoctorHasAppointments(id,false),"Bác sĩ đã có phiếu hẹn. Không đổi chuyên khoa để giữ thông tin và lịch sử phiếu.");
                    if (!active)
                        Rules.Require(!data.DoctorHasAppointments(id,true),"Bác sĩ còn lịch đã xác nhận hoặc đã đến. Hãy xử lý các lịch này trước khi khóa tài khoản.");
                }
            }

            data.UpdateStaff(username, name, phone, email, active, hash, id);
            if (hash != null) data.SaveReadablePassword(id,password);
            if (role == Actor.Doctor)
                data.UpdateDoctor(specialty, degree, experience, biography, id);
        });
    }

    public DataTable FindPatients(Actor actor, string phone)
    {
        actor.Allow(Actor.Admin, Actor.Reception);
        return data.FindPatients(Rules.Phone(phone, true));
    }
}

