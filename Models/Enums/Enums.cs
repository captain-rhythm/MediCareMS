namespace MediCareMS.Models.Enums;

public enum AccountStatus { Active = 1, Inactive = 0, Suspended = 2 }
public enum AppointmentStatus { Pending = 0, Confirmed = 1, Completed = 2, Cancelled = 3, NoShow = 4 }
public enum PaymentStatus { Unpaid = 0, Paid = 1, Partial = 2, Refunded = 3, Cancelled = 4 }
public enum PaymentMethod { Cash = 0, Card = 1, Online = 2, Insurance = 3 }
public enum SSLCommerzStatus { Pending = 0, Success = 1, Failed = 2, Cancelled = 3, Invalid = 4 }
public enum Gender { Male, Female, Other }
public enum BloodGroup { Unknown = 0, A_Positive, A_Negative, B_Positive, B_Negative, AB_Positive, AB_Negative, O_Positive, O_Negative }
public enum DayOfWeekEnum { Sunday = 0, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday }
public enum DoctorStatus { Available = 0, OnLeave = 1, Inactive = 2 }
public enum TestReportStatus { Requested = 0, InProgress = 1, Completed = 2 }
public enum PrescriptionStatus { Draft = 0, Finalized = 1 }
