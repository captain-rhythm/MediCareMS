# MediCareMS - Detailed Project Documentation

## Introduction

This is a comprehensive medical project named **MediCareMS** (MediCare Management System). It is a robust, end-to-end Hospital and Clinic Management System designed to digitize and streamline hospital operations. 

Built using the modern **ASP.NET Core 8 MVC** framework, the project serves multiple types of users including **Super Admins, Doctors, Receptionists, and Patients**. It handles everything from patient registration and doctor scheduling to appointment booking, billing, and even an AI-powered chat system.

---

## Detailed Features of MediCareMS

The project is packed with a wide array of features divided by the different user roles and modules:

### 1. Authentication & Security Module
- **Role-Based Access Control (RBAC):** Users are strictly divided into roles (`Super Admin`, `Doctor`, `Receptionist`, `Patient`). Each role has specific permissions and sees a different dashboard.
- **Secure Login & Registration:** Uses strong password hashing (PBKDF2 with SHA256) to keep user data safe.
- **Google OAuth Integration:** Users can log in quickly using their Google accounts.
- **Audit Logging:** The system automatically logs critical actions taken by users (for tracking and security compliance).

### 2. Patient Portal Features
- **Self-Registration:** Patients can create their own accounts.
- **Profile Management:** Patients can update their blood group, emergency contacts, and personal details.
- **Appointment Booking:** Patients can browse doctors by department, view their available time slots, and book appointments.
- **Prescription & History Viewing:** Patients have access to their past appointments, digital prescriptions, and generated invoices.

### 3. Doctor Management Module
- **Doctor Profiles:** Detailed profiles for doctors including their specialization, department, and contact info.
- **Dynamic Scheduling:** Doctors or Admins can set specific availability schedules (e.g., available Mondays 10 AM to 2 PM and Wednesdays 4 PM to 8 PM).
- **Patient Management:** Doctors can view their upcoming appointments, mark them as completed, and generate digital prescriptions for patients.

### 4. Appointment & Token System
- **Automated Tokens:** When an appointment is booked, a unique token/ticket number is generated for tracking.
- **QR Code Generation:** Appointments generate QR codes that can be scanned for quick verification at the reception.
- **Lifecycle Tracking:** Appointments move through a lifecycle: `Pending` (just booked) → `Confirmed` (approved by reception/admin) → `Completed` (doctor finished the checkup) or `Cancelled`.

### 5. Billing & Payment Gateway
- **Invoice Generation:** Automatically generates detailed invoices for appointments and treatments.
- **Online Payments:** Integrated with the **SSLCommerz** payment gateway, allowing patients to pay their bills online securely using credit cards, debit cards, or mobile banking.

### 6. AI Agent & Real-Time Chat
- **Real-Time Messaging:** Built-in chat system using SignalR so patients and hospital staff can communicate instantly.
- **AI Chatbot Agent:** An integrated AI assistant (powered by Groq) that can help answer basic medical queries or guide users on how to use the hospital system.

### 7. Admin & Reporting Dashboard
- **Hospital Statistics:** A dashboard showing total registered patients, total doctors, and revenue charts.
- **Department Management:** Admins can add, edit, or remove hospital departments (e.g., Cardiology, Neurology).
- **Staff Management:** Admins can register new doctors or receptionists and manage their access.

---

## How This Project Actually Works

Here is a step-by-step breakdown of how the project functions behind the scenes and from a user's perspective:

### The Technology Under the Hood
1. **The Core Framework:** The project runs on **ASP.NET Core 8 MVC**. This means it uses **Models** (to represent database tables like `User` or `Appointment`), **Views** (the HTML/CSS web pages the user sees), and **Controllers** (the brain that takes user input, talks to the database, and decides what page to show).
2. **The Database:** It uses **Entity Framework Core (EF Core)** to talk to the database. By default, it uses **SQLite** for easy local setup, but it is fully ready to use **PostgreSQL** in a production environment.
3. **The Frontend:** The user interface is built using standard HTML (Razor pages `.cshtml`), styled with modern **Vanilla CSS** featuring glassmorphism effects, and uses **Font Awesome** for icons.

### Step-by-Step System Workflow

#### Scenario: A Patient Books an Appointment
1. **Registration/Login:** The patient visits the website and signs up. The `AuthController` securely hashes their password and saves their data to the database.
2. **Finding a Doctor:** The patient navigates to the booking page. The `DoctorController` and `DepartmentController` fetch a list of available doctors from the database and display them on the View.
3. **Booking the Slot:** The patient selects a time slot. The `AppointmentController` checks if the slot is still free. If it is, it creates an `Appointment` record in the database, generates a unique Token, and assigns a `Pending` status.
4. **Payment (Optional at Booking):** If online payment is required, the `PaymentController` redirects the user to the SSLCommerz gateway. Once paid, the system verifies the transaction and updates the appointment status.
5. **Confirmation:** The Receptionist or Admin sees the new appointment on their dashboard and updates the status to `Confirmed`. The system might send an automated email to the patient using the `EmailService`.
6. **The Checkup:** The patient visits the hospital. The doctor logs into their portal, sees the patient on their schedule, conducts the checkup, and marks the appointment as `Completed`. They can then type out a digital prescription which is saved to the patient's record.

### Key Internal Mechanisms
- **Dependency Injection (DI):** The project relies heavily on DI. Services like `IEmailService` (for sending emails), `IQRCodeService` (for generating QR codes), and `IAIService` (for the chatbot) are injected into the controllers only when needed, making the app fast and memory-efficient.
- **SignalR (Real-Time Web):** Instead of making the user refresh the page to see new chat messages, the project uses SignalR Hubs (`ChatHub`). This creates a persistent, two-way connection between the server and the browser, so messages appear instantly.
- **Data Protection & Middleware:** The project uses ASP.NET Data Protection to secure cookies and sessions. It also has a custom `AuditLoggingMiddleware` that sits in the pipeline—every time a user clicks a button or loads a page, this middleware silently logs who did what and when, ensuring total accountability in the hospital system.
