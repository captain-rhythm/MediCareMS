namespace MediCareMS.Models.ViewModels.Patient;

/// <summary>Parameters accepted by Patient/GetPagedList.</summary>
public class PatientPageRequest
{
    public int    Page       { get; set; } = 1;
    public int    PageSize   { get; set; } = 10;
    public string Search     { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string SortField  { get; set; } = "fullName";
    public string SortDir    { get; set; } = "asc";
}

/// <summary>Single patient row returned by the paged API.</summary>
public class PatientRowDto
{
    public int    Id          { get; set; }
    public string PatientNo   { get; set; } = string.Empty;
    public string FullName    { get; set; } = string.Empty;
    public string Mobile      { get; set; } = string.Empty;
    public string AgeGender   { get; set; } = string.Empty;
    public int    Age         { get; set; }
    public string BloodGroup  { get; set; } = string.Empty;
    public int    TotalVisits { get; set; }
    public string DetailUrl   { get; set; } = string.Empty;
    public string EditUrl     { get; set; } = string.Empty;
    public string DeleteUrl   { get; set; } = string.Empty;
}

/// <summary>Envelope returned by Patient/GetPagedList.</summary>
public class PatientPageResponse
{
    public IEnumerable<PatientRowDto> Data     { get; set; } = Enumerable.Empty<PatientRowDto>();
    public int                        Total    { get; set; }
    public int                        Page     { get; set; }
    public int                        PageSize { get; set; }
}
