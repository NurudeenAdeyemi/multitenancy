using System.Data.SqlClient;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Infrastructure.Common;
using FSH.WebApi.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public class MakeSecureConnectionString : IMakeSecureConnectionString
{
    private const string HiddenValueDefault = "*******";
    private readonly DatabaseSettings _dbSettings;

    public MakeSecureConnectionString(IOptions<DatabaseSettings> dbSettings) =>
        _dbSettings = dbSettings.Value;

    public string? MakeSecure(string? connectionString, string? dbProvider)
    {
        if (connectionString == null)
        {
            return connectionString;
        }

        if (string.IsNullOrWhiteSpace(dbProvider))
        {
            dbProvider = _dbSettings.DBProvider;
        }

        return dbProvider?.ToLower() switch
        {
            DbProviderKeys.Npgsql => MakeSecureNpgsqlConnectionString(connectionString),
            DbProviderKeys.SqlServer => MakeSecureSqlConnectionString(connectionString),
            DbProviderKeys.MySql => MakeSecureMySqlConnectionString(connectionString),
            DbProviderKeys.Oracle => MakeSecureOracleConnectionString(connectionString),
            _ => connectionString
        };
    }

    private string MakeSecureOracleConnectionString(string connectionString)
    {
        var builder = new OracleConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID))
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }

    private string MakeSecureMySqlConnectionString(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID))
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }

    private string MakeSecureSqlConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) || !builder.IntegratedSecurity)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID) || !builder.IntegratedSecurity)
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }

    private string MakeSecureNpgsqlConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) || !builder.IntegratedSecurity)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.Username) || !builder.IntegratedSecurity)
        {
            builder.Username = HiddenValueDefault;
        }

        return builder.ToString();
    }
}