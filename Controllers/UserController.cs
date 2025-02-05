using DailyExpenseTracker.Repository;
using Microsoft.AspNetCore.Mvc;

namespace DailyExpenseTracker.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> GetUser(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);  // Return the user details to the view
        }
    }

}
