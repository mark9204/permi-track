# PermiTrack

**Enterprise Permission & Access Management System**

> 🎓 **University Project** - This is an educational project developed as part of our university studies to learn modern software development practices, security principles, and enterprise application architecture.

A modern, scalable solution for managing organizational permissions, user access rights, and role-based authorization with comprehensive audit trails and approval workflows.

## 🎯 Project Overview

PermiTrack is a learning-focused implementation of an enterprise-grade permission management system. Through this project, we're exploring how medium to large enterprises handle transparent, scalable, and auditable management of employee and system permissions. The system provides a centralized platform for handling access requests, role management, and security compliance.

### Learning Objectives

- Understanding enterprise authentication and authorization patterns
- Implementing role-based access control (RBAC)
- Building RESTful APIs with proper documentation
- Working with Entity Framework Core and database migrations
- Creating approval workflows and audit systems
- Developing modern React applications with TypeScript
- Applying clean architecture principles
- Team collaboration using Git and GitHub

## ✨ Key Features

### 🔐 Authentication & User Management
- Multi-channel authentication (OAuth2, JWT, ASP.NET Identity)
- Role-based access control (RBAC): Administrator, User, Guest
- Two-factor authentication (2FA)
- Password recovery and email verification

### 👥 Permission & Role Management
- Dynamic role creation, modification, and deletion
- Granular permissions at API endpoint, operation, and data scope levels
- Automatic permission inheritance
- Centralized permission model for scalability

### 📋 Approval Workflow
- Multi-level approval chains (e.g., manager + system administrator)
- User-initiated permission requests
- Real-time notifications for status changes
- Complete audit trail of all requests and approvals

### 📊 Audit & Compliance
- Comprehensive logging of all permission, role, and user changes
- Searchable audit logs (by time, person, object)
- Incident management reporting
- Exportable logs for external security systems

### 🔌 Integration & API
- RESTful API with Swagger/OpenAPI documentation
- Microservices-friendly architecture
- External system integration support (Intranet, Document Management, HR systems)
- Modular and extensible API design

### 💻 Administrative Interface
- Modern React-based management UI with Material-UI
- Real-time permission and user overview
- Role-based UI rendering
- Statistical dashboard for system usage monitoring

## 🛠️ Technology Stack

### Backend
- **.NET Core 8.0** - ASP.NET Core Web API
- **Entity Framework Core** - ORM and database management
- **SQL Server / PostgreSQL** - Database options
- **JWT & OAuth2** - Authentication and authorization

### Frontend
- **React** with TypeScript
- **Material-UI (MUI)** - Component library
- **Vite** - Build tool
- **React Router** - Navigation

### Architecture
- Clean Architecture principles
- Repository pattern
- Dependency injection
- Background job processing
- Custom authorization policies

## 📁 Project Structure

permi-track/
├── PermiTrack/ # Main Web API project
│ ├── Controllers/ # API endpoints
│ ├── Authorization/ # Custom authorization handlers
│ ├── Middleware/ # Custom middleware components
│ ├── BackgroundJobs/ # Background task processing
│ └── Extensions/ # Service extensions
├── PermiTrack.DataContext/ # Data access layer
│ ├── Entities/ # Domain models
│ ├── DTOs/ # Data transfer objects
│ ├── Enums/ # Enumerations
│ ├── Mappings/ # Entity configurations
│ └── Migrations/ # Database migrations
├── PermiTrack.Services/ # Business logic layer
└── PermiTrack.Frontend/ # React frontend application
├── src/ # Source files
└── public/ # Static assets


## 🔒 Security

This project implements various security best practices as part of our learning process:

- All sensitive data is encrypted
- HTTPS enforced in production
- JWT tokens with configurable expiration
- CORS policies configured
- SQL injection protection via Entity Framework
- XSS protection implemented

> ⚠️ **Note**: This is a student project and should not be used in production environments without thorough security review and testing.

## 📖 Project Status

This project is actively under development as part of our university coursework. We're continuously learning and improving the codebase.

## 👥 Team Members

- [**mark9204**](https://github.com/mark9204)
- [**Barnaa77**](https://github.com/Barnaa77)
- [**PekBencee**](https://github.com/PekBencee)

## 🎓 Academic Context

This project is being developed as part of our software engineering studies. It serves as a practical application of concepts learned in courses including:
- Software Architecture
- Web Development
- Database Systems
- Security and Authentication
- Team Software Development

## 📄 License

This is an educational project developed for university coursework. Please contact the team members for any usage inquiries.

## 🤝 Contributing

As this is a university project, contributions are currently limited to team members. However, we welcome feedback and suggestions through GitHub issues!

## 📧 Contact

For questions about this project, please open an issue on GitHub or contact the team members directly.

---

🎓 Developed with dedication by students learning enterprise software development
