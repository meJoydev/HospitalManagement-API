# Hospital Appointment & Patient Management API

A production-grade REST API built with **ASP.NET Core 8** using 
**Clean Architecture**, JWT authentication, and Entity Framework Core.

## Tech Stack
- ASP.NET Core 8 Web API
- Entity Framework Core 8 + SQL Server
- JWT Authentication + Role-Based Authorization (Admin, Doctor, Patient)
- Clean Architecture (Domain, Application, Infrastructure, API layers)
- Repository Pattern + Dependency Injection
- xUnit + Moq (Unit Tests)
- Swagger / OpenAPI documentation

## Features
- Multi-role JWT authentication (Admin, Doctor, Patient)
- Appointment booking with double-booking prevention
- Appointment state machine (Requested → Confirmed → Completed/Cancelled)
- EF Core RowVersion concurrency token
- Audit logging for every status change
- Pagination on all list endpoints
- Global exception handling middleware
- Background Hosted Service for notifications

## Project Structure
HospitalManagement/
├── HospitalManagement.Domain/        ← Entities, Enums
├── HospitalManagement.Application/   ← Interfaces, DTOs, Services
├── HospitalManagement.Infrastructure/← DbContext, Repositories, EF Core
└── HospitalManagement.API/           ← Controllers, Middleware, Program.cs

## How to Run Locally
1. Clone the repository
   git clone https://github.com/meJoydev/HospitalManagement-API.git

2. Open HospitalManagement.sln in Visual Studio 2022

3. Update appsettings.json connection string if needed

4. Open Package Manager Console and run:
   Update-Database -StartupProject HospitalManagement.API

5. Press Ctrl+F5 to run

6. Swagger opens at http://localhost:5009

## API Endpoints

| Method | URL | Role | Description |
|--------|-----|------|-------------|
| POST | /api/auth/register | Public | Register as Admin/Doctor/Patient |
| POST | /api/auth/login | Public | Login and get JWT token |
| GET | /api/doctor | Any auth user | List all doctors |
| GET | /api/doctor/{id} | Any auth user | Get doctor details |
| GET | /api/doctor/{id}/schedule | Any auth user | Get doctor schedule |
| POST | /api/doctor/{id}/schedule | Admin | Add doctor schedule |
| GET | /api/patient | Admin | List all patients |
| GET | /api/patient/me | Patient | My profile |
| GET | /api/appointment | Admin | All appointments (paginated) |
| GET | /api/appointment/my | Patient | My appointments |
| GET | /api/appointment/doctor | Doctor | My patients' appointments |
| POST | /api/appointment | Patient | Book appointment |
| PUT | /api/appointment/{id}/status | Doctor/Admin | Update appointment status |
