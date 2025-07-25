using AutoMapper;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Infrastructure.Data.Repositories;
using System.Text.Json;

namespace xyz_university_payment_api.Core.Application.Services
{
    public class FeeManagementService : IFeeManagementService
    {
        private readonly IFeeManagementRepository _repository;
        private readonly IMapper _mapper;

        public FeeManagementService(IFeeManagementRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // Fee Category Operations
        public async Task<List<FeeCategoryDto>> GetAllFeeCategoriesAsync()
        {
            var categories = await _repository.GetAllFeeCategoriesAsync();
            return _mapper.Map<List<FeeCategoryDto>>(categories);
        }

        public async Task<FeeCategoryDto?> GetFeeCategoryByIdAsync(int id)
        {
            var category = await _repository.GetFeeCategoryByIdAsync(id);
            return _mapper.Map<FeeCategoryDto>(category);
        }

        public async Task<FeeCategoryDto> CreateFeeCategoryAsync(CreateFeeCategoryDto dto)
        {
            var category = _mapper.Map<FeeCategory>(dto);
            var createdCategory = await _repository.CreateFeeCategoryAsync(category);
            return _mapper.Map<FeeCategoryDto>(createdCategory);
        }

        public async Task<FeeCategoryDto> UpdateFeeCategoryAsync(int id, UpdateFeeCategoryDto dto)
        {
            var existingCategory = await _repository.GetFeeCategoryByIdAsync(id);
            if (existingCategory == null)
                throw new InvalidOperationException($"Fee category with ID {id} not found.");

            _mapper.Map(dto, existingCategory);
            var updatedCategory = await _repository.UpdateFeeCategoryAsync(existingCategory);
            return _mapper.Map<FeeCategoryDto>(updatedCategory);
        }

        public async Task<bool> DeleteFeeCategoryAsync(int id)
        {
            return await _repository.DeleteFeeCategoryAsync(id);
        }

        // Fee Structure Operations
        public async Task<List<FeeStructureDto>> GetAllFeeStructuresAsync()
        {
            var structures = await _repository.GetAllFeeStructuresAsync();
            return _mapper.Map<List<FeeStructureDto>>(structures);
        }

        public async Task<List<FeeStructureDto>> GetAllFeeStructuresIncludingInactiveAsync()
        {
            var structures = await _repository.GetAllFeeStructuresIncludingInactiveAsync();
            return _mapper.Map<List<FeeStructureDto>>(structures);
        }

        public async Task<FeeStructureDto?> GetFeeStructureByIdAsync(int id)
        {
            var structure = await _repository.GetFeeStructureByIdAsync(id);
            return _mapper.Map<FeeStructureDto>(structure);
        }

        public async Task<List<FeeStructureDto>> GetFeeStructuresByAcademicYearAsync(string academicYear, string semester)
        {
            var structures = await _repository.GetFeeStructuresByAcademicYearAsync(academicYear, semester);
            return _mapper.Map<List<FeeStructureDto>>(structures);
        }

        public async Task<FeeStructureDto> CreateFeeStructureAsync(CreateFeeStructureDto dto)
        {
            var structure = _mapper.Map<FeeStructure>(dto);
            
            // Create fee structure items
            foreach (var itemDto in dto.FeeStructureItems)
            {
                var item = _mapper.Map<FeeStructureItem>(itemDto);
                structure.FeeStructureItems.Add(item);
            }

            var createdStructure = await _repository.CreateFeeStructureAsync(structure);
            return _mapper.Map<FeeStructureDto>(createdStructure);
        }

        public async Task<FeeStructureDto> UpdateFeeStructureAsync(int id, UpdateFeeStructureDto dto)
        {
            var existingStructure = await _repository.GetFeeStructureByIdAsync(id);
            if (existingStructure == null)
                throw new InvalidOperationException($"Fee structure with ID {id} not found.");

            // Update basic properties
            existingStructure.Name = dto.Name;
            existingStructure.Description = dto.Description;
            existingStructure.AcademicYear = dto.AcademicYear;
            existingStructure.Semester = dto.Semester;
            existingStructure.IsActive = dto.IsActive;
            
            // Update fee structure items
            existingStructure.FeeStructureItems.Clear();
            foreach (var itemDto in dto.FeeStructureItems)
            {
                var item = _mapper.Map<FeeStructureItem>(itemDto);
                existingStructure.FeeStructureItems.Add(item);
            }

            var updatedStructure = await _repository.UpdateFeeStructureAsync(existingStructure);
            return _mapper.Map<FeeStructureDto>(updatedStructure);
        }

        public async Task<bool> DeleteFeeStructureAsync(int id)
        {
            return await _repository.DeleteFeeStructureAsync(id);
        }

        public async Task<FeeStructureDto> ReactivateFeeStructureAsync(int id)
        {
            var existingStructure = await _repository.GetFeeStructureByIdAsync(id);
            if (existingStructure == null)
                throw new InvalidOperationException($"Fee structure with ID {id} not found.");

            existingStructure.IsActive = true;
            existingStructure.UpdatedAt = DateTime.UtcNow;

            var updatedStructure = await _repository.UpdateFeeStructureAsync(existingStructure);
            return _mapper.Map<FeeStructureDto>(updatedStructure);
        }

        // Additional Fee Operations
        public async Task<List<AdditionalFeeDto>> GetAllAdditionalFeesAsync()
        {
            var fees = await _repository.GetAllAdditionalFeesAsync();
            return _mapper.Map<List<AdditionalFeeDto>>(fees);
        }

        public async Task<AdditionalFeeDto?> GetAdditionalFeeByIdAsync(int id)
        {
            var fee = await _repository.GetAdditionalFeeByIdAsync(id);
            return _mapper.Map<AdditionalFeeDto>(fee);
        }

        public async Task<AdditionalFeeDto> CreateAdditionalFeeAsync(CreateAdditionalFeeDto dto)
        {
            var fee = _mapper.Map<AdditionalFee>(dto);
            var createdFee = await _repository.CreateAdditionalFeeAsync(fee);
            return _mapper.Map<AdditionalFeeDto>(createdFee);
        }

        public async Task<AdditionalFeeDto> UpdateAdditionalFeeAsync(int id, UpdateAdditionalFeeDto dto)
        {
            var existingFee = await _repository.GetAdditionalFeeByIdAsync(id);
            if (existingFee == null)
                throw new InvalidOperationException($"Additional fee with ID {id} not found.");

            _mapper.Map(dto, existingFee);
            var updatedFee = await _repository.UpdateAdditionalFeeAsync(existingFee);
            return _mapper.Map<AdditionalFeeDto>(updatedFee);
        }

        public async Task<bool> DeleteAdditionalFeeAsync(int id)
        {
            return await _repository.DeleteAdditionalFeeAsync(id);
        }

        // Student Fee Assignment Operations
        public async Task<List<StudentFeeAssignmentDto>> GetStudentFeeAssignmentsAsync(string studentNumber)
        {
            var assignments = await _repository.GetStudentFeeAssignmentsAsync(studentNumber);
            return _mapper.Map<List<StudentFeeAssignmentDto>>(assignments);
        }

        public async Task<StudentFeeAssignmentDto> AssignFeeStructureToStudentAsync(AssignFeeStructureDto dto)
        {
            // Check if assignment already exists
            var exists = await _repository.ExistsStudentFeeAssignmentAsync(
                dto.StudentNumber, dto.FeeStructureId, dto.AcademicYear, dto.Semester);
            
            if (exists)
                throw new InvalidOperationException("Fee structure already assigned to this student for the specified academic year and semester.");

            var assignment = _mapper.Map<StudentFeeAssignment>(dto);
            var createdAssignment = await _repository.CreateStudentFeeAssignmentAsync(assignment);
            
            // Generate fee balances for the student
            await GenerateFeeBalancesForStudentAsync(dto.StudentNumber, dto.FeeStructureId);
            
            return _mapper.Map<StudentFeeAssignmentDto>(createdAssignment);
        }

        public async Task<List<StudentFeeAssignmentDto>> BulkAssignFeeStructureAsync(BulkAssignFeeStructureDto dto)
        {
            var assignments = new List<StudentFeeAssignmentDto>();
            foreach (var studentNumber in dto.StudentNumbers)
            {
                try
                {
                    var assignmentDto = new AssignFeeStructureDto
                    {
                        StudentNumber = studentNumber,
                        FeeStructureId = dto.FeeStructureId,
                        AcademicYear = dto.AcademicYear,
                        Semester = dto.Semester
                    };
                    var assignment = await AssignFeeStructureToStudentAsync(assignmentDto);
                    assignments.Add(assignment);

                    // --- Reconcile the new StudentFeeBalance for this student and structure ---
                    var balances = await _repository.GetStudentFeeBalancesAsync(studentNumber);
                    var payments = await _repository.GetAllPaymentsAsync();
                    var studentPayments = payments.Where(p => p.StudentNumber == studentNumber).ToList();
                    var totalPaid = studentPayments.Sum(p => p.AmountPaid);
                    foreach (var balance in balances)
                    {
                        if (balance.FeeStructureItem != null && balance.FeeStructureItem.FeeStructureId == dto.FeeStructureId)
                        {
                            balance.AmountPaid = totalPaid;
                            balance.OutstandingBalance = balance.TotalAmount - totalPaid;
                            // Update status
                            if (balance.OutstandingBalance <= 0)
                                balance.Status = FeeBalanceStatus.Paid;
                            else if (totalPaid > 0)
                                balance.Status = FeeBalanceStatus.Partial;
                            else
                                balance.Status = FeeBalanceStatus.Outstanding;
                            await _repository.UpdateStudentFeeBalanceAsync(balance);
                        }
                    }
                    // --- End reconciliation ---
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other students
                    Console.WriteLine($"Error assigning fee structure to student {studentNumber}: {ex.Message}");
                }
            }
            return assignments;
        }

        public async Task<bool> RemoveFeeStructureAssignmentAsync(int assignmentId)
        {
            return await _repository.DeleteStudentFeeAssignmentAsync(assignmentId);
        }

        public async Task<AssignmentResultDto> AssignFeeStructureToAllAsync(int feeStructureId)
        {
            var result = new AssignmentResultDto();
            
            // Get the fee structure
            var feeStructure = await _repository.GetFeeStructureByIdAsync(feeStructureId);
            if (feeStructure == null)
                throw new InvalidOperationException($"Fee structure with ID {feeStructureId} not found");

            // Get all students
            var allStudents = await _repository.GetAllStudentsAsync();
            
            // Get all existing assignments for this fee structure
            var existingAssignments = await _repository.GetAllStudentFeeAssignmentsAsync();
            var existingForThisStructure = existingAssignments
                .Where(a => a.FeeStructureId == feeStructureId)
                .Select(a => a.StudentNumber)
                .ToHashSet();

            // Get outstanding balances from previous semesters for each student
            var outstandingBalances = await _repository.GetAllStudentFeeBalancesAsync();
            
            foreach (var student in allStudents)
            {
                // Skip if already assigned this fee structure
                if (existingForThisStructure.Contains(student.StudentNumber))
                    continue;

                // Calculate outstanding amount from previous semesters
                var previousOutstanding = outstandingBalances
                    .Where(b => b.StudentNumber == student.StudentNumber && 
                               b.OutstandingBalance > 0)
                    .Sum(b => b.OutstandingBalance);

                // Create new assignment
                var assignment = new StudentFeeAssignment
                {
                    StudentNumber = student.StudentNumber,
                    FeeStructureId = feeStructureId,
                    AcademicYear = feeStructure.AcademicYear,
                    Semester = feeStructure.Semester,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = "Admin"
                };

                await _repository.CreateStudentFeeAssignmentAsync(assignment);

                // Create fee balance entries for each fee structure item
                var feeStructureItems = await _repository.GetFeeStructureItemsByStructureIdAsync(feeStructureId);
                foreach (var item in feeStructureItems)
                {
                    var balance = new StudentFeeBalance
                    {
                        StudentNumber = student.StudentNumber,
                        FeeStructureItemId = item.Id,
                        TotalAmount = item.Amount,
                        AmountPaid = 0,
                        OutstandingBalance = item.Amount + (previousOutstanding > 0 ? previousOutstanding : 0),
                        DueDate = item.DueDate ?? DateTime.UtcNow.AddDays(30),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _repository.CreateStudentFeeBalanceAsync(balance);
                }

                result.TotalAssigned++;
                if (previousOutstanding > 0)
                {
                    result.OutstandingBalancesAdded++;
                    result.TotalOutstandingAmount += previousOutstanding;
                }
            }

            await _repository.SaveChangesAsync();
            return result;
        }

        // Student Fee Balance Operations
        public async Task<StudentFeeBalanceSummaryDto> GetStudentFeeBalanceSummaryAsync(string studentNumber)
        {
            var feeBalances = await _repository.GetStudentFeeBalancesAsync(studentNumber);
            var additionalFees = await _repository.GetStudentAdditionalFeesAsync(studentNumber);
            
            // Get student information
            var student = await _repository.GetStudentsByNumbersAsync(new List<string> { studentNumber });
            var studentInfo = student.FirstOrDefault();
            
            var summary = new StudentFeeBalanceSummaryDto
            {
                StudentNumber = studentNumber,
                StudentName = studentInfo?.FullName ?? "Unknown Student",
                Program = studentInfo?.Program ?? "Unknown Program",
                FeeBalances = _mapper.Map<List<StudentFeeBalanceDto>>(feeBalances),
                AdditionalFees = _mapper.Map<List<StudentAdditionalFeeDto>>(additionalFees)
            };
            
            // Calculate totals
            summary.TotalOutstandingBalance = feeBalances.Sum(fb => fb.OutstandingBalance) + 
                                            additionalFees.Sum(af => af.Amount);
            summary.TotalPaid = feeBalances.Sum(fb => fb.AmountPaid);
            
            // Get next payment due date
            var nextDueDate = feeBalances
                .Where(fb => fb.OutstandingBalance > 0 && fb.DueDate > DateTime.UtcNow)
                .OrderBy(fb => fb.DueDate)
                .FirstOrDefault()?.DueDate ?? DateTime.UtcNow;
            
            summary.NextPaymentDue = nextDueDate;
            
            return summary;
        }

        public async Task<List<StudentFeeBalanceDto>> GetStudentFeeBalancesAsync(string studentNumber)
        {
            var balances = await _repository.GetStudentFeeBalancesAsync(studentNumber);
            return _mapper.Map<List<StudentFeeBalanceDto>>(balances);
        }

        public async Task<List<StudentFeeBalanceDto>> GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus status)
        {
            var balances = await _repository.GetStudentFeeBalancesByStatusAsync(status);
            return _mapper.Map<List<StudentFeeBalanceDto>>(balances);
        }

        public async Task<StudentFeeBalanceDto> UpdateStudentFeeBalanceAsync(int balanceId, decimal amountPaid)
        {
            var balance = await _repository.GetStudentFeeBalanceByIdAsync(balanceId);
            if (balance == null)
                throw new InvalidOperationException($"Fee balance with ID {balanceId} not found.");

            balance.AmountPaid += amountPaid;
            balance.OutstandingBalance = balance.TotalAmount - balance.AmountPaid;
            
            // Update status
            if (balance.OutstandingBalance <= 0)
                balance.Status = FeeBalanceStatus.Paid;
            else if (balance.AmountPaid > 0)
                balance.Status = FeeBalanceStatus.Partial;
            else
                balance.Status = FeeBalanceStatus.Outstanding;

            var updatedBalance = await _repository.UpdateStudentFeeBalanceAsync(balance);
            return _mapper.Map<StudentFeeBalanceDto>(updatedBalance);
        }

        public async Task<bool> RecalculateStudentBalancesAsync(string studentNumber)
        {
            var balances = await _repository.GetStudentFeeBalancesAsync(studentNumber);
            
            foreach (var balance in balances)
            {
                balance.OutstandingBalance = balance.TotalAmount - balance.AmountPaid;
                
                // Update status
                if (balance.OutstandingBalance <= 0)
                    balance.Status = FeeBalanceStatus.Paid;
                else if (balance.AmountPaid > 0)
                    balance.Status = FeeBalanceStatus.Partial;
                else if (balance.DueDate < DateTime.UtcNow)
                    balance.Status = FeeBalanceStatus.Overdue;
                else
                    balance.Status = FeeBalanceStatus.Outstanding;

                await _repository.UpdateStudentFeeBalanceAsync(balance);
            }
            
            return true;
        }

        // Student Additional Fee Operations
        public async Task<List<StudentAdditionalFeeDto>> GetStudentAdditionalFeesAsync(string studentNumber)
        {
            var fees = await _repository.GetStudentAdditionalFeesAsync(studentNumber);
            return _mapper.Map<List<StudentAdditionalFeeDto>>(fees);
        }

        public async Task<StudentAdditionalFeeDto> AssignAdditionalFeeToStudentAsync(string studentNumber, int additionalFeeId)
        {
            // Check if already assigned
            var existing = await _repository.GetStudentAdditionalFeeByStudentAndFeeAsync(studentNumber, additionalFeeId);
            if (existing != null)
                throw new InvalidOperationException("Additional fee already assigned to this student.");

            var additionalFee = await _repository.GetAdditionalFeeByIdAsync(additionalFeeId);
            if (additionalFee == null)
                throw new InvalidOperationException($"Additional fee with ID {additionalFeeId} not found.");

            var studentAdditionalFee = new StudentAdditionalFee
            {
                StudentNumber = studentNumber,
                AdditionalFeeId = additionalFeeId,
                Amount = additionalFee.Amount,
                DueDate = additionalFee.StartDate ?? DateTime.UtcNow.AddDays(30),
                Status = FeeBalanceStatus.Outstanding
            };

            var created = await _repository.CreateStudentAdditionalFeeAsync(studentAdditionalFee);
            return _mapper.Map<StudentAdditionalFeeDto>(created);
        }

        public async Task<bool> RemoveAdditionalFeeFromStudentAsync(int studentAdditionalFeeId)
        {
            return await _repository.DeleteStudentAdditionalFeeAsync(studentAdditionalFeeId);
        }

        // Report Operations
        public async Task<FeeReportDto> GetFeeReportAsync()
        {
            var structures = await _repository.GetAllFeeStructuresAsync();
            var additionalFees = await _repository.GetAllAdditionalFeesAsync();
            var outstandingBalances = await _repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Outstanding);
            var overdueBalances = await _repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Overdue);

