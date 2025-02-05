using DailyExpenseTracker.Entity.Request;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.common;

namespace DailyExpenseTracker.Repository
{
    public class UserRepository
    {
        private readonly DapperDbContext _dbContext;

        public UserRepository(DapperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Method to get a user by UserId using stored procedure
        public async Task<UserModel> GetUserByIdAsync(int userId)
        {
            using (var connection = _dbContext.CreateConnection())
            {

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                return await connection.QueryFirstOrDefaultAsync<UserModel>(StoredProcedure.GetUserByIdProcedure, parameters, commandType: CommandType.StoredProcedure);

            }
        }

        // Method to get a user by email using stored procedure
        public async Task<UserModel> GetUserByEmailAsync(string email)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                // Stored procedure name
                var parameters = new DynamicParameters();
                parameters.Add("@Email", email);
                return await connection.QueryFirstOrDefaultAsync<UserModel>(StoredProcedure.GetUserByEmailProcedure, parameters, commandType: CommandType.StoredProcedure);

               
            }
        }


        public async Task<int> UpdateUserAsync(UserModel model)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                var parameters = new
                {
                    model.UserId,
                    model.UserName,
                    model.Email,
                    model.MobileNumber,
                    model.Password
                };

                return await connection.ExecuteAsync(StoredProcedure.UpdateUserProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }




        // Method to add a new user to the database using stored procedure
        public async Task<int> AddUserAsync(UserModel model)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                // The name of the stored procedure to add a user
                var parameters = new
                {
                    model.UserName,
                    model.Email,
                    model.MobileNumber,
                    model.Password
                   
                    // Add other properties as needed
                };

                return await connection.QuerySingleOrDefaultAsync<int>(StoredProcedure.AddUserProcedure, parameters, commandType: CommandType.StoredProcedure);
               
            }
        }
    }
}
