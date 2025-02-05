using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting; // Add this import
using System.IO;
using System.Threading.Tasks;

namespace DailyExpenseTracker.Controllers
{
    public class PdfController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Inject IWebHostEnvironment into the controller
        public PdfController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> GeneratePdf(IEnumerable<dynamic> reportData)
        {
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
                Data = new { ReportData = reportData } // Pass the dynamic report data to the template
            });

            // Return the PDF file as a download response
            return File(result.Content, "application/pdf", "GeneratedReport.pdf");
        }
    }
    }
