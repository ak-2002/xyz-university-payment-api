using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Application.DTOs;
using xyz_university_payment_api.Presentation.Attributes;
using xyz_university_payment_api.Core.Application.Services;

namespace xyz_university_payment_api.Presentation.Controllers.V3
{
    /// <summary>
    /// Controller for data seeding operations
    /// </summary>
    [ApiController]
    [Route("api/v3/[controller]")]
    [ApiVersion("3.0")]
    public class DataSeedingControllerV3 : ControllerBase
    {
        private readonly IDataSeedingService _dataSeedingService;
        private readonly ILogger<DataSeedingControllerV3> _logger;

        public DataSeedingControllerV3(
            IDataSeedingService dataSeedingService,
            ILogger<DataSeedingControllerV3> logger)
        {
            _dataSeedingService = dataSeedingService;
            _logger = logger;
        }

        /// <summary>
        /// Check if seed data exists
        /// </summary>
        /// <returns>Status of seed data</returns>
        [HttpGet("status")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetSeedDataStatus()
        {
            try
            {
                var hasSeedData = await _dataSeedingService.HasSeedDataAsync();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Seed data status retrieved successfully",
                    Data = new
                    {
                        HasSeedData = hasSeedData,
                        Message = hasSeedData ? "Seed data exists" : "No seed data found"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking seed data status");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to check seed data status",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Seed fee schedules (Admin only)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("seed-fee-schedules")]
        [AuthorizeRole("Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> SeedFeeSchedules()
        {
            try
            {
                await _dataSeedingService.SeedFeeSchedulesAsync();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Fee schedules seeded successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding fee schedules");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to seed fee schedules",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Seed student balances (Admin only)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("seed-student-balances")]
        [AuthorizeRole("Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> SeedStudentBalances()
        {
            try
            {
                await _dataSeedingService.SeedStudentBalancesAsync();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Student balances seeded successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding student balances");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to seed student balances",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Seed all data (Admin only)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("seed-all")]
        [AuthorizeRole("Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> SeedAllData()
        {
            try
            {
                await _dataSeedingService.SeedAllDataAsync();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "All data seeded successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding all data");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to seed all data",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get sample fee schedule data (for reference)
        /// </summary>
        /// <returns>Sample fee schedule data</returns>
        [HttpGet("sample-fee-schedules")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult GetSampleFeeSchedules()
        {
            var sampleData = new
            {
                FeeSchedules = new[]
                {
                    new
                    {
                        Semester = "Summer",
                        AcademicYear = "2025",
                        Program = "Computer Science",
                        TuitionFee = 4500.00m,
                        RegistrationFee = 500.00m,
                        LibraryFee = 200.00m,
                        LaboratoryFee = 300.00m,
                        OtherFees = 100.00m,
                        TotalAmount = 5600.00m
                    },
                    new
                    {
                        Semester = "Summer",
                        AcademicYear = "2025",
                        Program = "Business Administration",
                        TuitionFee = 4000.00m,
                        RegistrationFee = 500.00m,
                        LibraryFee = 200.00m,
                        LaboratoryFee = 0.00m,
                        OtherFees = 100.00m,
                        TotalAmount = 4800.00m
                    },
                    new
                    {
                        Semester = "Summer",
                        AcademicYear = "2025",
                        Program = "Engineering",
                        TuitionFee = 5000.00m,
                        RegistrationFee = 500.00m,
                        LibraryFee = 200.00m,
                        LaboratoryFee = 400.00m,
                        OtherFees = 150.00m,
                        TotalAmount = 6250.00m
                    }
                },
                Description = "Sample fee schedules for Summer 2025 semester"
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Sample fee schedule data retrieved successfully",
                Data = sampleData
            });
        }
    }
} 