# MediCare - Online Hospital Appointment Booking System

MediCare is a modern, responsive Hospital Management System (HMS) built with **ASP.NET Core 8**. It provides a comprehensive solution for managing hospital operations, including doctor schedules, patient registration, and appointment booking.

## 🚀 Key Features

### 🔐 Secure Authentication & Authorization
- Role-based access control (RBAC) with roles for **Super Admin**, **Doctor**, **Receptionist**, and **Patient**.
- Secure password hashing using PBKDF2 with SHA256.
- Persistent session management with "Remember Me" functionality.

### 📊 Admin Dashboard
- Real-time statistics for total patients, doctors, and appointments.
- Revenue tracking (Daily and Monthly) with SQLite-optimized queries.
- Quick actions for common tasks like adding doctors or booking appointments.

### 👨‍⚕️ Doctor Management
- Management of doctor profiles, specializations, and departments.
- Flexible doctor scheduling across different days of the week.

### 📅 Appointment System
- Smart appointment booking with automated token generation.
- Real-time availability tracking for doctors.
- Comprehensive appointment management (Confirm, Cancel, Complete).

### 👤 Patient Portal
- Self-registration and secure login.
- Profile management (Blood Group, Mobile Number, Emergency Contacts).
- Personal appointment history and digital prescriptions.

## 🛠️ Technical Stack
- **Framework**: ASP.NET Core 8 (MVC)
- **Database**: SQLite (Portable and self-contained)
- **ORM**: Entity Framework Core
- **Styling**: Vanilla CSS with modern aesthetics (Glassmorphism, Vibrant Palettes)
- **Icons**: Font Awesome 6.5.0
- **Typography**: Google Fonts (Inter)

## 🏁 Getting Started

### Prerequisites
- .NET 8 SDK installed on your machine.

### Installation & Run
1. **Clone the repository**:
   ```bash
   git clone https://github.com/captain-rhythm/MediCareMS.git
   ```
2. **Navigate to the project directory**:
   ```bash
   cd MediCareMS
   ```
3. **Run the application**:
   ```bash
   dotnet run
   ```
4. **Access the application**:
   Open your browser and go to `http://localhost:5002`

## 👤 Default Credentials

### Admin Access
- **Email**: `admin@medicare.local`
- **Password**: `Admin@12345`

### Patient Access
- **Email**: `patient@medicare.local`
- **Password**: `Patient@12345`

## 📂 Project Structure
- `Controllers/`: Application logic for Auth, Admin, Users, and more.
- `Models/`: Database entities and view models.
- `Data/`: DB Context and Initializer for seeding data.
- `Views/`: Responsive Razor views for all portals.
- `wwwroot/`: Static assets (CSS, JS, Images).

## 📝 License
This project is for educational purposes.