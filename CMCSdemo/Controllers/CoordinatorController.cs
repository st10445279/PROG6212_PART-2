using Microsoft.AspNetCore.Mvc;
using CMCSdemo.Models;

namespace CMCSdemo.Controllers
{
    public class CoordinatorController : Controller
    {
        private string GetCurrentUserName() => "Programme Coordinator";

        public IActionResult Dashboard(ClaimSearchFilter? filter)
        {
            filter ??= new ClaimSearchFilter { StatusFilter = ClaimStatus.SubmittedToCoordinator };

            var result = MockDatabase.SearchClaims(filter);
            ViewBag.Filter = filter;

            return View(result);
        }

        public IActionResult VerifyClaims(ClaimSearchFilter? filter)
        {
            filter ??= new ClaimSearchFilter { StatusFilter = ClaimStatus.SubmittedToCoordinator };

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
                return RedirectToAction("VerifyClaims");
            }

            claim.Documents = MockDatabase.GetDocumentsByClaimId(id);
            ViewBag.Comments = MockDatabase.GetCommentsByClaimId(id);
            ViewBag.ActivityLogs = MockDatabase.GetActivityLogsByClaimId(id);

            return View(claim);
        }

        [HttpPost]
        public IActionResult VerifyClaim(int claimId, string action, string comments)
        {
            try
            {
                var claim = MockDatabase.GetClaimById(claimId);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction("VerifyClaims");
                }

                claim.CoordinatorComments = comments;
                claim.VerifiedDate = DateTime.Now;

                if (action == "approve")
                {
                    claim.Status = ClaimStatus.SubmittedToManager;
                    MockDatabase.LogActivity(claimId, GetCurrentUserName(), "Coordinator", "Verified Claim", comments);
                    TempData["Success"] = $"Claim #{claimId} verified and forwarded to Academic Manager.";
                }
                else if (action == "reject")
                {
                    claim.Status = ClaimStatus.RejectedByCoordinator;
                    MockDatabase.LogActivity(claimId, GetCurrentUserName(), "Coordinator", "Rejected Claim", comments);
                    TempData["Success"] = $"Claim #{claimId} rejected.";
                }

                MockDatabase.UpdateClaim(claim);
                return RedirectToAction("VerifyClaims");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("VerifyClaims");
            }
        }

        [HttpPost]
        public IActionResult BulkVerify(string claimIds, string action, string comments)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claimIds))
                {
                    TempData["Error"] = "No claims selected.";
                    return RedirectToAction("VerifyClaims");
                }

                var ids = claimIds.Split(',').Select(int.Parse).ToList();
                var newStatus = action == "approve" ? ClaimStatus.SubmittedToManager : ClaimStatus.RejectedByCoordinator;

                MockDatabase.BulkUpdateClaimStatus(ids, newStatus, GetCurrentUserName(), "Coordinator", comments);

                TempData["Success"] = $"Successfully {action}d {ids.Count} claim(s).";
                return RedirectToAction("VerifyClaims");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("VerifyClaims");
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
                UserRole = "Coordinator",
                CommentText = commentText
            };

            MockDatabase.AddComment(comment);
            TempData["Success"] = "Comment added successfully!";
            return RedirectToAction("ViewClaim", new { id = claimId });
        }
    }
}