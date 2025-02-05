using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using DailyExpenseTracker.common;
using DailyExpenseTracker.IRepository;
using DailyExpenseTracker.Data;
using Microsoft.Data.SqlClient;
using DailyExpenseTracker.Entity.Response;

namespace DailyExpenseTracker.Repository
{
    public class ExpenseDetailsRepository : IExpenseDetailsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly DapperDbContext _dbContext;

        public ExpenseDetailsRepository(IConfiguration configuration, DapperDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ExpenseResponse>> GetAllAsync()
        {
            using (var conn = _dbContext.CreateConnection())
            {
                return (await conn.QueryAsync<ExpenseResponse>(StoredProcedure.GetAllExpensesProcedure, commandType: CommandType.StoredProcedure)).ToList();
            }
        }

        // Get an expense by ID, with userId parameter
        public async Task<IEnumerable<ExpenseResponse>> GetExpenseByIdAsync(int userId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var expenses = await conn.QueryAsync<ExpenseResponse>(
                    StoredProcedure.GetExpenseByIdProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return expenses; // Return IEnumerable<ExpenseResponse>
            }
        }



        //public async Task<ExpenseResponse> GetExpenseByUserAndExpenseIdAsync(int userId, int expenseId)
        //{
        //    using (var conn = _dbContext.CreateConnection())
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@UserId", userId);
        //        parameters.Add("@ExpenseId", expenseId);

        //        // Fetch the expense by both UserId and ExpenseId
        //        return await conn.QuerySingleOrDefaultAsync<ExpenseResponse>(
        //            StoredProcedure.GetExpenseByIdProcedure,
        //            parameters,
        //            commandType: CommandType.StoredProcedure
        //        );
        //    }
        //}

        //// Get an expense by ExpenseId and UserId
        //public async Task<ExpenseResponse> GetExpenseByUserAndExpenseIdAsync(int expenseId, int userId)
        //{
        //    using (var conn = _dbContext.CreateConnection())
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@ExpenseId", expenseId);

        //        parameters.Add("@UserId", userId);

        //        return await conn.QuerySingleOrDefaultAsync<ExpenseResponse>(StoredProcedure.GetExpenseByIdProcedure, parameters, commandType: CommandType.StoredProcedure);
        //    }
        //}


        // Change the return type to IEnumerable<ExpenseResponse>
        public async Task<IEnumerable<ExpenseResponse>> GetExpenseByUserAndExpenseIdAsync(int expenseId, int userId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ExpenseId", expenseId);
                parameters.Add("@UserId", userId);

                var expenses = await conn.QueryAsync<ExpenseResponse>(
                    StoredProcedure.GetExpenseByUserAndExpenseIdProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return expenses; // This now returns a collection of ExpenseResponse
            }
        }


        public async Task<ExpenseResponse> GetExpenseDetailById(int expenseId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ExpenseId", expenseId);
              
             return await conn.QueryFirstAsync<ExpenseResponse>(
                    StoredProcedure.GetExpenseDetailByIdProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                 // This now returns a collection of ExpenseResponse
            }
        }


        // Add a new expense, passing the model directly
        public async Task AddExpenseAsync(ExpenseResponse expense, int userId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Category", expense.Category);
                parameters.Add("@Amount", expense.Amount);
                parameters.Add("@Description", expense.Description);
                parameters.Add("@Date", expense.Date);
                parameters.Add("@UserId", userId);
                await conn.ExecuteAsync(StoredProcedure.AddExpenseProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        // Update an existing expense, passing the model directly
        public async Task<ResponseMessage> UpdateExpenseAsync(ExpenseResponse expense)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ExpenseId", expense.ExpenseId); 
                parameters.Add("@Amount", expense.Amount);
                parameters.Add("@Category", expense.Category);
                parameters.Add("@Description", expense.Description);
                parameters.Add("@UserId", expense.UserId);

                return await conn.QueryFirstOrDefaultAsync<ResponseMessage>(StoredProcedure.UpdateExpenseProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        // Delete an expense, with userId parameter
        public async Task DeleteExpenseAsync(int expenseId,int userId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ExpenseId", expenseId);
                parameters.Add("@UserId", userId);

                await conn.ExecuteAsync(StoredProcedure.DeleteExpenseProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }



        public async Task<decimal> GetTotalExpensesByUser(int userId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var query = "SELECT ISNULL(SUM(Amount), 0) FROM Expenses WHERE UserId = @UserId";

                return await conn.ExecuteScalarAsync<decimal>(query, parameters);
            }
        }


        public async Task<int> GetTotalCategoriesByUser(int userId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                // Execute SQL or Stored Procedure
                return await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(DISTINCT Category) FROM Expenses WHERE UserId = @UserId",
                    parameters
                );
            }
        }



        public async Task<IEnumerable<ExpenseResponse>> GetExpensesByUserIdAsync(int userId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var query = "SELECT * FROM Expenses WHERE UserId = @UserId";

                return await conn.QueryAsync<ExpenseResponse>(query, parameters);
            }
        }




        // Fetch response message by expense ID
        public async Task<ResponseMessage> GetResponseMessageByIdAsync(int expenseId)
        {
            using (var conn = _dbContext.CreateConnection())
            {
                var query = "SELECT Message, Data, IsSuccess FROM ResponseMessage WHERE Id = @ExpenseId";
                return await conn.QueryFirstOrDefaultAsync<ResponseMessage>(query, new { ExpenseId = expenseId });
            }
        }
    }
}
