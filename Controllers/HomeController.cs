using Emdad_Dashboard.Data;
using Emdad_Dashboard.Models;
using Emdad_Dashboard.VeiwModel.Dashboard;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Emdad_Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }
      
        public IActionResult Index()
        {
            List<Card> Cards = new List<Card>();
            // Create a Card object
            Cards.Add(new Card
            {
                Icon = "fa fa-cubes",
                Number = 123,
                CardName = "Total Sales"
            });

            Cards.Add(new Card
            {
                Icon = "fa fa-usd",
                Number = 44,
                CardName = "Clients"
            });

            Cards.Add(new Card
            {
                Icon = "fa fa-diamond",
                Number = 37,
                CardName = "Tasks"
            });
            Cards.Add(new Card
            {
                Icon = "fa fa-user",
                Number = 218,
                CardName = "Employees"
            });
            // Add the Card object to ViewBag
            ViewBag.LayoutCard = Cards;
            var latestDate = _context.attendances
                 .OrderByDescending(x => x.DateOnly)
                 .Select(x => x.DateOnly)
                 .FirstOrDefault();

            ViewBag.All = _context.attendances.Where(x => x.DateOnly == latestDate).ToList();

            ViewBag.EmployeeAbsent = _context.attendances.Where(x => x.DateOnly == latestDate && x.Status == AttendanceStatus.U).ToList();
            ViewBag.EmployeeAttend = _context.attendances.Where(x => x.DateOnly == latestDate && x.Status == AttendanceStatus.P).ToList();
            ViewBag.EmployeeOff = _context.attendances.Where(x => x.DateOnly == latestDate && x.Status == AttendanceStatus.O).ToList();
            ViewBag.EmployeeAbsentWithExcuse = _context.attendances.Where(x => x.DateOnly == latestDate && x.Status == AttendanceStatus.E).ToList();


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
