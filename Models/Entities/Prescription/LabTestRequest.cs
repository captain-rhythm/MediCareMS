using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Prescription;

public class LabTestRequest
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public TestReportStatus Status { get; set; } = TestReportStatus.Requested;
    public string? ReportFilePath { get; set; }
    public DateTime? ReportUploadedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
