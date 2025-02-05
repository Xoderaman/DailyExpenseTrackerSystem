using DailyExpenseTracker.Entity.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyExpenseTracker.IRepository
{
    public interface IExpenseDetailsRepository
    {
        Task<IEnumerable<ExpenseResponse>> GetExpenseByIdAsync(int userId);  // Added userId

        Task AddExpenseAsync(ExpenseResponse expense, int userId); // Directly passing the model

        Task<ResponseMessage> UpdateExpenseAsync(ExpenseResponse expense); // Corrected signature
                                                                           // Directly passing the model

        Task<IEnumerable<ExpenseResponse>> GetExpenseByUserAndExpenseIdAsync(int expenseId, int userId);

            //Task<ExpenseResponse> GetExpenseByUserAndExpenseIdAsync( int userId);
            Task DeleteExpenseAsync(int expenseId, int userId);  // Added userId

        Task<IEnumerable<ExpenseResponse>> GetAllAsync();
        Task<ExpenseResponse> GetExpenseDetailById(int expenseId);

        Task<ResponseMessage> GetResponseMessageByIdAsync(int expenseId);
        Task<IEnumerable<ExpenseResponse>> GetExpensesByUserIdAsync(int userId);


        Task<decimal> GetTotalExpensesByUser(int userId);
        Task<int> GetTotalCategoriesByUser(int userId);
    }
}
