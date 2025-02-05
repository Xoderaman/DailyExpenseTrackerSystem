using DailyExpenseTracker.Entity.Response;
using DailyExpenseTracker.IRepository;
using Microsoft.AspNetCore.Mvc;
using jsreport.Local;
using jsreport.Binary;
using jsreport.Types;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using jsreport.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace DailyExpenseTracker.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly IExpenseDetailsRepository _expenseDetailsRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor injects the repository
        public ExpenseController(IExpenseDetailsRepository expenseDetailsRepository, IWebHostEnvironment webHostEnvironment)
        {
            _expenseDetailsRepository = expenseDetailsRepository;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account"); // Redirect if not logged in
            }

            // Fetch the logged-in user's expenses
            IEnumerable<ExpenseResponse> expenses = await _expenseDetailsRepository.GetExpenseByIdAsync(userId.Value);

            if (expenses == null || !expenses.Any())
            {
                return NotFound("No expenses found.");
            }

            // Encrypt expense IDs before passing them to the view
            var encryptedExpenses = expenses.Select(expense => new
            {
                EncryptedId = EncryptExpenseId(expense.ExpenseId),
                Expense = expense
            }).ToList();

            // Fetch total expenses & categories count for the logged-in user
            var totalTransactionAmount = await _expenseDetailsRepository.GetTotalExpensesByUser(userId.Value);
            var categoryCount = await _expenseDetailsRepository.GetTotalCategoriesByUser(userId.Value);

            // Debugging output
            Console.WriteLine($"Total Expense for User {userId}: {totalTransactionAmount}");

            // Store values in ViewBag
            ViewBag.TotalTransactionAmount = totalTransactionAmount;
            ViewBag.CategoryCount = categoryCount;

            // Group expenses by category
            var expenseByCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount)
                }).ToList();

            // Calculate total sales
            ViewBag.TotalSales = expenseByCategory.Sum(e => e.TotalAmount);
            ViewBag.ExpenseByCategory = expenseByCategory;

            // 🔹 Fix: Store categories & amounts in ViewBag for the chart
            ViewBag.Categories = expenseByCategory.Select(e => e.Category).ToList();
            ViewBag.Amounts = expenseByCategory.Select(e => e.TotalAmount).ToList();

            return View(encryptedExpenses);
        }



        public IActionResult Report()
        {
            return View();
        }





        public async Task<IActionResult> DownloadReport()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("User must be logged in to download the report.");
            }

            // Fetch expenses for the logged-in user
            var expenses = await _expenseDetailsRepository.GetExpenseByIdAsync(userId.Value);

            if (expenses == null || !expenses.Any())
            {
                return NotFound("No expenses found for the report.");
            }

            // Group data to display in the report
            var reportData = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount)
                }).ToList();

            // Calculate the total expenses
            var totalExpenses = expenses.Sum(e => e.Amount);

            // Create an instance of jsreport Local with proper configuration
            var jsreport = new LocalReporting()
                .UseBinary(JsReportBinary.GetBinary())
                .AsUtility()  // Set the reporting utility mode
                .Create();

            // Define the template path (use the wwwroot folder path correctly)
            var templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "Templates", "template.html");

            if (!System.IO.File.Exists(templatePath))
            {
                return NotFound("Template file not found.");
            }

            // Create the report options with the template and data
            var result = await jsreport.RenderAsync(new RenderRequest
            {
                Template = new Template
                {
                    Content = System.IO.File.ReadAllText(templatePath), // Read the template from the file
                    Engine = Engine.Handlebars,  // Use handlebars engine (jsreport supports multiple engines)
                    Recipe = Recipe.ChromePdf,  // Use the chrome-pdf recipe to generate PDF
                },
                Data = new
                {
                    ReportData = reportData,  // Pass the dynamic report data to the template
                    TotalExpenses = totalExpenses  // Pass the total expenses to the template
                }
            });

            // Return the PDF file as a download response
            return File(result.Content, "application/pdf", "GeneratedReport.pdf");
        }








        //        public async Task<IActionResult> DownloadReport()
        //        {
        //            var userId = HttpContext.Session.GetInt32("UserId");

        //            if (userId == null)
        //            {
        //                return Unauthorized("User must be logged in to download the report.");
        //            }

        //            // Fetch expenses for the logged-in user
        //            var expenses = await _expenseDetailsRepository.GetExpenseByIdAsync(userId.Value);

        //            if (expenses == null || !expenses.Any())
        //            {
        //                return NotFound("No expenses found for the report.");
        //            }

        //            // Group data to display in the report
        //            var reportData = expenses
        //                .GroupBy(e => e.Category)
        //                .Select(g => new
        //                {
        //                    Category = g.Key,
        //                    TotalAmount = g.Sum(e => e.Amount)
        //                }).ToList();

        //            // Populate ViewBag with data to display in the view before downloading the report
        //            ViewBag.ReportData = reportData;
        //            ViewBag.TotalExpenses = expenses.Sum(e => e.Amount);
        //            ViewBag.TotalCategories = reportData.Count;

        //            // Log the data to verify it's correct
        //            Console.WriteLine("Report Data: " + string.Join(", ", reportData.Select(r => $"Category: {r.Category}, TotalAmount: {r.TotalAmount}")));

        //            // Prepare the data to send to JSReport's HTTP API
        //            var reportRequest = new
        //            {
        //                template = new
        //                {
        //                    content = @"
        //<h1>Expense Report</h1>
        //<table border='1'>
        //    <thead>
        //        <tr>
        //            <th>Category</th>
        //            <th>Total Amount</th>
        //        </tr>
        //    </thead>
        //    <tbody>
        //        {{#each data}}
        //        <tr>
        //            <td>{{this.Category}}</td>
        //            <td>{{this.TotalAmount}}</td>
        //        </tr>
        //        {{else}}
        //        <tr><td colspan='2'>No expenses found.</td></tr>
        //        {{/each}}
        //    </tbody>
        //</table>",
        //                    engine = "handlebars",
        //                    recipe = "html" // HTML format for report
        //                },
        //                data = new { data = reportData }
        //            };

        //            using (var httpClient = new HttpClient())
        //            {
        //                // Send the request to JSReport's HTTP API
        //                var response = await httpClient.PostAsJsonAsync("http://localhost:5488/api/report", reportRequest);

        //                // Log the response body to verify it
        //                var responseBody = await response.Content.ReadAsStringAsync();
        //                Console.WriteLine("Response from JSReport: " + responseBody);

        //                if (!response.IsSuccessStatusCode)
        //                {
        //                    return StatusCode((int)response.StatusCode, "Error generating the report");
        //                }

        //                // Read the report content as a byte array
        //                var reportContent = await response.Content.ReadAsByteArrayAsync();

        //                // Define the file path where the report will be saved
        //                string reportFilePath = Path.Combine(@"C:\Users\ASUS\Desktop\DailyExpenseTrackerSystem\DailyExpenseTracker\Reports", "ExpenseReport.html");

        //                // Ensure the Reports folder exists
        //                var reportFolder = Path.GetDirectoryName(reportFilePath);
        //                if (!Directory.Exists(reportFolder))
        //                {
        //                    Directory.CreateDirectory(reportFolder);
        //                }

        //                // Save the report as HTML to the specified folder
        //                System.IO.File.WriteAllBytes(reportFilePath, reportContent);

        //                // Check if the file exists at the path
        //                if (!System.IO.File.Exists(reportFilePath))
        //                {
        //                    return NotFound("Failed to create the report file.");
        //                }

        //                // Return the saved report as a file download (HTML file)
        //                return File(System.IO.File.ReadAllBytes(reportFilePath), "text/html", "ExpenseReport.html");
        //            }
        //        }




        // GET: /Expense/Index
        //public async Task<IActionResult> Index()
        //{
        //    IEnumerable<ExpenseResponse> expenses = await _expenseDetailsRepository.GetAllAsync();
        //    var totalAmount = expenses.Sum(expense => expense.Amount);

        //    // Pass the total amount to ViewBag
        //    ViewBag.TotalTransactionAmount = totalAmount;

        //    return View(expenses); // Return the list of expenses to the view
        //}

        // GET: /Expense/Details/{id}
        public async Task<IActionResult> Details()
        {
            // Get the userId from the current logged-in user
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("User must be logged in to view details.");
            }

            // Get a single expense by ID and UserId from the repository
            IEnumerable<ExpenseResponse> expenses = await _expenseDetailsRepository.GetExpenseByIdAsync(userId.Value);
            var expensedetail = expenses.FirstOrDefault();
            if (expenses == null)
            {
                return NotFound($"Expense with ID {userId.Value} not found.");
            }

            return View(expensedetail); // Return the expense details to the view
        }

        // GET: /Expense/Add
        public IActionResult Add()
        {
            return View();
        }

        // POST: /Expense/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ExpenseResponse expense)
        {

            var userId = HttpContext.Session.GetInt32("UserId");


            if (ModelState.IsValid)
            {
                // Add the expense using the repository
                await _expenseDetailsRepository.AddExpenseAsync(expense, userId.Value);
                return RedirectToAction("Index");
            }

            return View(expense); // Return the Add view if there are validation errors
        }

        // GET: /Expense/Edit/{expenseId}
        //public async Task<IActionResult> Edit(int expenseId)
        //{
        //    // Get the userId from the current logged-in user
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return Unauthorized("User must be logged in to edit the expense.");
        //    }

        //    // Get a single expense by ID and UserId from the repository
        //    var expense = await _expenseDetailsRepository.GetExpenseByIdAsync( userId.Value);

        //    if (expense == null)
        //    {
        //        return NotFound($"Expense with ID {expenseId} not found.");
        //    }

        //    return View(expense); // Return the Edit view with the existing expense data
        //}



        private string EncryptExpenseId(int id)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(id.ToString());
            string encryptedExpenseId = WebEncoders.Base64UrlEncode(plainTextBytes); // URL Safe Encoding

            Console.WriteLine($"EncryptedExpenseId: {encryptedExpenseId}");

            return encryptedExpenseId;
        }


        private int DecryptExpenseId(string encryptedId)
        {
            try
            {
                var base64Bytes = WebEncoders.Base64UrlDecode(encryptedId);
                string decryptedText = Encoding.UTF8.GetString(base64Bytes);
                int decryptedExpenseId = int.Parse(decryptedText);

                Console.WriteLine($"Decrypted Expense ID: {decryptedExpenseId}");

                return decryptedExpenseId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption failed for {encryptedId}: {ex.Message}");
                return 0; // Return 0 or handle error properly
            }
        }




        // GET: /Expense/Edit/{expenseId}

        public async Task<IActionResult> Edit(string encryptedExpenseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("User must be logged in to edit the expense.");
            }

            // Decrypt the Expense ID
            int expenseId = DecryptExpenseId(encryptedExpenseId);

            // Fetch expense from database
            var expense = await _expenseDetailsRepository.GetExpenseDetailById(expenseId);

            if (expense == null)
            {
                return NotFound($"Expense with ID {expenseId} not found.");
            }

            return View(expense);
        }




        // POST: /Expense/Edit/{expenseId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int expenseId, ExpenseResponse expense)
        {
            var userId = HttpContext.Session.GetInt32("UserId"); // Retrieve UserId from session

            if (userId == null || userId == 0)
            {
                return Unauthorized("User must be logged in to edit the expense.");
            }

            if (ModelState.IsValid)
            {
                // Ensure the expenseId and userId are set correctly
                expense.ExpenseId = expenseId;
                expense.UserId = userId.Value; // Assign the retrieved user ID

                // Call the repository to update the expense
                var responseMessage = await _expenseDetailsRepository.UpdateExpenseAsync(expense);
                if (responseMessage != null && responseMessage.IsSuccess)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update the expense.");
                }
            }

            return View(expense); // Return the Edit view if validation fails
        }

        // GET: /Expense/Delete/{id}
        public async Task<IActionResult> Delete(string encryptedExpenseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("User must be logged in to delete an expense.");
            }

            try
            {
                // Decrypt the ExpenseId
                int expenseId = DecryptExpenseId(encryptedExpenseId);

                // Get a single expense by ID and UserId from the repository
                IEnumerable<ExpenseResponse> expenses = await _expenseDetailsRepository.GetExpenseByIdAsync(userId.Value);
                if (expenses == null || !expenses.Any(e => e.ExpenseId == expenseId))
                {
                    return NotFound($"Expense with ID {expenseId} not found for the current user.");
                }

                // Delete the expense
                await _expenseDetailsRepository.DeleteExpenseAsync(expenseId, userId.Value);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting expense: {ex.Message}");
            }
        }

    }
}





// POST: /Expense/DeleteConfirmed/{id}
//[HttpPost]
//[ActionName("DeleteConfirmed")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var expense = await _expenseDetailsRepository.GetExpenseByIdAsync(id);
//    if (expense == null)
//    {
//        var responseMessage = await _expenseDetailsRepository.GetResponseMessageByIdAsync(2); // ID for error message
//        ViewBag.ResponseMessage = responseMessage; 
//        return View(); 
//    }

//    // Delete the expense
//    await _expenseDetailsRepository.DeleteExpenseAsync(id);
//    var successMessage = await _expenseDetailsRepository.GetResponseMessageByIdAsync(1); 

//    return RedirectToAction("Index"); 

//}
