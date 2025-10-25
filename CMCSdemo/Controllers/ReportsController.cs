using Microsoft.AspNetCore.Mvc;
using CMCSdemo.Models;
using System.Text;

namespace CMCSdemo.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            var stats = MockDatabase.GetStatistics();
            return View(stats);
        }

        public IActionResult MonthlyReport(int? month, int? year)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            var claims = MockDatabase.GetAllClaims()
                .Where(c => c.SubmittedDate.Month == selectedMonth && c.SubmittedDate.Year == selectedYear)
                .ToList();

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.MonthName = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM yyyy");

            return View(claims);
        }

        public IActionResult LecturerPerformance()
        {
            var stats = MockDatabase.GetStatistics();
            return View(stats.TopLecturers);
        }

        public IActionResult ExportToCSV()
        {
            var claims = MockDatabase.GetAllClaims();
            var csv = new StringBuilder();

            csv.AppendLine("Claim ID,Lecturer Name,Email,Hours Worked,Hourly Rate,Total Amount,Status,Submitted Date,Verified Date,Approved Date");

            foreach (var claim in claims)
            {
                csv.AppendLine($"{claim.ClaimId},{claim.LecturerName},{claim.LecturerEmail},{claim.HoursWorked},{claim.HourlyRate},{claim.TotalAmount},{claim.Status},{claim.SubmittedDate:yyyy-MM-dd},{claim.VerifiedDate:yyyy-MM-dd},{claim.ApprovedDate:yyyy-MM-dd}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Claims_Export_{DateTime.Now:yyyyMMdd}.csv");
        }

        public IActionResult ActivityLog()
        {
            var logs = MockDatabase.GetAllActivityLogs();
            return View(logs);
        }
    }
}