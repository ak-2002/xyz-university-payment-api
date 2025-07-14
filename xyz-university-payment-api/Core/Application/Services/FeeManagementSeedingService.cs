using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Infrastructure.Data.Repositories;
using System.Text.Json;

namespace xyz_university_payment_api.Core.Application.Services
{
    public class FeeManagementSeedingService
    {
        private readonly IFeeManagementRepository _repository;

        public FeeManagementSeedingService(IFeeManagementRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> SeedFeeManagementDataAsync()
        {
            try
            {
                // Seed Fee Categories
                await SeedFeeCategoriesAsync();
                
                // Seed Fee Structures
                await SeedFeeStructuresAsync();
                
                // Seed Additional Fees
                await SeedAdditionalFeesAsync();
                
                // Assign fee structures to existing students
                await AssignFeeStructuresToStudentsAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding fee management data: {ex.Message}");
                return false;
            }
        }

        private async Task SeedFeeCategoriesAsync()
        {
            var categories = new List<FeeCategory>
            {
                new FeeCategory
                {
                    Name = "Tuition Fee",
                    Description = "Main academic tuition fee for courses and instruction",
                    Type = FeeCategoryType.Standard,
                    Frequency = FeeFrequency.Recurring,
                    IsRequired = true,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Registration Fee",
                    Description = "One-time registration fee for new students",
                    Type = FeeCategoryType.Standard,
                    Frequency = FeeFrequency.OneTime,
                    IsRequired = true,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Library Fee",
                    Description = "Access to library resources and study materials",
                    Type = FeeCategoryType.Standard,
                    Frequency = FeeFrequency.Recurring,
                    IsRequired = true,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Laboratory Fee",
                    Description = "Access to laboratory facilities and equipment",
                    Type = FeeCategoryType.Standard,
                    Frequency = FeeFrequency.Recurring,
                    IsRequired = false,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Student Activity Fee",
                    Description = "Support for student clubs, events, and activities",
                    Type = FeeCategoryType.Standard,
                    Frequency = FeeFrequency.Recurring,
                    IsRequired = true,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Technology Fee",
                    Description = "Access to computer labs and technology resources",
                    Type = FeeCategoryType.Standard,
                    Frequency = FeeFrequency.Recurring,
                    IsRequired = true,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Health Services Fee",
                    Description = "Access to campus health services and medical care",
                    Type = FeeCategoryType.Standard,
                    Frequency = FeeFrequency.Recurring,
                    IsRequired = true,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Parking Fee",
                    Description = "Campus parking permit and facilities",
                    Type = FeeCategoryType.Additional,
                    Frequency = FeeFrequency.Recurring,
                    IsRequired = false,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Uniform Fee",
                    Description = "Required uniforms for specific programs",
                    Type = FeeCategoryType.Additional,
                    Frequency = FeeFrequency.OneTime,
                    IsRequired = false,
                    IsActive = true
                },
                new FeeCategory
                {
                    Name = "Graduation Fee",
                    Description = "Graduation ceremony and certificate processing",
                    Type = FeeCategoryType.Additional,
                    Frequency = FeeFrequency.OneTime,
                    IsRequired = false,
                    IsActive = true
                }
            };

            foreach (var category in categories)
            {
                var existing = await _repository.GetAllFeeCategoriesAsync();
                if (!existing.Any(c => c.Name == category.Name))
                {
                    await _repository.CreateFeeCategoryAsync(category);
                }
            }
        }

        private async Task SeedFeeStructuresAsync()
        {
            var categories = await _repository.GetAllFeeCategoriesAsync();
            
            // Summer 2025 Fee Structure
            var summerStructure = new FeeStructure
            {
                Name = "Standard Summer 2025",
                Description = "Standard fee structure for Summer 2025 semester",
                AcademicYear = "2025",
                Semester = "Summer",
                IsActive = true
            };

            await _repository.CreateFeeStructureAsync(summerStructure);

            // Add fee structure items
            var summerItems = new List<FeeStructureItem>
            {
                new FeeStructureItem
                {
                    FeeStructureId = summerStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Tuition Fee").Id,
                    Amount = 2500.00m,
                    IsRequired = true,
                    Description = "Summer semester tuition fee",
                    DueDate = DateTime.Parse("2025-06-15")
                },
                new FeeStructureItem
                {
                    FeeStructureId = summerStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Registration Fee").Id,
                    Amount = 150.00m,
                    IsRequired = true,
                    Description = "Summer registration fee",
                    DueDate = DateTime.Parse("2025-05-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = summerStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Library Fee").Id,
                    Amount = 75.00m,
                    IsRequired = true,
                    Description = "Summer library access fee",
                    DueDate = DateTime.Parse("2025-06-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = summerStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Student Activity Fee").Id,
                    Amount = 50.00m,
                    IsRequired = true,
                    Description = "Summer student activities fee",
                    DueDate = DateTime.Parse("2025-06-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = summerStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Technology Fee").Id,
                    Amount = 100.00m,
                    IsRequired = true,
                    Description = "Summer technology access fee",
                    DueDate = DateTime.Parse("2025-06-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = summerStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Health Services Fee").Id,
                    Amount = 60.00m,
                    IsRequired = true,
                    Description = "Summer health services fee",
                    DueDate = DateTime.Parse("2025-06-01")
                }
            };

            foreach (var item in summerItems)
            {
                await _repository.CreateFeeStructureItemAsync(item);
            }

            // Fall 2025 Fee Structure
            var fallStructure = new FeeStructure
            {
                Name = "Standard Fall 2025",
                Description = "Standard fee structure for Fall 2025 semester",
                AcademicYear = "2025",
                Semester = "Fall",
                IsActive = true
            };

            await _repository.CreateFeeStructureAsync(fallStructure);

            // Add fee structure items for Fall
            var fallItems = new List<FeeStructureItem>
            {
                new FeeStructureItem
                {
                    FeeStructureId = fallStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Tuition Fee").Id,
                    Amount = 3000.00m,
                    IsRequired = true,
                    Description = "Fall semester tuition fee",
                    DueDate = DateTime.Parse("2025-09-15")
                },
                new FeeStructureItem
                {
                    FeeStructureId = fallStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Registration Fee").Id,
                    Amount = 150.00m,
                    IsRequired = true,
                    Description = "Fall registration fee",
                    DueDate = DateTime.Parse("2025-08-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = fallStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Library Fee").Id,
                    Amount = 100.00m,
                    IsRequired = true,
                    Description = "Fall library access fee",
                    DueDate = DateTime.Parse("2025-09-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = fallStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Student Activity Fee").Id,
                    Amount = 75.00m,
                    IsRequired = true,
                    Description = "Fall student activities fee",
                    DueDate = DateTime.Parse("2025-09-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = fallStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Technology Fee").Id,
                    Amount = 125.00m,
                    IsRequired = true,
                    Description = "Fall technology access fee",
                    DueDate = DateTime.Parse("2025-09-01")
                },
                new FeeStructureItem
                {
                    FeeStructureId = fallStructure.Id,
                    FeeCategoryId = categories.First(c => c.Name == "Health Services Fee").Id,
                    Amount = 80.00m,
                    IsRequired = true,
                    Description = "Fall health services fee",
                    DueDate = DateTime.Parse("2025-09-01")
                }
            };

            foreach (var item in fallItems)
            {
                await _repository.CreateFeeStructureItemAsync(item);
            }
        }

        private async Task SeedAdditionalFeesAsync()
        {
            var additionalFees = new List<AdditionalFee>
            {
                new AdditionalFee
                {
                    Name = "Science Lab Equipment Fee",
                    Description = "Additional fee for science laboratory equipment and materials",
                    Amount = 200.00m,
                    Frequency = FeeFrequency.OneTime,
                    ApplicableTo = FeeApplicability.Program,
                    ApplicablePrograms = JsonSerializer.Serialize(new List<string> { "Computer Science", "Engineering", "Biology" }),
                    StartDate = DateTime.Parse("2025-06-01"),
                    EndDate = DateTime.Parse("2025-12-31"),
                    IsActive = true,
                    CreatedBy = "System"
                },
                new AdditionalFee
                {
                    Name = "Sports Tournament Fee",
                    Description = "Participation fee for inter-university sports tournament",
                    Amount = 50.00m,
                    Frequency = FeeFrequency.OneTime,
                    ApplicableTo = FeeApplicability.All,
                    StartDate = DateTime.Parse("2025-09-01"),
                    EndDate = DateTime.Parse("2025-10-31"),
                    IsActive = true,
                    CreatedBy = "System"
                },
                new AdditionalFee
                {
                    Name = "Graduation Ceremony Fee",
                    Description = "Fee for graduation ceremony attendance and certificate",
                    Amount = 100.00m,
                    Frequency = FeeFrequency.OneTime,
                    ApplicableTo = FeeApplicability.Individual,
                    ApplicableStudents = JsonSerializer.Serialize(new List<string> { "S660099", "S660066" }),
                    StartDate = DateTime.Parse("2025-11-01"),
                    EndDate = DateTime.Parse("2025-12-31"),
                    IsActive = true,
                    CreatedBy = "System"
                },
                new AdditionalFee
                {
                    Name = "Study Abroad Program Fee",
                    Description = "Administrative fee for study abroad program participation",
                    Amount = 500.00m,
                    Frequency = FeeFrequency.OneTime,
                    ApplicableTo = FeeApplicability.Program,
                    ApplicablePrograms = JsonSerializer.Serialize(new List<string> { "International Business", "Languages" }),
                    StartDate = DateTime.Parse("2025-08-01"),
                    EndDate = DateTime.Parse("2025-12-31"),
                    IsActive = true,
                    CreatedBy = "System"
                },
                new AdditionalFee
                {
                    Name = "Parking Permit Fee",
                    Description = "Annual parking permit for campus parking facilities",
                    Amount = 150.00m,
                    Frequency = FeeFrequency.Recurring,
                    ApplicableTo = FeeApplicability.All,
                    StartDate = DateTime.Parse("2025-09-01"),
                    EndDate = DateTime.Parse("2026-08-31"),
                    IsActive = true,
                    CreatedBy = "System"
                }
            };

            foreach (var fee in additionalFees)
            {
                var existing = await _repository.GetAllAdditionalFeesAsync();
                if (!existing.Any(f => f.Name == fee.Name))
                {
                    await _repository.CreateAdditionalFeeAsync(fee);
                }
            }
        }

        private async Task AssignFeeStructuresToStudentsAsync()
        {
            // Get existing students
            var students = await _repository.GetStudentsByProgramAsync(""); // Get all students
            var summerStructure = await _repository.GetFeeStructuresByAcademicYearAsync("2025", "Summer");
            var fallStructure = await _repository.GetFeeStructuresByAcademicYearAsync("2025", "Fall");

            if (!summerStructure.Any() || !fallStructure.Any()) return;

            var summerStructureId = summerStructure.First().Id;
            var fallStructureId = fallStructure.First().Id;

            foreach (var student in students)
            {
                try
                {
                    // Assign Summer 2025 structure
                    var summerAssignment = new StudentFeeAssignment
                    {
                        StudentNumber = student.StudentNumber,
                        FeeStructureId = summerStructureId,
                        AcademicYear = "2025",
                        Semester = "Summer",
                        AssignedBy = "System"
                    };

                    var exists = await _repository.ExistsStudentFeeAssignmentAsync(
                        student.StudentNumber, summerStructureId, "2025", "Summer");
                    
                    if (!exists)
                    {
                        await _repository.CreateStudentFeeAssignmentAsync(summerAssignment);
                    }

                    // Assign Fall 2025 structure
                    var fallAssignment = new StudentFeeAssignment
                    {
                        StudentNumber = student.StudentNumber,
                        FeeStructureId = fallStructureId,
                        AcademicYear = "2025",
                        Semester = "Fall",
                        AssignedBy = "System"
                    };

                    exists = await _repository.ExistsStudentFeeAssignmentAsync(
                        student.StudentNumber, fallStructureId, "2025", "Fall");
                    
                    if (!exists)
                    {
                        await _repository.CreateStudentFeeAssignmentAsync(fallAssignment);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error assigning fee structure to student {student.StudentNumber}: {ex.Message}");
                }
            }
        }

        public async Task<bool> ApplyAdditionalFeesToStudentsAsync()
        {
            try
            {
                var additionalFees = await _repository.GetAllAdditionalFeesAsync();
                
                foreach (var fee in additionalFees.Where(f => f.IsActive))
                {
                    await ApplyAdditionalFeeToStudentsAsync(fee);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying additional fees: {ex.Message}");
                return false;
            }
        }

        private async Task ApplyAdditionalFeeToStudentsAsync(AdditionalFee fee)
        {
            List<Student> applicableStudents = new List<Student>();

            switch (fee.ApplicableTo)
            {
                case FeeApplicability.All:
                    applicableStudents = await _repository.GetStudentsByProgramAsync(""); // Get all students
                    break;
                case FeeApplicability.Program:
                    if (!string.IsNullOrEmpty(fee.ApplicablePrograms))
                    {
                        var programs = JsonSerializer.Deserialize<List<string>>(fee.ApplicablePrograms);
                        foreach (var program in programs)
                        {
                            var students = await _repository.GetStudentsByProgramAsync(program);
                            applicableStudents.AddRange(students);
                        }
                    }
                    break;
                case FeeApplicability.Individual:
                    if (!string.IsNullOrEmpty(fee.ApplicableStudents))
                    {
                        var studentNumbers = JsonSerializer.Deserialize<List<string>>(fee.ApplicableStudents);
                        applicableStudents = await _repository.GetStudentsByNumbersAsync(studentNumbers);
                    }
                    break;
            }

            foreach (var student in applicableStudents.Distinct())
            {
                try
                {
                    var existing = await _repository.GetStudentAdditionalFeeByStudentAndFeeAsync(student.StudentNumber, fee.Id);
                    if (existing == null)
                    {
                        var studentAdditionalFee = new StudentAdditionalFee
                        {
                            StudentNumber = student.StudentNumber,
                            AdditionalFeeId = fee.Id,
                            Amount = fee.Amount,
                            DueDate = fee.StartDate ?? DateTime.UtcNow.AddDays(30),
                            Status = FeeBalanceStatus.Outstanding
                        };

                        await _repository.CreateStudentAdditionalFeeAsync(studentAdditionalFee);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying additional fee {fee.Name} to student {student.StudentNumber}: {ex.Message}");
                }
            }
        }
    }
} 