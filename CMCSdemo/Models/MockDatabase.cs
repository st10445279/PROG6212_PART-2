namespace CMCSdemo.Models
{
    public static class MockDatabase
    {
        private static List<Claim> _claims = new List<Claim>();
        private static List<ClaimDocument> _documents = new List<ClaimDocument>();
        private static List<ClaimComment> _comments = new List<ClaimComment>();
        private static List<ActivityLog> _activityLogs = new List<ActivityLog>();
        private static List<LecturerProfile> _lecturerProfiles = new List<LecturerProfile>();

        private static int _claimIdCounter = 1;
        private static int _documentIdCounter = 1;
        private static int _commentIdCounter = 1;
        private static int _logIdCounter = 1;
        private static int _profileIdCounter = 1;

        static MockDatabase()
        {
            SeedData();
        }

        private static void SeedData()
        {
            // Seed lecturer profiles
            _lecturerProfiles.Add(new LecturerProfile
            {
                ProfileId = _profileIdCounter++,
                Email = "john.smith@university.edu",
                FullName = "Dr. John Smith",
                PhoneNumber = "+27 11 123 4567",
                Department = "Computer Science",
                Specialization = "Software Engineering",
                DefaultHourlyRate = 150,
                BankAccountNumber = "1234567890",
                BankName = "Standard Bank",
                JoinedDate = DateTime.Now.AddYears(-2)
            });

            _lecturerProfiles.Add(new LecturerProfile
            {
                ProfileId = _profileIdCounter++,
                Email = "sarah.johnson@university.edu",
                FullName = "Prof. Sarah Johnson",
                PhoneNumber = "+27 11 234 5678",
                Department = "Information Technology",
                Specialization = "Database Management",
                DefaultHourlyRate = 180,
                BankAccountNumber = "0987654321",
                BankName = "FNB",
                JoinedDate = DateTime.Now.AddYears(-3)
            });

            // Seed claims with various statuses and dates
            for (int i = 0; i < 25; i++)
            {
                var claim = new Claim
                {
                    ClaimId = _claimIdCounter++,
                    LecturerName = i % 2 == 0 ? "Dr. John Smith" : "Prof. Sarah Johnson",
                    LecturerEmail = i % 2 == 0 ? "john.smith@university.edu" : "sarah.johnson@university.edu",
                    HoursWorked = 20 + (i * 5),
                    HourlyRate = i % 2 == 0 ? 150 : 180,
                    AdditionalNotes = $"Lectures for module {i + 1}",
                    Status = (ClaimStatus)(i % 8),
                    SubmittedDate = DateTime.Now.AddDays(-30 + i)
                };

                if (claim.Status >= ClaimStatus.VerifiedByCoordinator)
                {
                    claim.VerifiedDate = claim.SubmittedDate.AddDays(2);
                    claim.CoordinatorComments = "Verified and approved for processing";
                }

                if (claim.Status >= ClaimStatus.ApprovedByManager)
                {
                    claim.ApprovedDate = claim.VerifiedDate?.AddDays(1);
                    claim.ManagerComments = "Approved for payment";
                }

                _claims.Add(claim);

                // Add activity logs
                LogActivity(claim.ClaimId, claim.LecturerName, "Lecturer", "Submitted Claim",
                    $"Submitted claim for {claim.HoursWorked} hours at R{claim.HourlyRate}/hour");
            }
        }

        // Claims CRUD operations
        public static List<Claim> GetAllClaims() => _claims;

        public static Claim? GetClaimById(int id) => _claims.FirstOrDefault(c => c.ClaimId == id);

        public static List<Claim> GetClaimsByStatus(ClaimStatus status) =>
            _claims.Where(c => c.Status == status).ToList();

        public static List<Claim> GetClaimsByLecturer(string email) =>
            _claims.Where(c => c.LecturerEmail == email).ToList();

        public static PaginatedResult<Claim> SearchClaims(ClaimSearchFilter filter)
        {
            var query = _claims.AsQueryable();

            // Apply search term
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(c =>
                    c.LecturerName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.LecturerEmail.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.AdditionalNotes.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply status filter
            if (filter.StatusFilter.HasValue)
            {
                query = query.Where(c => c.Status == filter.StatusFilter.Value);
            }

            // Apply date range filter
            if (filter.StartDate.HasValue)
            {
                query = query.Where(c => c.SubmittedDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(c => c.SubmittedDate <= filter.EndDate.Value);
            }

            // Apply amount range filter
            if (filter.MinAmount.HasValue)
            {
                query = query.Where(c => c.TotalAmount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(c => c.TotalAmount <= filter.MaxAmount.Value);
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "claimid" => filter.SortOrder == "asc" ? query.OrderBy(c => c.ClaimId) : query.OrderByDescending(c => c.ClaimId),
                "lecturername" => filter.SortOrder == "asc" ? query.OrderBy(c => c.LecturerName) : query.OrderByDescending(c => c.LecturerName),
                "hoursworked" => filter.SortOrder == "asc" ? query.OrderBy(c => c.HoursWorked) : query.OrderByDescending(c => c.HoursWorked),
                "totalamount" => filter.SortOrder == "asc" ? query.OrderBy(c => c.TotalAmount) : query.OrderByDescending(c => c.TotalAmount),
                "status" => filter.SortOrder == "asc" ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
                _ => filter.SortOrder == "asc" ? query.OrderBy(c => c.SubmittedDate) : query.OrderByDescending(c => c.SubmittedDate)
            };

            var totalCount = query.Count();
            var items = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new PaginatedResult<Claim>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public static void AddClaim(Claim claim)
        {
            claim.ClaimId = _claimIdCounter++;
            claim.SubmittedDate = DateTime.Now;
            _claims.Add(claim);
            LogActivity(claim.ClaimId, claim.LecturerName, "Lecturer", "Submitted Claim",
                $"Submitted claim for {claim.HoursWorked} hours at R{claim.HourlyRate}/hour");
        }

        public static void UpdateClaim(Claim claim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.ClaimId == claim.ClaimId);
            if (existingClaim != null)
            {
                var index = _claims.IndexOf(existingClaim);
                _claims[index] = claim;
            }
        }

        public static void DeleteClaim(int claimId)
        {
            var claim = GetClaimById(claimId);
            if (claim != null)
            {
                _claims.Remove(claim);
                LogActivity(claimId, "System", "System", "Deleted Claim", "Claim was deleted");
            }
        }

        public static void BulkUpdateClaimStatus(List<int> claimIds, ClaimStatus newStatus, string userName, string userRole, string comments)
        {
            foreach (var claimId in claimIds)
            {
                var claim = GetClaimById(claimId);
                if (claim != null)
                {
                    claim.Status = newStatus;

                    if (userRole == "Coordinator")
                    {
                        claim.CoordinatorComments = comments;
                        claim.VerifiedDate = DateTime.Now;
                    }
                    else if (userRole == "Manager")
                    {
                        claim.ManagerComments = comments;
                        claim.ApprovedDate = DateTime.Now;
                    }

                    UpdateClaim(claim);
                    LogActivity(claimId, userName, userRole, $"Bulk {newStatus}", comments);
                }
            }
        }

        // Document operations
        public static void AddDocument(ClaimDocument document)
        {
            document.DocumentId = _documentIdCounter++;
            document.UploadedDate = DateTime.Now;
            _documents.Add(document);

            var claim = GetClaimById(document.ClaimId);
            if (claim != null)
            {
                claim.Documents.Add(document);
                LogActivity(document.ClaimId, "System", "System", "Document Uploaded", $"Uploaded {document.FileName}");
            }
        }

        public static List<ClaimDocument> GetDocumentsByClaimId(int claimId) =>
            _documents.Where(d => d.ClaimId == claimId).ToList();

        // Comment operations
        public static void AddComment(ClaimComment comment)
        {
            comment.CommentId = _commentIdCounter++;
            comment.CreatedDate = DateTime.Now;
            _comments.Add(comment);
            LogActivity(comment.ClaimId, comment.UserName, comment.UserRole, "Added Comment", comment.CommentText);
        }

        public static List<ClaimComment> GetCommentsByClaimId(int claimId) =>
            _comments.Where(c => c.ClaimId == claimId).OrderBy(c => c.CreatedDate).ToList();

        // Activity log operations
        public static void LogActivity(int claimId, string userName, string userRole, string action, string details)
        {
            _activityLogs.Add(new ActivityLog
            {
                LogId = _logIdCounter++,
                ClaimId = claimId,
                UserName = userName,
                UserRole = userRole,
                Action = action,
                Details = details,
                Timestamp = DateTime.Now
            });
        }

        public static List<ActivityLog> GetActivityLogsByClaimId(int claimId) =>
            _activityLogs.Where(a => a.ClaimId == claimId).OrderByDescending(a => a.Timestamp).ToList();

        public static List<ActivityLog> GetAllActivityLogs() =>
            _activityLogs.OrderByDescending(a => a.Timestamp).ToList();

        // Lecturer profile operations
        public static LecturerProfile? GetLecturerProfile(string email) =>
            _lecturerProfiles.FirstOrDefault(p => p.Email == email);

        public static List<LecturerProfile> GetAllLecturerProfiles() => _lecturerProfiles;

        public static void AddOrUpdateLecturerProfile(LecturerProfile profile)
        {
            var existing = GetLecturerProfile(profile.Email);
            if (existing != null)
            {
                var index = _lecturerProfiles.IndexOf(existing);
                _lecturerProfiles[index] = profile;
            }
            else
            {
                profile.ProfileId = _profileIdCounter++;
                _lecturerProfiles.Add(profile);
            }
        }

        // Statistics operations
        public static ClaimStatistics GetStatistics()
        {
            var stats = new ClaimStatistics
            {
                TotalClaims = _claims.Count,
                PendingClaims = _claims.Count(c => c.Status == ClaimStatus.Pending ||
                                                   c.Status == ClaimStatus.SubmittedToCoordinator ||
                                                   c.Status == ClaimStatus.SubmittedToManager),
                ApprovedClaims = _claims.Count(c => c.Status == ClaimStatus.ApprovedByManager),
                RejectedClaims = _claims.Count(c => c.Status == ClaimStatus.RejectedByCoordinator ||
                                                    c.Status == ClaimStatus.RejectedByManager),
                TotalAmountClaimed = _claims.Sum(c => c.TotalAmount),
                TotalAmountApproved = _claims.Where(c => c.Status == ClaimStatus.ApprovedByManager).Sum(c => c.TotalAmount),
                TotalAmountRejected = _claims.Where(c => c.Status == ClaimStatus.RejectedByCoordinator ||
                                                         c.Status == ClaimStatus.RejectedByManager).Sum(c => c.TotalAmount),
                AverageClaimAmount = _claims.Any() ? _claims.Average(c => c.TotalAmount) : 0
            };

            // Claims by month
            var claimsByMonth = _claims
                .GroupBy(c => c.SubmittedDate.ToString("MMM yyyy"))
                .ToDictionary(g => g.Key, g => g.Count());
            stats.ClaimsByMonth = claimsByMonth;

            // Amount by month
            var amountByMonth = _claims
                .GroupBy(c => c.SubmittedDate.ToString("MMM yyyy"))
                .ToDictionary(g => g.Key, g => g.Sum(c => c.TotalAmount));
            stats.AmountByMonth = amountByMonth;

            // Top lecturers
            var topLecturers = _claims
                .GroupBy(c => c.LecturerName)
                .Select(g => new LecturerPerformance
                {
                    LecturerName = g.Key,
                    TotalClaims = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    ApprovedClaims = g.Count(c => c.Status == ClaimStatus.ApprovedByManager),
                    ApprovalRate = g.Any() ? (decimal)g.Count(c => c.Status == ClaimStatus.ApprovedByManager) / g.Count() * 100 : 0
                })
                .OrderByDescending(l => l.TotalAmount)
                .Take(10)
                .ToList();
            stats.TopLecturers = topLecturers;

            return stats;
        }
    }
}