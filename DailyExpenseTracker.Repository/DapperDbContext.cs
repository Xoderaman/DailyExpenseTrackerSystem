using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DailyExpenseTracker.Data
{
    public class DapperDbContext
    {
        private readonly string _connectionString;

        // Constructor to inject the configuration
        public DapperDbContext(IConfiguration configuration)
        {
            // Ensure the connection string is fetched properly
            _connectionString = configuration.GetConnectionString("DailyExpenseTracker")
                               ?? throw new InvalidOperationException("Connection string 'DailyExpenseTracker' not found.");
        }

        // Method to create and return a new SQL connection
        public IDbConnection CreateConnection()
        {
            // Return a new SqlConnection using the connection string
            return new SqlConnection(_connectionString);
        }
    }
}
