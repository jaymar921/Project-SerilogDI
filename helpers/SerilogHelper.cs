using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace Project_SerilogDI.helpers;

public static class SerilogHelper
{
    public static WebApplicationBuilder InitializeSerilog(this WebApplicationBuilder builder)
    {
        string loggingConnStr = builder.Configuration.GetConnectionString("LoggingDb");

        if(string.IsNullOrEmpty(loggingConnStr))
        {
            throw new InvalidOperationException("Logging connection string is not configured.");
        }

        var columnOptions = new ColumnOptions
        {
            AdditionalColumns = new List<SqlColumn>
            {
                new SqlColumn { ColumnName = "ApplicationName", DataType = System.Data.SqlDbType.VarChar, DataLength = 50 },
                new SqlColumn { ColumnName = "Environment", DataType = System.Data.SqlDbType.VarChar, DataLength = 50 }
            }
        };

        builder.Logging.ClearProviders();

        // Configure Serilog
        // Note: The logger is assigned to Log.Logger to ensure it's available globally for Serilog's static logging methods.
        // This is important for scenarios where you might want to use Serilog's static Log class for logging outside of the DI context.
        // If you prefer to use the logger through DI, you can also register it as a singleton in the service collection.

        var serilogLogger = Log.Logger = new LoggerConfiguration()
            .Enrich.WithProperty("ApplicationName", "ProjectSerilogDI")
            .Enrich.WithProperty("Environment", "DEV")
            .WriteTo
            .MSSqlServer(
                connectionString: loggingConnStr,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "LogEvents",
                    AutoCreateSqlTable = true,
                    AutoCreateSqlDatabase = true
                },
                columnOptions: columnOptions
            )
            .MinimumLevel.Information()
            .CreateLogger();

        builder.Host.UseSerilog(serilogLogger);

        return builder;
    }
}
