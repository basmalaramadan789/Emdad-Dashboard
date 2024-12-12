using Emdad_Dashboard.Data;
using Emdad_Dashboard.Helper;
using Emdad_Dashboard.Models;
using Emdad_Dashboard.VeiwModel.KPI;
using Emdad_Dashboard.VeiwModel.Vip;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace Emdad_Dashboard.Controllers;
public class VipController : Controller
{
    private readonly ApplicationContext _context;
    private readonly FileHelper _fileHelper;

    public VipController(ApplicationContext context, FileHelper fileHelper)
    {
        _context = context;
        _fileHelper = fileHelper;
    }


    //[HttpGet]
    //public IActionResult Index(DateTime? filterDate)
    //{
    //    // Fetch all VIP visits from the database and map to ViewModel
    //    IQueryable<Vip> vipsQuery = _context.Vips.AsQueryable();

    //    // Apply date filter if a date is provided
    //    if (filterDate.HasValue)
    //    {
    //        vipsQuery = vipsQuery.Where(v => v.VisitDate.HasValue && v.VisitDate.Value.Date == filterDate.Value.Date);
    //    }

    //    List<Vip> vips = vipsQuery.ToList();

    //    List<VipViewModel> viewModel = vips.Select(v => new VipViewModel
    //    {
    //        Id = v.Id,
    //        Name = v.Name,
    //        Visitors = v.Visitors,
    //        VisitDate = v.VisitDate,
    //        VisitPerDay = v.VisitPerDay,
    //        ArrivalTime = v.ArrivalTime,
    //        ContactDetails = v.ContactDetails,
    //        ReservationDetails = v.ReservationDetails,
    //        VisitType = v.VisitType,
    //        Gate = v.Gate,
    //        VisitDetails = v.VisitDetails,
    //        Feedback = v.Feedback,
    //        Remarks = v.Remarks
    //    }).ToList();

    //    return View(viewModel);
    //}



    [HttpGet]
    public IActionResult Index(DateTime? filterDate)
    {
        // Fetch all VIP visits from the database and map to ViewModel
        IQueryable<Vip> vipsQuery = _context.Vips.AsQueryable();

        // Apply date filter if a date is provided
        if (filterDate.HasValue)
        {
            vipsQuery = vipsQuery.Where(v => v.VisitDate.HasValue && v.VisitDate.Value.Date == filterDate.Value.Date);
        }

        List<Vip> vips = vipsQuery.ToList();

        List<VipViewModel> viewModel = vips.Select(v => new VipViewModel
        {
            Id = v.Id,
            Name = v.Name,
            Visitors = v.Visitors,
            VisitDate = v.VisitDate,
            VisitPerDay = v.VisitPerDay,
            ArrivalTime = v.ArrivalTime,
            ContactDetails = v.ContactDetails,
            ReservationDetails = v.ReservationDetails,
            VisitType = v.VisitType,
            Gate = v.Gate,
            VisitDetails = v.VisitDetails,
            Feedback = v.Feedback,
            Remarks = v.Remarks
        }).ToList();

        return View(viewModel);
    }



