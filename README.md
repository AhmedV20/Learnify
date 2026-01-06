# Learnify Backend API

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Backend CI](https://github.com/AhmedV20/Learnify/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/AhmedV20/Learnify/actions/workflows/backend-ci.yml)

**Learnify API** is a production-ready backend for an e-learning platform. Built with **.NET 9 Clean Architecture**, it provides RESTful APIs for course management, user authentication, payments, and more.

<p align="center">
<a href="#-features">Features</a> •
<a href="#-tech-stack">Tech Stack</a> •
<a href="#-getting-started">Getting Started</a> •
<a href="#-api-documentation">API Docs</a> •
<a href="docs/ARCHITECTURE.md">Architecture</a>
</p>

---

## 📖 Overview

Learnify is a comprehensive e-learning API featuring:

- **JWT authentication** with refresh tokens and OTP email verification
- **Two-factor authentication** (Email, Authenticator App, Backup Codes)
- **Stripe payment integration** and manual payment workflows
- **Video streaming** via Cloudinary
- **AI-powered chat assistance** via HuggingFace
- **Background job processing** with Hangfire
- **Structured logging** with Serilog and Seq

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    LEARNIFY BACKEND API                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              REST API Layer (/api/v1/*)             │   │
│  │         Controllers • Middleware • Filters          │   │
│  └─────────────────────┬───────────────────────────────┘   │
│                        │                                    │
│  ┌─────────────────────▼───────────────────────────────┐   │
│  │              Application Layer (CQRS)               │   │
│  │       Commands • Queries • DTOs • Validators        │   │
│  └─────────────────────┬───────────────────────────────┘   │
│                        │                                    │
│  ┌─────────────────────▼───────────────────────────────┐   │
│  │             Infrastructure Layer                     │   │
│  │    Repositories • Services • EF Core • Hangfire     │   │
│  └─────────────────────┬───────────────────────────────┘   │
│                        │                                    │
│  ┌─────────────────────▼───────────────────────────────┐   │
│  │                Domain Layer                          │   │
│  │           Entities • Enums • Value Objects           │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│                    External Services                        │
│  [SQL Server] [Redis] [Cloudinary] [Stripe] [Seq] [AI]     │
└─────────────────────────────────────────────────────────────┘
```

---

## ✨ Features

### 🎓 Learning Management
- Course creation with sections & lectures
- Video upload via Cloudinary
- Progress tracking & certificates
- Ratings & reviews

### 👥 User System
- JWT authentication with refresh tokens
- OTP email verification
- Two-factor authentication (Email, Authenticator, Backup Codes)
- Role-based access (Admin, Instructor, Student)
- Google OAuth integration

### 💰 Payments
- Stripe integration for card payments
- Manual payment with proof upload
- Admin payment review workflow
- Instructor payout system
- Coupon management

### 🔧 Enterprise Features
- **Serilog** - Structured logging with Console, File, and Seq sinks
- **Hangfire** - Background job processing with retry policies
- **Email Templates** - Responsive HTML emails with branding
- **Scalar API** - Modern API documentation UI
- **Health Checks** - Database, Redis, Stripe, Cloudinary monitoring

### 🤖 AI Features
- AI chat assistant via HuggingFace
- Smart course recommendations

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| .NET 9.0 | Runtime |
| ASP.NET Core 9.0 | Web framework |
| Entity Framework Core 9.0 | ORM |
| SQL Server | Database |
| Redis | Caching |
| Hangfire | Background jobs |
| Serilog + Seq | Logging |
| MediatR | CQRS pattern |
| FluentValidation | Input validation |
| AutoMapper | Object mapping |
| Stripe | Payment processing |
| Cloudinary | Media hosting |

---

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server (or LocalDB)
- Redis (optional, for caching)

### 1. Clone & Install

```bash
git clone https://github.com/yourusername/Learnify.git
cd Learnify/src/Learnify.Api
dotnet restore
```

### 2. Configure

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LearnifyDb;Trusted_Connection=True;"
  },
  "JWT": {
    "Secret": "your-super-secret-key-min-32-chars",
    "ValidIssuer": "Learnify",
    "ValidAudience": "Learnify"
  },
  "App": {
    "BaseUrl": "http://localhost:5279"
  }
}
```

### 3. Run Database Migrations

```bash
dotnet ef database update --project ../Learnify.Infrastructure
```

### 4. Start the API

```bash
dotnet run
```

| Endpoint | URL |
|----------|-----|
| API Base | http://localhost:5279 |
| Scalar API Docs | http://localhost:5279/scalar/v1 |
| Hangfire Dashboard | http://localhost:5279/hangfire |
| Health Check | http://localhost:5279/health |

---

## 📚 API Documentation

Interactive API documentation available at `/scalar/v1` when running.

### Key Endpoints

```
Authentication
POST /api/users/register          # Register new user
POST /api/users/login             # Login
POST /api/users/verify-otp        # Verify email OTP
POST /api/users/refresh-token     # Refresh JWT token

Two-Factor Auth
POST /api/two-factor/setup        # Setup 2FA
POST /api/two-factor/verify       # Verify 2FA code
POST /api/two-factor/disable      # Disable 2FA

Courses
GET  /api/courses                 # List courses
POST /api/courses                 # Create course (Instructor)
GET  /api/courses/{id}            # Get course details

Payments
POST /api/payments/checkout       # Create Stripe session
POST /api/manual-payments         # Submit manual payment

Admin
GET  /api/admin/reports           # Analytics data
GET  /api/admin/payments/pending  # Pending approvals
```

---

## 📁 Project Structure

```
src/
├── Learnify.Api/                 # Presentation Layer
│   ├── Controllers/              # 25 API controllers
│   ├── Extensions/               # Hangfire, Serilog config
│   ├── Health/                   # Health check implementations
│   ├── Logging/                  # Middleware & enrichers
│   ├── Middleware/               # Exception handling
│   └── RateLimiting/             # Rate limit policies
│
├── Learnify.Application/         # Business Logic Layer
│   ├── Users/                    # User commands/queries
│   ├── Courses/                  # Course management
│   ├── Enrollments/              # Enrollment logic
│   ├── Payments/                 # Payment processing
│   ├── BackgroundJobs/           # Job interfaces
│   ├── Common/                   # Shared DTOs, behaviors
│   └── Mappings/                 # AutoMapper profiles
│
├── Learnify.Infrastructure/      # External Concerns
│   ├── Data/                     # EF Core DbContext, migrations
│   ├── Repositories/             # Data access implementations
│   ├── Services/                 # External service integrations
│   ├── BackgroundJobs/           # Hangfire job implementations
│   └── Seed/                     # Database seeding
│
└── Learnify.Domain/              # Core Domain
    ├── Entities/                 # 23 domain entities
    └── Enums/                    # Domain enumerations
```

---

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'feat: add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines and [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md) for branching strategy.

---

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.

---

<div align="center">

**Built with ❤️ using .NET 9**

</div>