            return new FeeReportDto
            {
                ActiveFeeStructures = _mapper.Map<List<FeeStructureDto>>(structures),
                ActiveAdditionalFees = _mapper.Map<List<AdditionalFeeDto>>(additionalFees),
                StudentsWithOutstandingFees = outstandingBalances.Count + overdueBalances.Count,
                TotalOutstandingAmount = outstandingBalances.Sum(b => b.OutstandingBalance) + 
                                       overdueBalances.Sum(b => b.OutstandingBalance),
                TotalCollectedAmount = outstandingBalances.Sum(b => b.AmountPaid) + 
                                     overdueBalances.Sum(b => b.AmountPaid)
            };
        }

        public async Task<FeeReportDto> GetFeeReportByAcademicYearAsync(string academicYear, string semester)
        {
            var structures = await _repository.GetFeeStructuresByAcademicYearAsync(academicYear, semester);
            var additionalFees = await _repository.GetAllAdditionalFeesAsync();
            var outstandingBalances = await _repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Outstanding);
            var overdueBalances = await _repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Overdue);

            return new FeeReportDto
            {
                ActiveFeeStructures = _mapper.Map<List<FeeStructureDto>>(structures),
                ActiveAdditionalFees = _mapper.Map<List<AdditionalFeeDto>>(additionalFees),
                StudentsWithOutstandingFees = outstandingBalances.Count + overdueBalances.Count,
                TotalOutstandingAmount = outstandingBalances.Sum(b => b.OutstandingBalance) + 
                                       overdueBalances.Sum(b => b.OutstandingBalance),
                TotalCollectedAmount = outstandingBalances.Sum(b => b.AmountPaid) + 
                                     overdueBalances.Sum(b => b.AmountPaid)
            };
        }

        public async Task<List<StudentFeeBalanceSummaryDto>> GetStudentsWithOutstandingFeesAsync()
        {
            var outstandingBalances = await _repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Outstanding);
            var overdueBalances = await _repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Overdue);
            
            var allBalances = outstandingBalances.Concat(overdueBalances).ToList();
            var studentNumbers = allBalances.Select(b => b.StudentNumber).Distinct().ToList();
            
            var summaries = new List<StudentFeeBalanceSummaryDto>();
            foreach (var studentNumber in studentNumbers)
            {
                try
                {
                    var summary = await GetStudentFeeBalanceSummaryAsync(studentNumber);
                    if (summary.TotalOutstandingBalance > 0)
                    {
                        summaries.Add(summary);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other students
                    Console.WriteLine($"Error getting balance summary for student {studentNumber}: {ex.Message}");
                }
            }
            
            return summaries.OrderBy(s => s.TotalOutstandingBalance).ToList();
        }

        public async Task<List<StudentFeeBalanceSummaryDto>> GetStudentsWithOverdueFeesAsync()
        {
            var overdueBalances = await _repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Overdue);
            var studentNumbers = overdueBalances.Select(b => b.StudentNumber).Distinct().ToList();
            
            var summaries = new List<StudentFeeBalanceSummaryDto>();
            foreach (var studentNumber in studentNumbers)
            {
                try
                {
                    var summary = await GetStudentFeeBalanceSummaryAsync(studentNumber);
                    if (summary.TotalOutstandingBalance > 0)
                    {
                        summaries.Add(summary);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other students
                    Console.WriteLine($"Error getting balance summary for student {studentNumber}: {ex.Message}");
                }
            }
            
            return summaries.OrderBy(s => s.NextPaymentDue).ToList();
        }

        // Utility Operations
        public async Task<bool> ApplyAdditionalFeesToStudentsAsync(int additionalFeeId)
        {
            var additionalFee = await _repository.GetAdditionalFeeByIdAsync(additionalFeeId);
            if (additionalFee == null) return false;

            List<Student> applicableStudents = new List<Student>();

            switch (additionalFee.ApplicableTo)
            {
                case FeeApplicability.All:
                    applicableStudents = await _repository.GetStudentsByProgramAsync(""); // Get all students
                    break;
                case FeeApplicability.Program:
                    if (!string.IsNullOrEmpty(additionalFee.ApplicablePrograms))
                    {
                        var programs = JsonSerializer.Deserialize<List<string>>(additionalFee.ApplicablePrograms);
                        foreach (var program in programs)
                        {
                            var students = await _repository.GetStudentsByProgramAsync(program);
                            applicableStudents.AddRange(students);
                        }
                    }
                    break;
                case FeeApplicability.Class:
                    if (!string.IsNullOrEmpty(additionalFee.ApplicableClasses))
                    {
                        var classes = JsonSerializer.Deserialize<List<string>>(additionalFee.ApplicableClasses);
                        foreach (var className in classes)
                        {
                            var students = await _repository.GetStudentsByClassAsync(className);
                            applicableStudents.AddRange(students);
                        }
                    }
                    break;
                case FeeApplicability.Individual:
                    if (!string.IsNullOrEmpty(additionalFee.ApplicableStudents))
                    {
                        var studentNumbers = JsonSerializer.Deserialize<List<string>>(additionalFee.ApplicableStudents);
                        applicableStudents = await _repository.GetStudentsByNumbersAsync(studentNumbers);
                    }
                    break;
            }

            foreach (var student in applicableStudents.Distinct())
            {
                try
                {
                    await AssignAdditionalFeeToStudentAsync(student.StudentNumber, additionalFeeId);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other students
                    Console.WriteLine($"Error applying additional fee to student {student.StudentNumber}: {ex.Message}");
                }
            }

            return true;
        }

        public async Task<bool> UpdateFeeBalanceStatusesAsync()
        {
            var allBalances = await _repository.GetStudentFeeBalancesAsync(""); // Get all balances
            var updated = false;

            foreach (var balance in allBalances)
            {
                var originalStatus = balance.Status;
                
                if (balance.OutstandingBalance <= 0)
                    balance.Status = FeeBalanceStatus.Paid;
                else if (balance.AmountPaid > 0)
                    balance.Status = FeeBalanceStatus.Partial;
                else if (balance.DueDate < DateTime.UtcNow)
                    balance.Status = FeeBalanceStatus.Overdue;
                else
                    balance.Status = FeeBalanceStatus.Outstanding;

                if (balance.Status != originalStatus)
                {
                    await _repository.UpdateStudentFeeBalanceAsync(balance);
                    updated = true;
                }
            }

            return updated;
        }

        public async Task<bool> GenerateFeeBalancesForNewAssignmentsAsync()
        {
            try
            {
                // Get all fee structure assignments
                var assignments = await _repository.GetAllStudentFeeAssignmentsAsync();
                var generatedCount = 0;

                foreach (var assignment in assignments)
                {
                    try
                    {
                        // Generate fee balances for this assignment
                        await GenerateFeeBalancesForStudentAsync(assignment.StudentNumber, assignment.FeeStructureId);
                        generatedCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other assignments
                        Console.WriteLine($"Error generating balances for assignment {assignment.Id}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Generated fee balances for {generatedCount} assignments");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateFeeBalancesForNewAssignmentsAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MigrateOldStudentBalancesAsync()
        {
            try
            {
                Console.WriteLine("Starting migration of old student balances...");
                
                // Get all old student balances from the StudentBalance table
                var oldBalances = await _repository.GetAllOldStudentBalancesAsync();
                Console.WriteLine($"Found {oldBalances.Count} old student balances to migrate");
                
                var migratedCount = 0;
                var errorCount = 0;

                foreach (var oldBalance in oldBalances)
                {
                    try
                    {
                        Console.WriteLine($"Processing student {oldBalance.StudentNumber} with balance ${oldBalance.OutstandingBalance}");
                        
                        // Find or create a fee structure item that matches this balance
                        var feeStructureItem = await _repository.GetOrCreateFeeStructureItemForOldBalanceAsync(oldBalance);
                        
                        if (feeStructureItem != null)
                        {
                            Console.WriteLine($"Created/found fee structure item {feeStructureItem.Id} for student {oldBalance.StudentNumber}");
                            
                            // Check if a StudentFeeBalance already exists for this student and item
                            var existingBalance = await _repository.GetStudentFeeBalanceByStudentAndItemAsync(
                                oldBalance.StudentNumber, feeStructureItem.Id);
                            
                            if (existingBalance == null)
                            {
                                // Convert old balance status to new enum
                                var newStatus = ConvertOldStatusToNewStatus(oldBalance.Status);
                                Console.WriteLine($"Converting status from '{oldBalance.Status}' to '{newStatus}'");
                                
                                var newBalance = new StudentFeeBalance
                                {
                                    StudentNumber = oldBalance.StudentNumber,
                                    FeeStructureItemId = feeStructureItem.Id,
                                    TotalAmount = oldBalance.TotalAmount,
                                    AmountPaid = oldBalance.AmountPaid,
                                    OutstandingBalance = oldBalance.OutstandingBalance,
                                    DueDate = oldBalance.DueDate,
                                    Status = newStatus,
                                    IsActive = oldBalance.IsActive,
                                    CreatedAt = oldBalance.CreatedAt,
                                    UpdatedAt = oldBalance.UpdatedAt
                                };

                                await _repository.CreateStudentFeeBalanceAsync(newBalance);
                                migratedCount++;
                                Console.WriteLine($"Successfully migrated balance for student {oldBalance.StudentNumber}");
                            }
                            else
                            {
                                Console.WriteLine($"Balance already exists for student {oldBalance.StudentNumber}, skipping");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to create fee structure item for student {oldBalance.StudentNumber}");
                            errorCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other balances
                        Console.WriteLine($"Error migrating balance for student {oldBalance.StudentNumber}: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        errorCount++;
                    }
                }

                Console.WriteLine($"Migration completed. Migrated: {migratedCount}, Errors: {errorCount}");
                
                if (errorCount > 0)
                {
                    Console.WriteLine("Some balances failed to migrate. Check the logs above for details.");
                }
                
                return migratedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MigrateOldStudentBalancesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private FeeBalanceStatus ConvertOldStatusToNewStatus(string oldStatus)
        {
            return oldStatus.ToLower() switch
            {
                "outstanding" => FeeBalanceStatus.Outstanding,
                "partial" => FeeBalanceStatus.Partial,
                "paid" => FeeBalanceStatus.Paid,
                "overdue" => FeeBalanceStatus.Overdue,
                _ => FeeBalanceStatus.Outstanding
            };
        }

        // Private helper methods
        private async Task GenerateFeeBalancesForStudentAsync(string studentNumber, int feeStructureId)
        {
            var feeStructure = await _repository.GetFeeStructureByIdAsync(feeStructureId);
            if (feeStructure == null) return;

            foreach (var item in feeStructure.FeeStructureItems)
            {
                var existingBalance = await _repository.GetStudentFeeBalanceByStudentAndItemAsync(studentNumber, item.Id);
                if (existingBalance != null) continue; // Already exists

                var balance = new StudentFeeBalance
                {
                    StudentNumber = studentNumber,
                    FeeStructureItemId = item.Id,
                    TotalAmount = item.Amount,
                    AmountPaid = 0,
                    OutstandingBalance = item.Amount,
                    DueDate = item.DueDate ?? DateTime.UtcNow.AddDays(30),
                    Status = FeeBalanceStatus.Outstanding
                };

                await _repository.CreateStudentFeeBalanceAsync(balance);
            }
        }

        public async Task<bool> ReconcileStudentFeeBalancesAsync()
        {
            // Get all StudentFeeBalance records
            var allFeeBalances = await _repository.GetAllStudentFeeBalancesAsync();
            if (allFeeBalances.Count == 0) return false;

            // Get all payments
            var allPayments = await _repository.GetAllPaymentsAsync();

            int updatedCount = 0;
            foreach (var feeBalance in allFeeBalances)
            {
                // Sum all payments for this student
                var studentPayments = allPayments
                    .Where(p => p.StudentNumber == feeBalance.StudentNumber)
                    .ToList();
                var totalPaid = studentPayments.Sum(p => p.AmountPaid);
                var newOutstanding = Math.Max(0, feeBalance.TotalAmount - totalPaid);

                // Only update if values have changed
                if (feeBalance.AmountPaid != totalPaid || feeBalance.OutstandingBalance != newOutstanding)
                {
                    feeBalance.AmountPaid = totalPaid;
                    feeBalance.OutstandingBalance = newOutstanding;

                    // Update status
                    if (feeBalance.OutstandingBalance <= 0)
                        feeBalance.Status = FeeBalanceStatus.Paid;
                    else if (feeBalance.AmountPaid > 0)
                        feeBalance.Status = FeeBalanceStatus.Partial;
                    else
                        feeBalance.Status = FeeBalanceStatus.Outstanding;

                    await _repository.UpdateStudentFeeBalanceAsync(feeBalance);
                    updatedCount++;
                }
            }
            return updatedCount > 0;
        }

        public async Task<List<StudentFeeBalance>> GetAllStudentFeeBalancesAsync()
        {
            return await _repository.GetAllStudentFeeBalancesAsync();
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _repository.GetAllStudentsAsync();
        }

        public async Task<List<StudentFeeAssignment>> GetAllStudentFeeAssignmentsAsync()
        {
            return await _repository.GetAllStudentFeeAssignmentsAsync();
        }
    }
} 