    [HttpPost]
    public async Task<IActionResult> Index(IFormFile UploadedFile)
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ViewBag.Message = "Please upload a valid Excel file.";
            return View(new List<VipViewModel>());
        }

        
        string filePath;
        try
        {
            filePath = await _fileHelper.UploadFileAsync(UploadedFile);
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"File upload failed: {ex.Message}";
            return View(new List<VipViewModel>());
        }

        
        List<Vip> vips;
        try
        {
            var nestedVips = _fileHelper.ReadExcelFileForVip(filePath);
            vips = nestedVips.SelectMany(x => x).ToList(); // Flatten the nested list

            if (vips.Count == 0)
            {
                ViewBag.Message = "No valid data found in the uploaded file.";
                return View(new List<VipViewModel>());
            }
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error reading file: {ex.Message}";
            return View(new List<VipViewModel>());
        }

       
        try
        {
            _context.Vips.AddRange(vips);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error saving data to database: {ex.Message}";
           
            var errorViewModel = vips.Select(v => new VipViewModel
            {
                Id = v.Id,
                Name = v.Name,
                Visitors = v.Visitors,
                VisitDate = v.VisitDate,
                VisitPerDay = v.VisitPerDay,
                ArrivalTime = v.ArrivalTime,
                ContactDetails = v.ContactDetails,
                ReservationDetails = v.ReservationDetails,
                VisitType = v.VisitType,
                Gate = v.Gate,
                VisitDetails = v.VisitDetails,
                Feedback = v.Feedback,
                Remarks = v.Remarks
            }).ToList();
            return View(errorViewModel);
        }

        
        var viewModel = vips.Select(v => new VipViewModel
        {
            Id = v.Id,
            Name = v.Name,
            Visitors = v.Visitors,
            VisitDate = v.VisitDate,
            VisitPerDay = v.VisitPerDay,
            ArrivalTime = v.ArrivalTime,
            ContactDetails = v.ContactDetails,
            ReservationDetails = v.ReservationDetails,
            VisitType = v.VisitType,
            Gate = v.Gate,
            VisitDetails = v.VisitDetails,
            Feedback = v.Feedback,
            Remarks = v.Remarks
        }).ToList();

        ViewBag.Message = "File uploaded and data saved successfully!";
        return View(viewModel);
    }







    


    [HttpGet]
    public IActionResult VipDashboard(DateTime? searchDate)
    {
        
        var vipsQuery = _context.Vips.AsQueryable();

        if (searchDate.HasValue)
        {
            vipsQuery = vipsQuery.Where(v => v.VisitDate.HasValue && v.VisitDate.Value.Date == searchDate.Value.Date);
        }

        var vips = vipsQuery
            .OrderBy(v => v.VisitDate)
            .ToList();

        
        if (!vips.Any())
        {
            ViewBag.TotalVips = 0;
            ViewBag.GroupedByVisitType = new List<object>();
            ViewBag.GroupedByGate = new List<object>();
            ViewBag.GroupedByDate = new List<object>();
            ViewBag.GroupedByTime = new List<object>();
            ViewBag.SearchDate = searchDate ?? DateTime.Today;
            return View();
        }

       
        var totalVips = vips.Sum(v => v.Visitors);


        var groupedByVisitType = vips
            .GroupBy(v => v.VisitType)
            .Select(g => new
            {
                VisitType = g.Key,
                TotalVisitors = g.Sum(v => v.Visitors) 
            }).ToList();

        
        var groupedByGate = vips
            .GroupBy(v => v.Gate)
            .Select(g => new
            {
                Gate = g.Key,
                TotalVisitors = g.Sum(v => v.Visitors) 
            }).ToList();

        // Group by Date
        var groupedByDate = vips
            .Where(v => v.VisitDate.HasValue)
            .GroupBy(v => v.VisitDate.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalVisitors = g.Sum(v => v.Visitors) 
            }).ToList();

        
        var groupedByTime = vips
            .GroupBy(v => v.ArrivalTime)
            .Select(g => new
            {
                Time = g.Key,
                TotalVisitors = g.Sum(v => v.Visitors) 
            }).ToList();

       
        ViewBag.TotalVips = totalVips;
        ViewBag.GroupedByVisitType = groupedByVisitType;
        ViewBag.GroupedByGate = groupedByGate;
        ViewBag.GroupedByDate = groupedByDate;
        ViewBag.GroupedByTime = groupedByTime;
        ViewBag.SearchDate = searchDate ?? DateTime.Today;

        
        ViewBag.VisitTypeGroups = JsonConvert.SerializeObject(groupedByVisitType);
        ViewBag.GateGroups = JsonConvert.SerializeObject(groupedByGate);
        ViewBag.VisitDateGroups = JsonConvert.SerializeObject(groupedByDate);

        return View();
    }
}




