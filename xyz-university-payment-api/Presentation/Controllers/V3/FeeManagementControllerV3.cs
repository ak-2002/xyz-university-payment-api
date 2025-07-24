using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Infrastructure.Data.Repositories;
using xyz_university_payment_api.Core.Application.Services;

namespace xyz_university_payment_api.Presentation.Controllers.V3
{
    [ApiController]
    [Route("api/v3/feemanagement")]
    [ApiVersion("3.0")]
    [ApiExplorerSettings(GroupName = "v3")]
    [Authorize(Roles = "Admin,Manager,Student")]
    public class FeeManagementControllerV3 : ControllerBase
    {
        private readonly IFeeManagementService _feeManagementService;

        public FeeManagementControllerV3(IFeeManagementService feeManagementService)
        {
            _feeManagementService = feeManagementService;
        }

        /// <summary>
        /// Test endpoint to verify controller is working
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public ActionResult<string> Test()
        {
            return Ok("FeeManagementControllerV3 is working!");
        }

        #region Fee Categories

        /// <summary>
        /// Get all fee categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<FeeCategoryDto>>> GetFeeCategories()
        {
            try
            {
                var categories = await _feeManagementService.GetAllFeeCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve fee categories", details = ex.Message });
            }
        }

        /// <summary>
        /// Get fee category by ID
        /// </summary>
        [HttpGet("categories/{id}")]
        public async Task<ActionResult<FeeCategoryDto>> GetFeeCategory(int id)
        {
            try
            {
                var category = await _feeManagementService.GetFeeCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(new { error = "Fee category not found" });

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve fee category", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new fee category
        /// </summary>
        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeeCategoryDto>> CreateFeeCategory([FromBody] CreateFeeCategoryDto dto)
        {
            try
            {
                var category = await _feeManagementService.CreateFeeCategoryAsync(dto);
                return CreatedAtAction(nameof(GetFeeCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create fee category", details = ex.Message });
            }
        }

        /// <summary>
        /// Update fee category
        /// </summary>
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeeCategoryDto>> UpdateFeeCategory(int id, [FromBody] UpdateFeeCategoryDto dto)
        {
            try
            {
                var category = await _feeManagementService.UpdateFeeCategoryAsync(id, dto);
                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update fee category", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete fee category
        /// </summary>
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteFeeCategory(int id)
        {
            try
            {
                var result = await _feeManagementService.DeleteFeeCategoryAsync(id);
                if (!result)
                    return NotFound(new { error = "Fee category not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete fee category", details = ex.Message });
            }
        }

        #endregion

        #region Fee Structures

        /// <summary>
        /// Get all fee structures
        /// </summary>
        [HttpGet("structures")]
        public async Task<ActionResult<List<FeeStructureDto>>> GetFeeStructures()
        {
            try
            {
                var structures = await _feeManagementService.GetAllFeeStructuresAsync();
                return Ok(structures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve fee structures", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all fee structures including inactive ones (Admin only)
        /// </summary>
        [HttpGet("structures/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<FeeStructureDto>>> GetAllFeeStructuresIncludingInactive()
        {
            try
            {
                var structures = await _feeManagementService.GetAllFeeStructuresIncludingInactiveAsync();
                return Ok(structures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve fee structures", details = ex.Message });
            }
        }

        /// <summary>
        /// Get fee structures by academic year and semester
        /// </summary>
        [HttpGet("structures/academic-year/{academicYear}/semester/{semester}")]
        public async Task<ActionResult<List<FeeStructureDto>>> GetFeeStructuresByAcademicYear(string academicYear, string semester)
        {
            try
            {
                var structures = await _feeManagementService.GetFeeStructuresByAcademicYearAsync(academicYear, semester);
                return Ok(structures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve fee structures", details = ex.Message });
            }
        }

        /// <summary>
        /// Get fee structure by ID
        /// </summary>
        [HttpGet("structures/{id}")]
        public async Task<ActionResult<FeeStructureDto>> GetFeeStructure(int id)
        {
            try
            {
                var structure = await _feeManagementService.GetFeeStructureByIdAsync(id);
                if (structure == null)
                    return NotFound(new { error = "Fee structure not found" });

                return Ok(structure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve fee structure", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new fee structure
        /// </summary>
        [HttpPost("structures")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeeStructureDto>> CreateFeeStructure([FromBody] CreateFeeStructureDto dto)
        {
            try
            {
                var structure = await _feeManagementService.CreateFeeStructureAsync(dto);
                return CreatedAtAction(nameof(GetFeeStructure), new { id = structure.Id }, structure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create fee structure", details = ex.Message });
            }
        }

        /// <summary>
        /// Update fee structure
        /// </summary>
        [HttpPut("structures/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeeStructureDto>> UpdateFeeStructure(int id, [FromBody] UpdateFeeStructureDto dto)
        {
            try
            {
                var structure = await _feeManagementService.UpdateFeeStructureAsync(id, dto);
                return Ok(structure);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update fee structure", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete fee structure
        /// </summary>
        [HttpDelete("structures/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteFeeStructure(int id)
        {
            try
            {
                var result = await _feeManagementService.DeleteFeeStructureAsync(id);
                if (!result)
                    return NotFound(new { error = "Fee structure not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete fee structure", details = ex.Message });
            }
        }

        /// <summary>
        /// Reactivate fee structure
        /// </summary>
        [HttpPatch("structures/{id}/reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeeStructureDto>> ReactivateFeeStructure(int id)
        {
            try
            {
                var structure = await _feeManagementService.ReactivateFeeStructureAsync(id);
                return Ok(structure);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to reactivate fee structure", details = ex.Message });
            }
        }

        #endregion

        #region Additional Fees

        /// <summary>
        /// Get all additional fees
        /// </summary>
        [HttpGet("additional-fees")]
        public async Task<ActionResult<List<AdditionalFeeDto>>> GetAdditionalFees()
        {
            try
            {
                var fees = await _feeManagementService.GetAllAdditionalFeesAsync();
                return Ok(fees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve additional fees", details = ex.Message });
            }
        }

        /// <summary>
        /// Get additional fee by ID
        /// </summary>
        [HttpGet("additional-fees/{id}")]
        public async Task<ActionResult<AdditionalFeeDto>> GetAdditionalFee(int id)
        {
            try
            {
                var fee = await _feeManagementService.GetAdditionalFeeByIdAsync(id);
                if (fee == null)
                    return NotFound(new { error = "Additional fee not found" });

                return Ok(fee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve additional fee", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new additional fee
        /// </summary>
        [HttpPost("additional-fees")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdditionalFeeDto>> CreateAdditionalFee([FromBody] CreateAdditionalFeeDto dto)
        {
            try
            {
                var fee = await _feeManagementService.CreateAdditionalFeeAsync(dto);
                return CreatedAtAction(nameof(GetAdditionalFee), new { id = fee.Id }, fee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create additional fee", details = ex.Message });
            }
        }

        /// <summary>
        /// Update additional fee
        /// </summary>
        [HttpPut("additional-fees/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdditionalFeeDto>> UpdateAdditionalFee(int id, [FromBody] UpdateAdditionalFeeDto dto)
        {
            try
            {
                var fee = await _feeManagementService.UpdateAdditionalFeeAsync(id, dto);
                return Ok(fee);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update additional fee", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete additional fee
        /// </summary>
        [HttpDelete("additional-fees/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAdditionalFee(int id)
        {
            try
            {
                var result = await _feeManagementService.DeleteAdditionalFeeAsync(id);
                if (!result)
                    return NotFound(new { error = "Additional fee not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete additional fee", details = ex.Message });
            }
        }

        /// <summary>
        /// Apply additional fee to applicable students
        /// </summary>
        [HttpPost("additional-fees/{id}/apply")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApplyAdditionalFee(int id)
        {
            try
            {
                var result = await _feeManagementService.ApplyAdditionalFeesToStudentsAsync(id);
                if (!result)
                    return NotFound(new { error = "Additional fee not found" });

                return Ok(new { message = "Additional fee applied successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to apply additional fee", details = ex.Message });
            }
        }

        #endregion

        #region Student Fee Assignments

        /// <summary>
        /// Get student fee assignments
        /// </summary>
        [HttpGet("students/{studentNumber}/assignments")]
        public async Task<ActionResult<List<StudentFeeAssignmentDto>>> GetStudentFeeAssignments(string studentNumber)
        {
            try
            {
                var assignments = await _feeManagementService.GetStudentFeeAssignmentsAsync(studentNumber);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve student fee assignments", details = ex.Message });
            }
        }

        /// <summary>
        /// Assign fee structure to student
        /// </summary>
        [HttpPost("students/assign-fee-structure")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentFeeAssignmentDto>> AssignFeeStructureToStudent([FromBody] AssignFeeStructureDto dto)
        {
            try
            {
                var assignment = await _feeManagementService.AssignFeeStructureToStudentAsync(dto);
                return Ok(assignment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to assign fee structure", details = ex.Message });
            }
        }

        /// <summary>
        /// Bulk assign fee structure to multiple students
        /// </summary>
        [HttpPost("students/bulk-assign-fee-structure")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<StudentFeeAssignmentDto>>> BulkAssignFeeStructure([FromBody] BulkAssignFeeStructureDto dto)
        {
            try
            {
                var assignments = await _feeManagementService.BulkAssignFeeStructureAsync(dto);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to bulk assign fee structure", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove fee structure assignment
        /// </summary>
        [HttpDelete("assignments/{assignmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RemoveFeeStructureAssignment(int assignmentId)
        {
            try
            {
                var result = await _feeManagementService.RemoveFeeStructureAssignmentAsync(assignmentId);
                if (!result)
                    return NotFound(new { error = "Fee structure assignment not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to remove fee structure assignment", details = ex.Message });
            }
        }

        #endregion

        #region Student Fee Balances

        /// <summary>
        /// Get student fee balance summary
        /// </summary>
        [HttpGet("students/{studentNumber}/balance-summary")]
        public async Task<ActionResult<StudentFeeBalanceSummaryDto>> GetStudentFeeBalanceSummary(string studentNumber)
        {
            try
            {
                var summary = await _feeManagementService.GetStudentFeeBalanceSummaryAsync(studentNumber);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve student fee balance summary", details = ex.Message });
            }
        }

        /// <summary>
        /// Get student fee balances
        /// </summary>
        [HttpGet("students/{studentNumber}/balances")]
        public async Task<ActionResult<List<StudentFeeBalanceDto>>> GetStudentFeeBalances(string studentNumber)
        {
            try
            {
                var balances = await _feeManagementService.GetStudentFeeBalancesAsync(studentNumber);
                return Ok(balances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve student fee balances", details = ex.Message });
            }
        }

        /// <summary>
        /// Update student fee balance
        /// </summary>
        [HttpPut("balances/{balanceId}/update-payment")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<StudentFeeBalanceDto>> UpdateStudentFeeBalance(int balanceId, [FromBody] decimal amountPaid)
        {
            try
            {
                var balance = await _feeManagementService.UpdateStudentFeeBalanceAsync(balanceId, amountPaid);
                return Ok(balance);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update student fee balance", details = ex.Message });
            }
        }

        /// <summary>
        /// Recalculate student balances
        /// </summary>
        [HttpPost("students/{studentNumber}/recalculate-balances")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RecalculateStudentBalances(string studentNumber)
        {
            try
            {
                var result = await _feeManagementService.RecalculateStudentBalancesAsync(studentNumber);
                return Ok(new { message = "Student balances recalculated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to recalculate student balances", details = ex.Message });
            }
        }

        #endregion

        #region Student Additional Fees

        /// <summary>
        /// Get student additional fees
        /// </summary>
        [HttpGet("students/{studentNumber}/additional-fees")]
        public async Task<ActionResult<List<StudentAdditionalFeeDto>>> GetStudentAdditionalFees(string studentNumber)
        {
            try
            {
                var fees = await _feeManagementService.GetStudentAdditionalFeesAsync(studentNumber);
                return Ok(fees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve student additional fees", details = ex.Message });
            }
        }

        /// <summary>
        /// Assign additional fee to student
        /// </summary>
        [HttpPost("students/{studentNumber}/assign-additional-fee/{additionalFeeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentAdditionalFeeDto>> AssignAdditionalFeeToStudent(string studentNumber, int additionalFeeId)
        {
            try
            {
                var fee = await _feeManagementService.AssignAdditionalFeeToStudentAsync(studentNumber, additionalFeeId);
                return Ok(fee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to assign additional fee", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove additional fee from student
        /// </summary>
        [HttpDelete("student-additional-fees/{studentAdditionalFeeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RemoveAdditionalFeeFromStudent(int studentAdditionalFeeId)
        {
            try
            {
                var result = await _feeManagementService.RemoveAdditionalFeeFromStudentAsync(studentAdditionalFeeId);
                if (!result)
                    return NotFound(new { error = "Student additional fee not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to remove additional fee", details = ex.Message });
            }
        }

        #endregion

        #region Reports

        /// <summary>
        /// Get fee report
        /// </summary>
        [HttpGet("reports/fee-report")]
        public async Task<ActionResult<FeeReportDto>> GetFeeReport()
        {
            try
            {
                var report = await _feeManagementService.GetFeeReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate fee report", details = ex.Message });
            }
        }

        /// <summary>
        /// Get fee report by academic year
        /// </summary>
        [HttpGet("reports/fee-report/{academicYear}/{semester}")]
        public async Task<ActionResult<FeeReportDto>> GetFeeReportByAcademicYear(string academicYear, string semester)
        {
            try
            {
                var report = await _feeManagementService.GetFeeReportByAcademicYearAsync(academicYear, semester);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate fee report", details = ex.Message });
            }
        }

        /// <summary>
        /// Get students with outstanding fees
        /// </summary>
        [HttpGet("reports/students-outstanding-fees")]
        public async Task<ActionResult<List<StudentFeeBalanceSummaryDto>>> GetStudentsWithOutstandingFees()
        {
            try
            {
                var students = await _feeManagementService.GetStudentsWithOutstandingFeesAsync();
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve students with outstanding fees", details = ex.Message });
            }
        }

        /// <summary>
        /// Get students with overdue fees
        /// </summary>
        [HttpGet("reports/students-overdue-fees")]
        public async Task<ActionResult<List<StudentFeeBalanceSummaryDto>>> GetStudentsWithOverdueFees()
        {
            try
            {
                var students = await _feeManagementService.GetStudentsWithOverdueFeesAsync();
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve students with overdue fees", details = ex.Message });
            }
        }

        #endregion

        #region Utility Operations

        /// <summary>
        /// Debug endpoint to check fee balance data
        /// </summary>
        [HttpGet("utility/debug-balances")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DebugFeeBalances()
        {
            try
            {
                var repository = HttpContext.RequestServices.GetRequiredService<IFeeManagementRepository>();
                
                // Get all fee balances
                var allBalances = await repository.GetAllStudentFeeBalancesAsync();
                
                // Get balances by status
                var outstandingBalances = await repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Outstanding);
                var overdueBalances = await repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Overdue);
                var partialBalances = await repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Partial);
                var paidBalances = await repository.GetStudentFeeBalancesByStatusAsync(FeeBalanceStatus.Paid);
                
                // Get fee assignments
                var assignments = await repository.GetAllStudentFeeAssignmentsAsync();
                
                return Ok(new
                {
                    totalFeeBalances = allBalances.Count,
                    outstandingBalances = outstandingBalances.Count,
                    overdueBalances = overdueBalances.Count,
                    partialBalances = partialBalances.Count,
                    paidBalances = paidBalances.Count,
                    totalAssignments = assignments.Count,
                    sampleBalances = allBalances.Take(5).Select(b => new
                    {
                        id = b.Id,
                        studentNumber = b.StudentNumber,
                        totalAmount = b.TotalAmount,
                        amountPaid = b.AmountPaid,
                        outstandingBalance = b.OutstandingBalance,
                        status = b.Status.ToString(),
                        isActive = b.IsActive,
                        dueDate = b.DueDate
                    }),
                    sampleAssignments = assignments.Take(5).Select(a => new
                    {
                        id = a.Id,
                        studentNumber = a.StudentNumber,
                        feeStructureId = a.FeeStructureId,
                        academicYear = a.AcademicYear,
                        semester = a.Semester
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve debug data", details = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint to verify database connectivity
        /// </summary>
        [HttpGet("utility/test-database")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TestDatabase()
        {
            try
            {
                var repository = HttpContext.RequestServices.GetRequiredService<IFeeManagementRepository>();
                
                // Test basic database operations
                var oldBalancesCount = 0;
                var feeCategoriesCount = 0;
                var feeStructuresCount = 0;
                
                try
                {
                    var oldBalances = await repository.GetAllOldStudentBalancesAsync();
                    oldBalancesCount = oldBalances.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting old balances: {ex.Message}");
                }
                
                try
                {
                    var feeCategories = await repository.GetAllFeeCategoriesAsync();
                    feeCategoriesCount = feeCategories.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting fee categories: {ex.Message}");
                }
                
                try
                {
                    var feeStructures = await repository.GetAllFeeStructuresAsync();
                    feeStructuresCount = feeStructures.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting fee structures: {ex.Message}");
                }
                
                return Ok(new
                {
                    message = "Database connectivity test completed",
                    oldStudentBalancesCount = oldBalancesCount,
                    feeCategoriesCount = feeCategoriesCount,
                    feeStructuresCount = feeStructuresCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Database test failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Update fee balance statuses
        /// </summary>
        [HttpPost("utility/update-balance-statuses")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateFeeBalanceStatuses()
        {
            try
            {
                var result = await _feeManagementService.UpdateFeeBalanceStatusesAsync();
                return Ok(new { message = "Fee balance statuses updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update fee balance statuses", details = ex.Message });
            }
        }

        /// <summary>
        /// Generate fee balances for new assignments
        /// </summary>
        [HttpPost("utility/generate-balances")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GenerateFeeBalancesForNewAssignments()
        {
            try
            {
                var result = await _feeManagementService.GenerateFeeBalancesForNewAssignmentsAsync();
                return Ok(new { message = "Fee balances generated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate fee balances", details = ex.Message });
            }
        }

        /// <summary>
        /// Migrate old student balances to new fee management system
        /// </summary>
        [HttpPost("utility/migrate-old-balances")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> MigrateOldStudentBalances()
        {
            try
            {
                Console.WriteLine("Migration endpoint called");
                var result = await _feeManagementService.MigrateOldStudentBalancesAsync();
                
                if (result)
                {
                    return Ok(new { message = "Old student balances migrated successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Migration completed but no records were migrated" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    error = "Failed to migrate old student balances", 
                    details = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Reconcile StudentFeeBalance records with actual payments
        /// </summary>
        [HttpPost("utility/reconcile-fee-balances")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReconcileStudentFeeBalances()
        {
            try
            {
                var result = await _feeManagementService.ReconcileStudentFeeBalancesAsync();
                if (result)
                {
                    return Ok(new { message = "Student fee balances reconciled successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Reconciliation completed but no records were updated" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to reconcile student fee balances", details = ex.Message });
            }
        }

        /// <summary>
        /// Flexible assignment of fee structure to students (by list, by program, or all)
        /// </summary>
        [HttpPost("utility/assign-fee-structure")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> FlexibleAssignFeeStructure([FromBody] FlexibleAssignFeeStructureDto dto)
        {
            try
            {
                // Get all students
                var allStudents = await _feeManagementService.GetAllStudentsAsync();
                var studentsToAssign = new List<string>();
                if (dto.StudentNumbers != null && dto.StudentNumbers.Any())
                {
                    studentsToAssign.AddRange(dto.StudentNumbers);
                }
                if (dto.Programs != null && dto.Programs.Any())
                {
                    var programStudents = allStudents.Where(s => dto.Programs.Contains(s.Program)).Select(s => s.StudentNumber);
                    studentsToAssign.AddRange(programStudents);
                }
                if ((!dto.StudentNumbers?.Any() ?? true) && (!dto.Programs?.Any() ?? true))
                {
                    // If no filters, assign to all students
                    studentsToAssign = allStudents.Select(s => s.StudentNumber).ToList();
                }
                studentsToAssign = studentsToAssign.Distinct().ToList();
                var bulkDto = new BulkAssignFeeStructureDto
                {
                    StudentNumbers = studentsToAssign,
                    FeeStructureId = dto.FeeStructureId,
                    AcademicYear = dto.AcademicYear,
                    Semester = dto.Semester
                };
                var result = await _feeManagementService.BulkAssignFeeStructureAsync(bulkDto);
                return Ok(new { message = $"Assigned fee structure to {result.Count} students" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to assign fee structure", details = ex.Message });
            }
        }

        /// <summary>
        /// Assign fee structure to all students with outstanding balance handling
        /// </summary>
        [HttpPost("utility/assign-fee-structure-to-all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignFeeStructureToAll([FromBody] AssignFeeStructureToAllDto dto)
        {
            try
            {
                var result = await _feeManagementService.AssignFeeStructureToAllAsync(dto.FeeStructureId);
                return Ok(new { 
                    message = $"Assigned fee structure to {result.TotalAssigned} students", 
                    outstandingBalancesAdded = result.OutstandingBalancesAdded,
                    totalOutstandingAmount = result.TotalOutstandingAmount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to assign fee structure", details = ex.Message });
            }
        }

        #endregion

        #region Data Seeding

        /// <summary>
        /// Seed fee management data
        /// </summary>
        [HttpPost("seed-data")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SeedFeeManagementData()
        {
            try
            {
                var seedingService = HttpContext.RequestServices.GetRequiredService<FeeManagementSeedingService>();
                var result = await seedingService.SeedFeeManagementDataAsync();
                
                if (result)
                {
                    return Ok(new { message = "Fee management data seeded successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to seed fee management data" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to seed fee management data", details = ex.Message });
            }
        }

        /// <summary>
        /// Apply additional fees to students
        /// </summary>
        [HttpPost("apply-additional-fees")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApplyAdditionalFees()
        {
            try
            {
                var seedingService = HttpContext.RequestServices.GetRequiredService<FeeManagementSeedingService>();
                var result = await seedingService.ApplyAdditionalFeesToStudentsAsync();
                
                if (result)
                {
                    return Ok(new { message = "Additional fees applied successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to apply additional fees" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to apply additional fees", details = ex.Message });
            }
        }

        #endregion
    }
} 