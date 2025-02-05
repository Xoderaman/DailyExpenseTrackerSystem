using DailyExpenseTracker.Entity.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DailyExpenseTracker.Repository;  // Corrected namespace

namespace DailyExpenseTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        // Inject the UserRepository into the constructor
        public AccountController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: SignUp  
        public IActionResult SignUp()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> SignUp(UserModel model)
        {


            if (ModelState.IsValid)
            {

                int userId = await _userRepository.AddUserAsync(model);


                return RedirectToAction("Login");

            }
            return View(model);
        }



        // POST: SignUp
        //[HttpPost]
        //public async Task<IActionResult> SignUp(UserModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Check if user with the same username or email already exists
        //        var existingUser = await _userRepository.GetUserByEmailAsync(model.Email);

        //        if (existingUser != null)
        //        {
        //            // Username or email already exists, display an error
        //            ModelState.AddModelError("", "Username or Email already exists");
        //            return View(model);
        //        }

        //        // Add the new user to the database
        //        var result = await _userRepository.AddUserAsync(model);

        //        if (result > 0)
        //        {
        //            // Redirect to Login after successful signup
        //            return RedirectToAction("Login", "Account");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Failed to register user.");
        //        }
        //    }

        //    // If model validation fails, return the same view with error messages
        //    return View(model);
        //}

       
        public IActionResult Login()
        {
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user != null && user.Password == password)
            {
                
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetInt32("UserId", user.UserId);

                return RedirectToAction("Index", "Home");
            }

           
            ModelState.AddModelError("", "Invalid email or password");

            
            return View();
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
                        return RedirectToAction("UpdateProfile");
                    }
                }

                TempData["ErrorMessage"] = "Failed to update profile!";
            

            return View(model);
        }

        // Logout action
        public async Task<IActionResult> Logout()
        {
            // Sign out from the authentication scheme
            await HttpContext.SignOutAsync();

            // Redirect to Login page after logout
            return RedirectToAction("Login", "Account");
        }
    }
}
