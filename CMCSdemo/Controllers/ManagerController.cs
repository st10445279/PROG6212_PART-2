using Microsoft.AspNetCore.Mvc;
using CMCSdemo.Models;

namespace CMCSdemo.Controllers
{
    public class ManagerController : Controller
    {
        private string GetCurrentUserName() => "Academic Manager";

        public IActionResult Dashboard(ClaimSearchFilter? filter)
        {
            filter ??= new ClaimSearchFilter { StatusFilter = ClaimStatus.SubmittedToManager };

            var result = MockDatabase.SearchClaims(filter);
            ViewBag.Filter = filter;

            return View(result);
        }

        public IActionResult ApproveClaims(ClaimSearchFilter? filter)
        {
            filter ??= new ClaimSearchFilter { StatusFilter = ClaimStatus.SubmittedToManager };

            var result = MockDatabase.SearchClaims(filter);
            ViewBag.Filter = filter;

            return View(result);
        }

        public IActionResult ViewClaim(int id)
        {
            var claim = MockDatabase.GetClaimById(id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("ApproveClaims");
            }

            claim.Documents = MockDatabase.GetDocumentsByClaimId(id);
            ViewBag.Comments = MockDatabase.GetCommentsByClaimId(id);
            ViewBag.ActivityLogs = MockDatabase.GetActivityLogsByClaimId(id);

            return View(claim);
        }

        [HttpPost]
        public IActionResult ApproveClaim(int claimId, string action, string comments)
        {
            try
            {
                var claim = MockDatabase.GetClaimById(claimId);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction("ApproveClaims");
                }

                claim.ManagerComments = comments;
                claim.ApprovedDate = DateTime.Now;

                if (action == "approve")
                {
                    claim.Status = ClaimStatus.ApprovedByManager;
                    MockDatabase.LogActivity(claimId, GetCurrentUserName(), "Manager", "Approved Claim", comments);
                    TempData["Success"] = $"Claim #{claimId} approved for payment.";
                }
                else if (action == "reject")
                {
                    claim.Status = ClaimStatus.RejectedByManager;
                    MockDatabase.LogActivity(claimId, GetCurrentUserName(), "Manager", "Rejected Claim", comments);
                    TempData["Success"] = $"Claim #{claimId} rejected.";
                }

                MockDatabase.UpdateClaim(claim);
                return RedirectToAction("ApproveClaims");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("ApproveClaims");
            }
        }

        [HttpPost]
        public IActionResult BulkApprove(string claimIds, string action, string comments)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claimIds))
                {
                    TempData["Error"] = "No claims selected.";
                    return RedirectToAction("ApproveClaims");
                }

                var ids = claimIds.Split(',').Select(int.Parse).ToList();
                var newStatus = action == "approve" ? ClaimStatus.ApprovedByManager : ClaimStatus.RejectedByManager;

                MockDatabase.BulkUpdateClaimStatus(ids, newStatus, GetCurrentUserName(), "Manager", comments);

                TempData["Success"] = $"Successfully {action}d {ids.Count} claim(s).";
                return RedirectToAction("ApproveClaims");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("ApproveClaims");
            }
        }

        [HttpPost]
        public IActionResult AddComment(int claimId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("ViewClaim", new { id = claimId });
            }

            var comment = new ClaimComment
            {
                ClaimId = claimId,
                UserName = GetCurrentUserName(),
                UserRole = "Manager",
                CommentText = commentText
            };

            MockDatabase.AddComment(comment);
            TempData["Success"] = "Comment added successfully!";
            return RedirectToAction("ViewClaim", new { id = claimId });
        }
    }
}