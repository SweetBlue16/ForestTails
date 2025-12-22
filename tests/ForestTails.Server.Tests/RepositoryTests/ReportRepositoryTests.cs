using FluentAssertions;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.RepositoryTests
{
    public class ReportRepositoryTests : IDisposable
    {
        private readonly TestDbFactory dbFactory;
        private readonly ReportRepository reportRepository;

        public ReportRepositoryTests()
        {
            dbFactory = new TestDbFactory();
            var loggerMock = new Mock<ILogger<ReportRepository>>();
            reportRepository = new ReportRepository(dbFactory, loggerMock.Object);
        }

        [Fact]
        public async Task CreateReportAsyncTestShouldSaveReport()
        {
            await CreateUsersAsync();
            var report = new Report
            {
                ReporterUserId = 1,
                ReportedUserId = 2,
                Description = "Test Report"
            };
            await reportRepository.CreateReportAsync(report);
            using var context = dbFactory.CreateDbContext();
            var saved = await context.Reports.FirstAsync();
            saved.Description.Should().Be("Test Report");
            saved.ReporterUserId.Should().Be(1);
        }

        [Fact]
        public async Task GetPendingReportsAsyncTestShouldReturnOnlyUnresolved()
        {
            await CreateUsersAsync();
            await SeedReport(1, resolved: false);
            await SeedReport(2, resolved: true);
            var pending = await reportRepository.GetPendingReportsAsync();
            pending.Should().HaveCount(1);
            pending.First().Id.Should().Be(1);
        }

        [Fact]
        public async Task GetPendingReportsAsyncTestShouldIncludeUserData()
        {
            await CreateUsersAsync();
            using (var context = dbFactory.CreateDbContext())
            {
                context.Reports.Add(new Report
                {
                    ReporterUserId = 1,
                    ReportedUserId = 2,
                    IsResolved = false,
                    Description = "Include Test",
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
            var pending = await reportRepository.GetPendingReportsAsync();
            var report = pending.First();
            report.Reporter?.Username.Should().Be("Reporter");
            report.Reported?.Username.Should().Be("Reported");
        }

        [Fact]
        public async Task MarkAsResolvedAsyncTestShouldUpdateStatus()
        {
            await CreateUsersAsync();
            await SeedReport(10, resolved: false);
            await reportRepository.MarkAsResolvedAsync(10);
            using var context = dbFactory.CreateDbContext();
            var report = await context.Reports.FindAsync(10);
            report!.IsResolved.Should().BeTrue();
        }

        [Fact]
        public async Task MarkAsResolvedAsyncTestShouldDoNothingIfReportNotFound()
        {
            Func<Task> act = async () => await reportRepository.MarkAsResolvedAsync(999);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetPendingReportsAsyncTestShouldReturnOrderedByDate()
        {
            await CreateUsersAsync();
            using (var context = dbFactory.CreateDbContext())
            {
                context.Reports.Add(new Report 
                { 
                    Description = "Newer", 
                    CreatedAt = DateTime.Now.AddDays(1), 
                    IsResolved = false, 
                    ReporterUserId = 1, 
                    ReportedUserId = 2 
                });
                context.Reports.Add(new Report 
                { 
                    Description = "Older", 
                    CreatedAt = DateTime.Now.AddDays(-1), 
                    IsResolved = false, 
                    ReporterUserId = 1, 
                    ReportedUserId = 2 
                });
                await context.SaveChangesAsync();
            }
            var pending = await reportRepository.GetPendingReportsAsync();
            pending.First().Description.Should().Be("Older");
        }

        private async Task SeedReport(int id, bool resolved)
        {
            using var context = dbFactory.CreateDbContext();
            context.Reports.Add(new Report
            {
                Id = id,
                IsResolved = resolved,
                Description = "Seeded",
                CreatedAt = DateTime.UtcNow,
                ReporterUserId = 1,
                ReportedUserId = 2
            });
            await context.SaveChangesAsync();
        }

        private async Task CreateUsersAsync()
        {
            using var context = dbFactory.CreateDbContext();
            if (!await context.Users.AnyAsync(u => u.Id == 1))
            {
                context.Users.Add(new User 
                { 
                    Id = 1, 
                    Username = "Reporter", 
                    Email = "r@r.com", 
                    PasswordHash = "x", 
                    FullName = "x" 
                });
            }

            if (!await context.Users.AnyAsync(u => u.Id == 2))
            {
                context.Users.Add(new User 
                { 
                    Id = 2, 
                    Username = 
                    "Reported", 
                    Email = "d@d.com", 
                    PasswordHash = "x", 
                    FullName = "x" 
                });
            }
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            dbFactory.Dispose();
        }
    }
}
