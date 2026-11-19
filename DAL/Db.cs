using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

public sealed class Db : IDisposable
{
    private readonly SqlConnection connection;
    private SqlTransaction transaction;

    public Db()
    {
        connection = new SqlConnection(ConfigurationManager.ConnectionStrings["EzCare"].ConnectionString);
        try { connection.Open(); }
        catch { connection.Dispose(); throw; }
    }

    public static SqlParameter P(string name, object value)
    {
        return new SqlParameter(name, value ?? DBNull.Value);
    }

    private SqlCommand Command(string sql, SqlParameter[] parameters)
    {
        var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parameters);
        return command;
    }

    public DataTable Query(string sql, params SqlParameter[] parameters)
    {
        using (var command = Command(sql, parameters))
        using (var reader = command.ExecuteReader())
        {
            var table = new DataTable();
            table.Load(reader);
            return table;
        }
    }

    public object Value(string sql, params SqlParameter[] parameters)
    {
        using (var command = Command(sql, parameters))
            return command.ExecuteScalar();
    }

    public int Execute(string sql, params SqlParameter[] parameters)
    {
        using (var command = Command(sql, parameters))
            return command.ExecuteNonQuery();
    }

    public void Write(Action action)
    {
        using (transaction = connection.BeginTransaction())
        {
            try
            {
                int result = Convert.ToInt32(Value(@"DECLARE @result int;
                    EXEC @result=sp_getapplock @Resource='EzCare.Write',
                        @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000;
                    SELECT @result;"));
                if (result < 0)
                    throw new InvalidOperationException("Hệ thống đang xử lý một thay đổi khác. Vui lòng thử lại.");
                action();
                transaction.Commit();
            }
            finally
            {
                transaction = null;
            }
        }
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}

