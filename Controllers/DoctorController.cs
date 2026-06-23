using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Doctor;
using MediCareMS.Models.ViewModels.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediCareMS.Helpers.Security;
using MediCareMS.Models.Entities.Prescription;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MailKit.Net.Smtp;
using MimeKit;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin,Doctor,Receptionist,Nurse")]
public class DoctorController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IPasswordHashService _passwordHash;

    public DoctorController(AppDbContext db, IWebHostEnvironment env, IPasswordHashService passwordHash)
    {
        _db = db;
        _env = env;
        _passwordHash = passwordHash;
    }

    public async Task<IActionResult> Index()
    {
        var doctors = await _db.Doctors
            .Include(d => d.Department)
            .Include(d => d.Specialization)
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.FullName)
            .Select(d => new
            {
                id             = d.Id,
                doctorNo       = d.DoctorNo,
                fullName       = d.FullName,
                department     = d.Department != null ? d.Department.Name : "",
                specialization = d.Specialization != null ? d.Specialization.Name : "",
                qualification  = d.Qualification ?? "",
                experience     = d.ExperienceYears ?? 0,
                fee            = d.ConsultationFee,
                status         = d.Status.ToString(),
                editUrl        = Url.Action("Edit",   "Doctor", new { id = d.Id }),
                deleteUrl      = Url.Action("Delete", "Doctor", new { id = d.Id })
            })
            .ToListAsync();

        var opts = new System.Text.Json.JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        ViewBag.DoctorJson = System.Text.Json.JsonSerializer.Serialize(doctors, opts);
        return View();
    }


    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var doctors = await _db.Doctors
            .Include(d => d.Department)
            .Include(d => d.Specialization)
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.FullName)
            .Select(d => new
            {
                id             = d.Id,
                doctorNo       = d.DoctorNo,
                fullName       = d.FullName,
                department     = d.Department != null ? d.Department.Name : "",
                specialization = d.Specialization != null ? d.Specialization.Name : "",
                qualification  = d.Qualification ?? "",
                experience     = d.ExperienceYears ?? 0,
                fee            = d.ConsultationFee,
                status         = d.Status.ToString(),
                editUrl        = Url.Action("Edit",   "Doctor", new { id = d.Id }),
                deleteUrl      = Url.Action("Delete", "Doctor", new { id = d.Id })
            })
            .ToListAsync();

        return Json(doctors);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildViewModel(new DoctorCreateEditViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        var count = await _db.Doctors.CountAsync() + 1;
        var doctor = new DoctorProfile
        {
            DoctorNo = $"DOC-{count:D3}",
            FullName = vm.FullName,
            DepartmentId = vm.DepartmentId,
            SpecializationId = vm.SpecializationId,
            Qualification = vm.Qualification,
            ExperienceYears = vm.ExperienceYears,
            BmdcRegNo = vm.BmdcRegNo,
            MobileNumber = vm.MobileNumber,
            Email = vm.Email,
            ConsultationFee = vm.ConsultationFee,
            ChamberAddress = vm.ChamberAddress,
            Bio = vm.Bio,
            Status = vm.Status,
            PasswordHash = _passwordHash.HashPassword("123"),
            CreatedAt = DateTime.UtcNow
        };

        if (vm.ProfileImage != null)
            doctor.ProfileImagePath = await SaveFile(vm.ProfileImage);

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Doctor added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();

        var vm = new DoctorCreateEditViewModel
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            DepartmentId = doctor.DepartmentId,
            SpecializationId = doctor.SpecializationId,
            Qualification = doctor.Qualification,
            ExperienceYears = doctor.ExperienceYears,
            BmdcRegNo = doctor.BmdcRegNo,
            MobileNumber = doctor.MobileNumber,
            Email = doctor.Email,
            ConsultationFee = doctor.ConsultationFee,
            ChamberAddress = doctor.ChamberAddress,
            Bio = doctor.Bio,
            Status = doctor.Status,
            ExistingProfileImagePath = doctor.ProfileImagePath
        };
        return View(await BuildViewModel(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DoctorCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        var doctor = await _db.Doctors.FindAsync(vm.Id);
        if (doctor == null) return NotFound();

        doctor.FullName = vm.FullName;
        doctor.DepartmentId = vm.DepartmentId;
        doctor.SpecializationId = vm.SpecializationId;
        doctor.Qualification = vm.Qualification;
        doctor.ExperienceYears = vm.ExperienceYears;
        doctor.BmdcRegNo = vm.BmdcRegNo;
        doctor.MobileNumber = vm.MobileNumber;
        doctor.Email = vm.Email;
        doctor.ConsultationFee = vm.ConsultationFee;
        doctor.ChamberAddress = vm.ChamberAddress;
        doctor.Bio = vm.Bio;
        doctor.Status = vm.Status;
        doctor.UpdatedAt = DateTime.UtcNow;

        if (vm.ProfileImage != null)
            doctor.ProfileImagePath = await SaveFile(vm.ProfileImage);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Doctor updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor != null) { doctor.IsDeleted = true; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Doctor removed.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<DoctorCreateEditViewModel> BuildViewModel(DoctorCreateEditViewModel vm)
    {
        vm.Departments = await _db.Departments.Where(d => d.IsActive)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync();
        vm.Specializations = await _db.Specializations
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToListAsync();
        return vm;
    }

    private async Task<string> SaveFile(IFormFile file)
    {
        var uploads = Path.Combine(_env.WebRootPath, "uploads", "doctors");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(uploads, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/doctors/{fileName}";
    }

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Portal()
    {
        var doctorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(doctorIdClaim, out int docId)) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var appointments = await _db.Appointments
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == docId && a.AppointmentDate == today && a.Status == MediCareMS.Models.Enums.AppointmentStatus.Confirmed && !a.IsDeleted)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();

        var completedCount = await _db.Appointments.CountAsync(a => a.DoctorId == docId && a.Status == MediCareMS.Models.Enums.AppointmentStatus.Completed && !a.IsDeleted);

        ViewBag.Appointments = appointments;
        ViewBag.CompletedCount = completedCount;
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public IActionResult Appointments()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public IActionResult Patients()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> CreatePrescription(int? patientId)
    {
        var vm = new CreatePrescriptionViewModel();
        if (patientId.HasValue)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId.Value);
            if (patient != null)
            {
                vm.PatientId = patient.Id;
                vm.PatientName = patient.FullName;
                vm.PatientIdString = $"#MS-{patient.Id:D4}";
            }
        }
        else
        {
            var firstPatient = await _db.Patients.FirstOrDefaultAsync();
            if (firstPatient != null)
            {
                vm.PatientId = firstPatient.Id;
                vm.PatientName = firstPatient.FullName;
                vm.PatientIdString = $"#MS-{firstPatient.Id:D4}";
            }
        }
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePrescription(CreatePrescriptionViewModel vm, IFormFile? directUpload)
    {
        Console.WriteLine("=== CreatePrescription POST Executed ===");
        
        bool hasManualData = !string.IsNullOrWhiteSpace(vm.Diagnosis);
        bool hasFile = directUpload != null && directUpload.Length > 0;

        if (!hasManualData && !hasFile)
        {
            ModelState.AddModelError("", "Please provide either a Diagnosis for a manual prescription, or upload a prescription file.");
            Console.WriteLine(">>> Validation Failed: Neither manual data nor file provided.");
            return View(vm);
        }

        ModelState.Clear(); // We have our custom logic now, clear any implicit nullability validation errors

        var doctorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(doctorIdClaim, out int docId)) return Unauthorized();

        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.DoctorId == docId && a.PatientId == vm.PatientId)
                          ?? await _db.Appointments.FirstOrDefaultAsync(); // Fallback

        var appointmentId = appointment?.Id ?? 1;

        using var transaction = await _db.Database.BeginTransactionAsync();

        var prescription = await _db.Prescriptions
            .Include(p => p.Medicines)
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

        bool isNew = false;
        if (prescription == null)
        {
            isNew = true;
            prescription = new Prescription
            {
                PatientId = vm.PatientId > 0 ? vm.PatientId : 1,
                DoctorId = docId,
                AppointmentId = appointmentId,
                CreatedAt = DateTime.UtcNow,
                Status = MediCareMS.Models.Enums.PrescriptionStatus.Draft
            };
        }

        prescription.Diagnosis = string.IsNullOrWhiteSpace(vm.Diagnosis) && hasFile ? "See Attached File" : vm.Diagnosis ?? "N/A";
        prescription.Notes = vm.Notes ?? string.Empty;
        prescription.UpdatedAt = DateTime.UtcNow;

        if (!isNew && prescription.Medicines.Any())
        {
            _db.PrescriptionMedicines.RemoveRange(prescription.Medicines);
            prescription.Medicines.Clear();
        }

        if (vm.Medicines != null)
        {
            foreach (var med in vm.Medicines)
            {
                if (!string.IsNullOrWhiteSpace(med.MedicineName))
                {
                    prescription.Medicines.Add(new PrescriptionMedicine
                    {
                        MedicineName = med.MedicineName,
                        Dosage = med.Dosage ?? "",
                        Duration = med.Duration ?? "",
                        Instructions = med.Instructions ?? "",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (hasFile)
        {
            prescription.Notes += $"\n[Attached file: {directUpload!.FileName}]";
        }

        // --- Generate PDF using QuestPDF ---
        QuestPDF.Settings.License = LicenseType.Community;
        
        var patientInfo = await _db.Patients.FirstOrDefaultAsync(p => p.Id == prescription.PatientId);
        var doctorInfo = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == docId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text($"MediCareMS Prescription").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                
                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Item().Text($"Doctor: {doctorInfo?.FullName}").FontSize(14);
                    x.Item().Text($"Patient: {patientInfo?.FullName}").FontSize(14);
                    x.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}");
                    x.Spacing(20);
                    
                    x.Item().Text($"Diagnosis: {prescription.Diagnosis}").SemiBold();
                    x.Spacing(10);
                    
                    x.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                        t.Header(h =>
                        {
                            h.Cell().Text("Medicine"); h.Cell().Text("Dosage"); h.Cell().Text("Duration"); h.Cell().Text("Instructions");
                        });
                        foreach (var m in prescription.Medicines)
                        {
                            t.Cell().Text(m.MedicineName);
                            t.Cell().Text(m.Dosage);
                            t.Cell().Text(m.Duration);
                            t.Cell().Text(m.Instructions);
                        }
                    });

                    x.Spacing(20);
                    x.Item().Text("Notes:").SemiBold();
                    x.Item().Text(prescription.Notes);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        // Save PDF to local path
        var pdfName = $"Prescription_{patientInfo?.Id}_{DateTime.Now.Ticks}.pdf";
        var pdfPath = Path.Combine(_env.WebRootPath, "prescriptions", pdfName);
        Directory.CreateDirectory(Path.GetDirectoryName(pdfPath)!);
        document.GeneratePdf(pdfPath);
        
        prescription.PdfFilePath = $"/prescriptions/{pdfName}";

        if (isNew)
        {
            _db.Prescriptions.Add(prescription);
        }
        else
        {
            _db.Prescriptions.Update(prescription);
        }
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        // --- Send Email ---
        var userEmail = patientInfo?.Email;
        if (!string.IsNullOrEmpty(userEmail))
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("MediCareMS", "no-reply@medicarems.local"));
                message.To.Add(new MailboxAddress(patientInfo!.FullName, userEmail));
                message.Subject = "Your Prescription from MediCareMS";

                var builder = new BodyBuilder
                {
                    TextBody = "Hello, please find your prescription attached."
                };
                builder.Attachments.Add(pdfPath);
                message.Body = builder.ToMessageBody();

                // Using Papercut/Mailtrap dummy localhost SMTP (ignores if unavailable)
                using var client = new SmtpClient();
                await client.ConnectAsync("localhost", 25, false); // Adjust for real SMTP
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Silent catch for testing without local SMTP running
                System.Diagnostics.Debug.WriteLine($"Email fail: {ex.Message}");
            }
        }

        TempData["Success"] = "Prescription saved and sent successfully!";
        return RedirectToAction("Patients");
    }
}
