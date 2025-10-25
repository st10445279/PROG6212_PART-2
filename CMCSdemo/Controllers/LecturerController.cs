using Microsoft.AspNetCore.Mvc;
using CMCSdemo.Models;

namespace CMCSdemo.Controllers
{
    public class LecturerController : Controller
    {
        private string GetCurrentLecturerEmail() => "john.smith@university.edu";
        private string GetCurrentLecturerName() => "Dr. John Smith";

        public IActionResult Dashboard(ClaimSearchFilter? filter)
        {
            filter ??= new ClaimSearchFilter();

            var lecturerEmail = GetCurrentLecturerEmail();
            var allClaims = MockDatabase.GetClaimsByLecturer(lecturerEmail);

            // Apply filters
            var query = allClaims.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(c => c.AdditionalNotes.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.StatusFilter.HasValue)
            {
                query = query.Where(c => c.Status == filter.StatusFilter.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(c => c.SubmittedDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(c => c.SubmittedDate <= filter.EndDate.Value);
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "claimid" => filter.SortOrder == "asc" ? query.OrderBy(c => c.ClaimId) : query.OrderByDescending(c => c.ClaimId),
                "totalamount" => filter.SortOrder == "asc" ? query.OrderBy(c => c.TotalAmount) : query.OrderByDescending(c => c.TotalAmount),
                "status" => filter.SortOrder == "asc" ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
                _ => filter.SortOrder == "asc" ? query.OrderBy(c => c.SubmittedDate) : query.OrderByDescending(c => c.SubmittedDate)
            };

            var totalCount = query.Count();
            var claims = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var result = new PaginatedResult<Claim>
            {
                Items = claims,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            ViewBag.Filter = filter;
            return View(result);
        }

        public IActionResult SubmitClaim()
        {
            var profile = MockDatabase.GetLecturerProfile(GetCurrentLecturerEmail());
            if (profile != null)
            {
                ViewBag.DefaultRate = profile.DefaultHourlyRate;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, List<IFormFile> documents)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    claim.LecturerName = GetCurrentLecturerName();
                    claim.LecturerEmail = GetCurrentLecturerEmail();
                    claim.Status = ClaimStatus.SubmittedToCoordinator;

                    MockDatabase.AddClaim(claim);

                    if (documents != null && documents.Count > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        Directory.CreateDirectory(uploadsFolder);

                        foreach (var file in documents)
                        {
                            if (file.Length > 0)
                            {
                                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };
                                var extension = Path.GetExtension(file.FileName).ToLower();

                                if (!allowedExtensions.Contains(extension))
                                {
                                    TempData["Error"] = $"File type {extension} is not allowed.";
                                    continue;
                                }

                                if (file.Length > 5 * 1024 * 1024)
                                {
                                    TempData["Error"] = "File size must be less than 5MB.";
                                    continue;
                                }

                                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var document = new ClaimDocument
                                {
                                    ClaimId = claim.ClaimId,
                                    FileName = file.FileName,
                                    FilePath = $"/uploads/{uniqueFileName}",
                                    FileSize = file.Length
                                };

                                MockDatabase.AddDocument(document);
                            }
                        }
                    }

                    TempData["Success"] = "Claim submitted successfully!";
                    return RedirectToAction("Dashboard");
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(claim);
            }
        }

        public IActionResult EditClaim(int id)
        {
            var claim = MockDatabase.GetClaimById(id);
            if (claim == null || claim.LecturerEmail != GetCurrentLecturerEmail())
            {
                TempData["Error"] = "Claim not found or access denied.";
                return RedirectToAction("Dashboard");
            }

            if (claim.Status != ClaimStatus.Pending && claim.Status != ClaimStatus.RejectedByCoordinator && claim.Status != ClaimStatus.RejectedByManager)
            {
                TempData["Error"] = "Cannot edit claim that is already being processed or approved.";
                return RedirectToAction("Dashboard");
            }

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditClaim(Claim claim)
        {
            try
            {
                var existingClaim = MockDatabase.GetClaimById(claim.ClaimId);
                if (existingClaim == null || existingClaim.LecturerEmail != GetCurrentLecturerEmail())
                {
                    TempData["Error"] = "Claim not found or access denied.";
                    return RedirectToAction("Dashboard");
                }

                existingClaim.HoursWorked = claim.HoursWorked;
                existingClaim.HourlyRate = claim.HourlyRate;
                existingClaim.AdditionalNotes = claim.AdditionalNotes;
                existingClaim.Status = ClaimStatus.SubmittedToCoordinator;

                MockDatabase.UpdateClaim(existingClaim);
                MockDatabase.LogActivity(claim.ClaimId, GetCurrentLecturerName(), "Lecturer", "Updated Claim", "Claim details updated and resubmitted");

                TempData["Success"] = "Claim updated successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(claim);
            }
        }

        public IActionResult ViewStatus(int id)
        {
            var claim = MockDatabase.GetClaimById(id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Dashboard");
            }

            claim.Documents = MockDatabase.GetDocumentsByClaimId(id);
            ViewBag.Comments = MockDatabase.GetCommentsByClaimId(id);
            ViewBag.ActivityLogs = MockDatabase.GetActivityLogsByClaimId(id);

            return View(claim);
        }

        [HttpPost]
        public IActionResult AddComment(int claimId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("ViewStatus", new { id = claimId });
            }

            var comment = new ClaimComment
            {
                ClaimId = claimId,
                UserName = GetCurrentLecturerName(),
                UserRole = "Lecturer",
                CommentText = commentText
            };

            MockDatabase.AddComment(comment);
            TempData["Success"] = "Comment added successfully!";
            return RedirectToAction("ViewStatus", new { id = claimId });
        }

        [HttpPost]
        public IActionResult CancelClaim(int id)
        {
            var claim = MockDatabase.GetClaimById(id);
            if (claim == null || claim.LecturerEmail != GetCurrentLecturerEmail())
            {
                TempData["Error"] = "Claim not found or access denied.";
                return RedirectToAction("Dashboard");
            }

            if (claim.Status == ClaimStatus.ApprovedByManager)
            {
                TempData["Error"] = "Cannot cancel an approved claim.";
                return RedirectToAction("Dashboard");
            }

            MockDatabase.DeleteClaim(id);
            TempData["Success"] = "Claim cancelled successfully!";
            return RedirectToAction("Dashboard");
        }

        public IActionResult Profile()
        {
            var profile = MockDatabase.GetLecturerProfile(GetCurrentLecturerEmail());
            if (profile == null)
            {
                profile = new LecturerProfile
                {
                    Email = GetCurrentLecturerEmail(),
                    FullName = GetCurrentLecturerName()
                };
            }
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(LecturerProfile profile)
        {
            try
            {
                profile.Email = GetCurrentLecturerEmail();
                MockDatabase.AddOrUpdateLecturerProfile(profile);
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(profile);
            }
        }

        public IActionResult PrintClaim(int id)
        {
            var claim = MockDatabase.GetClaimById(id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Dashboard");
            }

            claim.Documents = MockDatabase.GetDocumentsByClaimId(id);
            return View(claim);
        }
    }
}