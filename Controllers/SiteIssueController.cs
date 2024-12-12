using Emdad_Dashboard.Data;
using Emdad_Dashboard.Helper;
using Emdad_Dashboard.Models;
using Emdad_Dashboard.VeiwModel.SiteIssues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;

namespace Emdad_Dashboard.Controllers;
public class SiteIssueController : Controller
{
    private readonly ApplicationContext _context;
    private readonly FileHelper _fileHelper;

    public SiteIssueController(ApplicationContext context, FileHelper fileHelper)
    {
        _context = context;
        _fileHelper = fileHelper;
    }
         

    [HttpGet]
    public IActionResult Index(DateOnly? filterDate, string searchTerm, string searchType)
    {
        
        IQueryable<SiteIssue> siteIssuesQuery = _context.siteIssues.AsQueryable();

       
        if (filterDate.HasValue)
        {
            siteIssuesQuery = siteIssuesQuery.Where(s => s.Date == filterDate.Value);
        }

        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (searchType == "Owner")
            {
                siteIssuesQuery = siteIssuesQuery.Where(s => s.Owner.Contains(searchTerm));
            }
            else if (searchType == "Type")
            {
                siteIssuesQuery = siteIssuesQuery.Where(s => s.Type.Contains(searchTerm));
            }
            else if (searchType == "Both")
            {
                siteIssuesQuery = siteIssuesQuery.Where(s => s.Owner.Contains(searchTerm) || s.Type.Contains(searchTerm));
            }
        }

        List<SiteIssue> siteIssues = siteIssuesQuery.ToList();

        List<SiteIssueViewModel> viewModel = siteIssues.Select(s => new SiteIssueViewModel
        {
            Id = s.Id,
            Type = s.Type,
            Time = s.Time,
            Date = s.Date,
            Owner = s.Owner,
            Description = s.Description,
            Resolution = s.Resolution,
            Status = s.Status,
            EmployeesName = s.EmployeesName,
        }).ToList();

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(IFormFile UploadedFile)
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ViewBag.Message = "Please upload a valid Excel file.";
            return View(new List<SiteIssueViewModel>());
        }

      
        string filePath;
        try
        {
            filePath = await _fileHelper.UploadFileAsync(UploadedFile);
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"File upload failed: {ex.Message}";
            return View(new List<SiteIssueViewModel>());
        }

      
        List<SiteIssue> siteIssues;
        try
        {
            var nestedSiteIssues = _fileHelper.ReadExcelFileForSiteIssue(filePath);
            siteIssues = nestedSiteIssues.SelectMany(x => x).ToList(); // Flatten the nested list

            if (siteIssues.Count == 0)
            {
                ViewBag.Message = "No valid data found in the uploaded file.";
                return View(new List<SiteIssueViewModel>());
            }
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error reading file: {ex.Message}";
            return View(new List<SiteIssueViewModel>());
        }

        
        try
        {
            _context.siteIssues.AddRange(siteIssues);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error saving data to database: {ex.Message}";
            
            var errorViewModel = siteIssues.Select(s => new SiteIssueViewModel
            {
                Id = s.Id,
                Type = s.Type,
                Time = s.Time,
                Date = s.Date,
                Owner = s.Owner,
                Description = s.Description
            }).ToList();
            return View(errorViewModel);
        }

        
        var viewModel = siteIssues.Select(s => new SiteIssueViewModel
        {
            Id = s.Id,
            Type = s.Type,
            Time = s.Time,
            Date = s.Date,
            Owner = s.Owner,
            Description = s.Description,
            Resolution = s.Resolution,
            Status = s.Status,
            EmployeesName = s.EmployeesName

        }).ToList();

        ViewBag.Message = "File uploaded and data saved successfully!";
        return View(viewModel);
    }


    




    [HttpGet]
    public IActionResult Dashboard(DateTime? searchDate)
    {
        // Fetch Site Issues from the database, filtered by the optional searchDate.
        var siteIssuesQuery = _context.siteIssues.AsQueryable();

        if (searchDate.HasValue)
        {
            siteIssuesQuery = siteIssuesQuery.Where(s => s.Date == DateOnly.FromDateTime(searchDate.Value));
        }

        var siteIssues = siteIssuesQuery
            .OrderBy(s => s.Date)
            .ToList();

        // Check if Site Issues are empty and handle gracefully
        if (!siteIssues.Any())
        {
            ViewBag.TotalIssues = 0;
            ViewBag.GroupedByType = new List<object>();
            ViewBag.GroupedByStatus = new List<object>();
            ViewBag.GroupedByDate = new List<object>();
            ViewBag.GroupedByOwner = new List<object>();
            ViewBag.SearchDate = searchDate ?? DateTime.Today;
            return View();
        }

        // Total Site Issues count
        var totalIssues = siteIssues.Count;

        // Group by Type
        var groupedByType = siteIssues
            .GroupBy(s => s.Type)
            .Select(g => new
            {
                Type = g.Key,
                Count = g.Count()
            }).ToList();

        // Group by Status
        var groupedByStatus = siteIssues
            .GroupBy(s => s.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            }).ToList();

        // Group by Date
        var groupedByDate = siteIssues
            .GroupBy(s => s.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            }).ToList();

        // Group by Owner
        var groupedByOwner = siteIssues
            .GroupBy(s => s.Owner)
            .Select(g => new
            {
                Owner = g.Key,
                Count = g.Count()
            }).ToList();

        // Assign grouped data to ViewBag
        ViewBag.TotalIssues = totalIssues;
        ViewBag.GroupedByType = groupedByType;
        ViewBag.GroupedByStatus = groupedByStatus;
        ViewBag.GroupedByDate = groupedByDate;
        ViewBag.GroupedByOwner = groupedByOwner;
        ViewBag.SearchDate = searchDate ?? DateTime.Today;

        // Pass data for charts
        ViewBag.StatusGroups = JsonConvert.SerializeObject(groupedByStatus);
        ViewBag.TypeGroups = JsonConvert.SerializeObject(groupedByType);

        return View();
    }
}