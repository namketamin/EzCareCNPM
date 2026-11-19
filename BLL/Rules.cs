using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public sealed class RuleException : Exception
{
    public RuleException(string message) : base(message) { }
}

public static class Rules
{
    public static void Require(bool condition, string message)
    {
        if (!condition) throw new RuleException(message);
    }

    public static string Text(string value, string label, int length, bool required = true)
    {
        value = (value ?? "").Trim();
        Require(!required || value.Length > 0, "Vui lòng nhập " + label + ".");
        Require(value.Length <= length, label + " tối đa " + length + " ký tự.");
        return value.Length == 0 ? null : value;
    }

    public static string Phone(string value, bool required)
    {
        value = Text(value, "số điện thoại", 15, required);
        Require(value == null || Regex.IsMatch(value, @"^\+?[0-9]{8,14}$"), "Số điện thoại không hợp lệ.");
        return value;
    }

    public static DateTime Date(string value)
    {
        DateTime day;
        Require(DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out day) && day.Year >= 1900, "Ngày không hợp lệ.");
        return day.Date;
    }

    public static DateTime? Birthday(string value)
    {
        if (String.IsNullOrWhiteSpace(value)) return null;
        DateTime day = Date(value);
        Require(day <= DateTime.Today, "Ngày sinh không thể ở tương lai.");
        return day;
    }

    public static TimeSpan Time(string value)
    {
        TimeSpan time;
        Require(TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out time),
            "Giờ không hợp lệ.");
        return time;
    }

    public static DataRow Row(DataTable table, string message)
    {
        Require(table.Rows.Count > 0, message);
        return table.Rows[0];
    }
}

public static class Passwords
{
    private const int Iterations = 120000;

    private static byte[] Derive(string password, byte[] salt)
    {
        using (var hash = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            return hash.GetBytes(32);
    }

    public static string Hash(string password)
    {
        Rules.Require(password != null && password.Length >= 8 && password.Length <= 128,
            "Mật khẩu cần từ 8 đến 128 ký tự.");
        byte[] salt = new byte[16];
        using (var random = RandomNumberGenerator.Create()) random.GetBytes(salt);
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(Derive(password, salt));
    }

    public static bool Verify(string password, string stored)
    {
        if (password == null || password.Length > 128) return false;
        if (String.IsNullOrEmpty(stored)) return false;
        string[] parts = stored.Split('.');
        if (parts.Length != 2) return false;
        byte[] expected, salt;
        try { expected=Convert.FromBase64String(parts[1]); salt=Convert.FromBase64String(parts[0]); }
        catch (FormatException) { return false; }
        if (expected.Length!=32 || salt.Length!=16) return false;
        byte[] actual = Derive(password, salt);
        int difference = actual.Length ^ expected.Length;
        for (int i = 0; i < Math.Min(actual.Length, expected.Length); i++)
            difference |= actual[i] ^ expected[i];
        return difference == 0;
    }
}

public sealed class Actor
{
    public const int Admin = 1, Doctor = 2, Reception = 3, Patient = 4;
    public int Id { get; private set; }
    public int Role { get; private set; }
    public string Name { get; private set; }

    public Actor(DataRow row)
    {
        Id = (int)row["UserID"];
        Role = (int)row["RoleID"];
        Name = (string)row["FullName"];
    }

    public void Allow(params int[] roles)
    {
        Rules.Require(Array.IndexOf(roles, Role) >= 0, "Bạn không có quyền thực hiện thao tác này.");
    }
}

