//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;

//[Route("api/[controller]")]
//[ApiController]
//public class BudgetController : ControllerBase
//{
//    private readonly IBudgetService _budgetService;

//    public BudgetController(IBudgetService budgetService)
//    {
//        _budgetService = budgetService;
//    }
   
//    [HttpGet("GetBudgetNews")]
//    public async Task<IActionResult> GetBudgetNews()
//    {
//        try
//        {
//            var newsResponse = await _budgetService.GetBudgetNewsAsync();
//            return Ok(newsResponse); // Return JSON data
//        }
//        catch (Exception ex)
//        {
//            return BadRequest(new { message = ex.Message });
//        }
      
//    }
//}
