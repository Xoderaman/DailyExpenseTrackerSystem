using Microsoft.AspNetCore.Mvc;
using Dapper;
using System;
using System.Threading.Tasks;
using System.Data;
using DailyExpenseTracker.Entity.Response;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.common;

namespace DailyExpenseTracker.Controllers
{
    public class QRScannerController : Controller
    {
        private readonly DapperDbContext _dapperContext;

        public QRScannerController(DapperDbContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public IActionResult ScanQR()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessQR([FromBody] ExpenseResponse expenseData)
        {
            if (expenseData == null)
            {
                return Json(new ResponseMessage
                {
                    Message = "Invalid QR data received.",
                    Data = null,
                    IsSuccess = false
                });
            }

            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Category", expenseData.Category);
                    parameters.Add("@Amount", expenseData.Amount);
                    parameters.Add("@Date", expenseData.Date);
                    parameters.Add("@Description", expenseData.Description);
                    parameters.Add("@UserId", expenseData.UserId);

                    await connection.ExecuteAsync(StoredProcedure.AddExpenseProcedure, parameters, commandType: CommandType.StoredProcedure);

                    return Json(new ResponseMessage
                    {
                        Message = "Expense added successfully!",
                        Data = expenseData,
                        IsSuccess = true
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ResponseMessage
                {
                    Message = "Error processing QR: " + ex.Message,
                    Data = null,
                    IsSuccess = false
                });
            }
        }
    }
}
