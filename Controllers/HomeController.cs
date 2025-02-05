using DailyExpenseTracker.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity;
using System.Diagnostics;
using DailyExpenseTracker.Repository;
using DailyExpenseTracker.Entity.Request;
using System.Globalization;


namespace DailyExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        // private readonly ILogger<HomeController> _logger;
        private readonly IExpenseDetailsRepository _expenseDetailsRepository;
        private readonly UserRepository _userRepository;
        // Constructor to inject logger and dbContext
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;

        //}
        public HomeController(IExpenseDetailsRepository expenseDetailsRepository, UserRepository userRepository)
        {
            _expenseDetailsRepository = expenseDetailsRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index(int? selectedYear)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var expenses = await _expenseDetailsRepository.GetExpensesByUserIdAsync(userId.Value);

            if (expenses == null || !expenses.Any())
            {
                return NotFound("No expenses found for this user.");
            }

            var userName = HttpContext.Session.GetString("UserName");
            ViewData["UserName"] = userName;

            ViewBag.TotalTransactionAmount = expenses.Sum(exp => exp.Amount);
            ViewBag.CategoryCount = expenses.Select(exp => exp.Category).Distinct().Count();

            var expenseData = expenses.GroupBy(exp => exp.Category)
                                      .Select(g => new
                                      {
                                          Category = g.Key,
                                          Amount = g.Sum(exp => exp.Amount)
                                      }).ToList();

            ViewBag.Categories = expenseData.Select(e => e.Category).ToArray();
            ViewBag.Amounts = expenseData.Select(e => e.Amount).ToArray();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            decimal monthlyExpenses = expenses
                .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .Sum(e => e.Amount);

            ViewBag.MonthlyExpenses = monthlyExpenses;

            var monthsCount = expenses
                .Select(e => new { e.Date.Year, e.Date.Month })
                .Distinct()
                .Count();

            decimal yearlyTotalExpenses = expenses
                .Where(e => e.Date.Year == currentYear)
                .Sum(e => e.Amount);

            ViewBag.AvgMonthlySpending = monthsCount > 0 ? yearlyTotalExpenses / monthsCount : 0;

            ViewBag.AvailableYears = expenses
                .Select(e => e.Date.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToArray();

            int yearToFilter = selectedYear ?? currentYear;
            ViewBag.SelectedYear = yearToFilter;

            var yearlyExpenses = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    Month = new DateTime(yearToFilter, m, 1).ToString("MMMM"),
                    TotalAmount = expenses
                        .Where(e => e.Date.Year == yearToFilter && e.Date.Month == m)
                        .Sum(e => e.Amount)
                }).ToList();

            ViewBag.MonthLabels = yearlyExpenses.Select(e => e.Month).ToArray();
            ViewBag.MonthlyAmounts = yearlyExpenses.Select(e => e.TotalAmount).ToArray();

            return View(expenses);
        }




        // Action for the Report page
        //public async Task<IActionResult> Index(int? selectedYear)
        //{
        //    // 🔹 Retrieve logged-in user's ID from session
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Account"); // Redirect if not logged in
        //    }

        //    // 🔹 Fetch only this user's expenses
        //    var expenses = await _expenseDetailsRepository.GetExpensesByUserIdAsync(userId.Value);

        //    if (expenses == null || !expenses.Any())
        //    {
        //        return NotFound("No expenses found for this user.");
        //    }

        //    // 🔹 Calculate total expenses for this user
        //    decimal totalTransactionAmount = expenses.Sum(exp => exp.Amount);

        //    // 🔹 Calculate unique categories for this user
        //    var uniqueCategories = expenses.Select(exp => exp.Category).Distinct().Count();

        //    // 🔹 Get username from session
        //    var userName = HttpContext.Session.GetString("UserName");

        //    // 🔹 Pass UserName to the view
        //    ViewData["UserName"] = userName;

        //    // 🔹 Group expenses by category
        //    var expenseData = expenses.GroupBy(exp => exp.Category)
        //                              .Select(g => new
        //                              {
        //                                  Category = g.Key,
        //                                  Amount = g.Sum(exp => exp.Amount)
        //                              }).ToList();

        //    // 🔹 Pass filtered data to the view
        //    ViewBag.TotalTransactionAmount = totalTransactionAmount;
        //    ViewBag.CategoryCount = uniqueCategories;
        //    ViewBag.Categories = expenseData.Select(e => e.Category).ToArray();
        //    ViewBag.Amounts = expenseData.Select(e => e.Amount).ToArray();


        //    //------------Current Month--------------//
        //    var currentMonth = DateTime.Now.Month;
        //    var currentYear = DateTime.Now.Year;

        //    decimal monthlyExpenses = expenses
        //        .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
        //        .Sum(e => e.Amount);

        //    ViewBag.MonthlyExpenses = monthlyExpenses;

        //    //------Average month spending -----------//

        //    var monthsCount = expenses
        //.Select(e => new { e.Date.Year, e.Date.Month })
        //.Distinct()
        //.Count();

        //    decimal yearlyExpenses = expenses
        //        .Where(e => e.Date.Year == currentYear)
        //        .Sum(e => e.Amount);

        //    decimal avgMonthlySpending = monthsCount > 0 ? yearlyExpenses / monthsCount : 0;

        //    ViewBag.AvgMonthlySpending = avgMonthlySpending;




        //    //---12 month chart -----------//
        //   var availableYears = new[] { 2023, 2024, 2025 };
        //    ViewBag.AvailableYears = availableYears;

        //    // 🔹 Default to current year if no year is selected
        //    int yearToFilter = selectedYear ?? DateTime.Now.Year;
        //    ViewBag.SelectedYear = yearToFilter;

        //    // 🔹 Filter expenses for the selected year
        //    var yearlyExpenses = Enumerable.Range(1, 12)
        //        .Select(m => new
        //        {
        //            Month = m,
        //            TotalAmount = expenses
        //                .Where(e => e.Date.Year == yearToFilter && e.Date.Month == m)
        //                .Sum(e => e.Amount)
        //        }).ToList();

        //    ViewBag.MonthLabels = yearlyExpenses
        //        .Select(e => new DateTime(yearToFilter, e.Month, 1).ToString("MMMM"))
        //        .ToArray();

        //    ViewBag.MonthlyAmounts = yearlyExpenses.Select(e => e.TotalAmount).ToArray();











        //    return View(expenses);
        //}










        public IActionResult LandingPage()
        {
            return View();
        }

        // Action for adding or viewing a transaction
        public IActionResult Transaction()
        {
            return View();
        }

        // Action for budget view
        public IActionResult Buget()
        {
            return View("~/Views/Budget/Buget.cshtml");
        }

        // Action for displaying and editing profile

        // Action for error handling
        public ActionResult HttpNotFound(string message)
        {
            return NotFound(message);
        }

        // Action for privacy policy view
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Test()
        {
            return Content("Test page is working.");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userRepository.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user);
        }

        // ✅ GET: Update Profile Page
        public async Task<IActionResult> UpdateProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userRepository.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user);
        }

        // ✅ POST: Update Profile using Dapper
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserModel model)
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingUser = await _userRepository.GetUserByIdAsync(userId.Value);
            if (existingUser != null)
            {
                existingUser.UserName = model.UserName;
                existingUser.Email = model.Email;
                existingUser.MobileNumber = model.MobileNumber;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    existingUser.Password = model.Password; // Hash the password before saving
                }

                var updateResult = await _userRepository.UpdateUserAsync(existingUser);
                if (updateResult > 0)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
            }

            TempData["ErrorMessage"] = "Failed to update profile!";


            return View(model);
        }

    }
}
