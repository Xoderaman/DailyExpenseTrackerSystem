//using DailyExpenseTracker.Entity;
//using Microsoft.AspNetCore.Mvc;

//public class TransactionController : Controller
//{
 

//    // Action to display the transaction history (TransactionHistory page)
//    public IActionResult TransactionHistory()
//    {
//        // Fetch all transactions from the database
//        var transactions = _context.Transactions.ToList();

//        // Return the TransactionHistory view from the Home folder
//        return View("~/Views/Home/TransactionHistory.cshtml", transactions);
//    }

//    // Add a new transaction (POST method)
//    [HttpPost]
//    public IActionResult AddTransaction(TransactionModel transaction)
//    {
//        if (ModelState.IsValid)
//        {
//            // Add the new transaction to the database
//            _context.Transactions.Add(transaction);
//            _context.SaveChanges();

//            // Redirect to the TransactionHistory action to refresh the transaction list
//            return RedirectToAction("TransactionHistory");
//        }

//        // If ModelState is not valid, return to the TransactionHistory view with the current transactions
//        return View("~/Views/Home/TransactionHistory.cshtml", _context.Transactions.ToList());
//    }
//    [HttpGet]
//    public IActionResult RemoveTransaction(int id)
//    {
//        var transaction = _context.Transactions.Find(id);
//        if (transaction != null)
//        {
//            _context.Transactions.Remove(transaction);
//            _context.SaveChanges();
//        }

//        return RedirectToAction("TransactionHistory");
//    }

//}
