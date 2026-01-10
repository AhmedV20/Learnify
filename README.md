<h1 align="center" id="learnify-backend-api">Learnify Backend API</h1>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9.0" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core" />
  <img src="https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C# 13" />
  <img src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" alt="License: MIT" />
</p>

<p align="center">
  <a href="https://github.com/AhmedV20/Learnify/actions/workflows/backend-ci.yml">
    <img src="https://github.com/AhmedV20/Learnify/actions/workflows/backend-ci.yml/badge.svg" alt="Backend CI" />
  </a>
  <img src="https://img.shields.io/badge/EF_Core-9.0-blueviolet?logo=nuget" alt="EF Core 9" />
  <img src="https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Redis-Caching-DC382D?logo=redis&logoColor=white" alt="Redis" />
  <img src="https://img.shields.io/badge/Stripe-Payments-008CDD?logo=stripe&logoColor=white" alt="Stripe" />
  <img src="https://img.shields.io/badge/Cloudinary-Media-3448C5?logo=cloudinary&logoColor=white" alt="Cloudinary" />
</p>

<p align="center">
  <a href="#-features">Features</a> •
  <a href="#-tech-stack">Tech Stack</a> •
  <a href="#-getting-started">Getting Started</a> •
  <a href="#-api-documentation">API Docs</a> •
  <a href="#-architecture">Architecture</a> •
  <a href="https://github.com/AhmedV20/Learnify">Repository</a>
</p>

---

## 📝 Description

**Learnify API** is a comprehensive, production-ready backend solution for building modern e-learning platforms. Built with **.NET 9** and following **Clean Architecture** principles, it provides a robust, scalable, and maintainable foundation for online education systems.

This API powers a complete learning management system with features including:

- **Multi-tenant user management** with role-based access control (Admin, Instructor, Student)
- **Complete course lifecycle** from creation to certification
- **Dual payment processing** via Stripe and manual payment workflows with admin approval
- **Advanced security** with JWT authentication, refresh tokens, 2FA (Email, Authenticator App, Backup Codes), and Google OAuth
- **Real-time video streaming** and progress tracking with resume functionality
- **AI-powered learning assistance** via HuggingFace integration
- **Enterprise-grade observability** with structured logging, health checks, and performance monitoring

Whether you're building a corporate training platform, an online academy, or a MOOC system, Learnify provides the battle-tested backend infrastructure you need.

---

## 📖 Overview

Learnify is designed as a scalable e-learning platform backend that handles everything from user authentication to payment processing and course delivery. The system is built around these core modules:

### 🔐 Authentication & Security
- **JWT-based authentication** with configurable token expiry and automatic refresh token rotation
- **OTP email verification** for account activation and password reset flows
- **Two-factor authentication** supporting Email codes, Authenticator apps (TOTP), and Backup recovery codes
- **Google OAuth integration** for seamless social login
- **Rate limiting** on sensitive endpoints (login, payments, file uploads)
- **Role-based authorization** with granular permission control

### 📚 Learning Management
- **Hierarchical course structure**: Courses → Sections → Lectures
- **Video content delivery** via Cloudinary with adaptive streaming
- **Lecture progress tracking** with resume playback functionality
- **Course ratings and reviews** with moderation capabilities
- **Course bookmarking** for students' learning lists
- **Category-based course organization** with slug-based URLs
- **Certificate generation** upon course completion

### 💳 Payment Processing
- **Stripe integration** for seamless card payments with checkout sessions
- **Manual payment system** with proof upload and admin approval workflow
- **Shopping cart functionality** for multi-course purchases
- **Coupon and discount system** with usage tracking
- **Instructor payout system** with Stripe Connect integration
- **Invoice generation** for completed transactions

### 📊 Analytics & Reporting
- **Instructor dashboard** with enrollment analytics and revenue tracking
- **Course performance metrics** including completion rates and ratings
- **Admin reports** with platform-wide statistics
- **Monthly trend analysis** for business insights

### ⚙️ Enterprise Features
- **Serilog structured logging** with Console, File, and Seq sinks
- **Hangfire background jobs** for email sending, cleanup tasks, and scheduled operations
- **Health checks** for Database, Redis, Stripe, Cloudinary, and Email services
- **API versioning** for backward compatibility
- **CQRS pattern** with MediatR for clean command/query separation

---

## ✨ Features

### 🎓 Learning Management System
| Feature | Description |
|---------|-------------|
| **Course Creation** | Full course builder with sections, lectures, and video uploads |
| **Video Streaming** | Cloudinary-powered video delivery with adaptive bitrate |
| **Progress Tracking** | Lecture-level progress with resume playback position |
| **Certificates** | Automatic certificate generation upon completion |
| **Ratings & Reviews** | 5-star rating system with written reviews |
| **Bookmarks** | Save courses to personal learning lists |
| **Category System** | Organize courses with hierarchical categories |
| **Search & Filter** | Advanced course discovery with pagination |

### 👥 User Management
| Feature | Description |
|---------|-------------|
| **Multi-role System** | Admin, Instructor, and Student roles with distinct permissions |
| **Profile Management** | User profiles with avatar upload and personal info |
| **JWT Authentication** | Secure token-based auth with refresh token rotation |
| **Two-Factor Auth** | Email, Authenticator App, and Backup Codes support |
| **Google OAuth** | One-click social login integration |
| **Email Verification** | OTP-based email verification flow |
| **Password Recovery** | Secure password reset with expiring tokens |
| **Account Banning** | Admin ability to ban/unban users |

### 💰 Payment & Commerce
| Feature | Description |
|---------|-------------|
| **Stripe Payments** | Full Stripe integration with checkout sessions |
| **Manual Payments** | Alternative payment with proof upload |
| **Payment Approval** | Admin workflow for manual payment verification |
| **Shopping Cart** | Multi-item cart with real-time totals |
| **Coupon System** | Discount codes with usage limits and expiry |
| **Instructor Payouts** | Stripe Connect for instructor withdrawals |
| **Invoice Generation** | Detailed invoices for all transactions |
| **Refund Handling** | Admin-managed refund processing |

### 👨‍🏫 Instructor Tools
| Feature | Description |
|---------|-------------|
| **Course Analytics** | Detailed enrollment and revenue statistics |
| **Student Management** | View enrolled students and their progress |
| **Payout Management** | Request and track earnings withdrawals |
| **Stripe Connect** | Full onboarding for direct bank payouts |
| **Revenue Dashboard** | Real-time earnings and trend analysis |

### 🛡️ Admin Dashboard
| Feature | Description |
|---------|-------------|
| **User Management** | View, ban, and manage all platform users |
| **Course Moderation** | Approve, reject, and manage course publications |
| **Payment Oversight** | Review and process manual payments |
| **Platform Analytics** | Overall statistics and growth metrics |
| **Payment Settings** | Enable/disable payment methods dynamically |
| **Withdrawal Approvals** | Process instructor withdrawal requests |

### 🤖 AI Features
| Feature | Description |
|---------|-------------|
| **AI Chat Assistant** | HuggingFace-powered learning assistant |
| **Smart Recommendations** | Course suggestions based on interests |

### 🔧 Enterprise & Operations
| Feature | Description |
|---------|-------------|
| **Structured Logging** | Serilog with Console, File, and Seq sinks |
| **Background Jobs** | Hangfire for async task processing |
| **Health Monitoring** | Comprehensive health check endpoints |
| **Rate Limiting** | Protection against abuse and DDoS |
| **API Versioning** | Backward-compatible API evolution |
| **Email Templates** | Branded HTML email notifications |
| **Performance Logging** | Request timing and slow query detection |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           LEARNIFY BACKEND API                              │
│                          Clean Architecture Design                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                    🌐 Presentation Layer (API)                        │  │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐  │  │
│  │  │ Controllers │ │ Middleware  │ │ Rate Limit  │ │ Health Checks   │  │  │
│  │  │   (25)      │ │ Exception   │ │ Policies    │ │ DB/Redis/Stripe │  │  │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────────┘  │  │
│  │  REST API (v1) • Scalar API Docs • JWT Auth • API Versioning          │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                      │
│  ┌───────────────────────────────────▼───────────────────────────────────┐  │
│  │                  📋 Application Layer (CQRS)                          │  │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐  │  │
│  │  │  Commands   │ │   Queries   │ │    DTOs     │ │   Validators    │  │  │
│  │  │ CreateUser  │ │ GetCourses  │ │  Requests   │ │ FluentValidation│  │  │
│  │  │ EnrollUser  │ │ GetPayments │ │  Responses  │ │ Business Rules  │  │  │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────────┘  │  │
│  │  MediatR Handlers • AutoMapper Profiles • Pipeline Behaviors         │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                      │
│  ┌───────────────────────────────────▼───────────────────────────────────┐  │
│  │                  🏛️ Domain Layer (Core)                               │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │                      Entities (23 Total)                        │  │  │
│  │  │  ApplicationUser • Course • Section • Lecture • Enrollment      │  │  │
│  │  │  Payment • Cart • CartItem • Category • Coupon • Invoice        │  │  │
│  │  │  LectureProgress • CourseRating • UserBookmark • InstructorPayout│ │  │
│  │  │  ManualPaymentRequest • ManualPaymentMethod • SystemSetting     │  │  │
│  │  └─────────────────────────────────────────────────────────────────┘  │  │
│  │  Enums • Value Objects • Domain Events                                │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                      │
│  ┌───────────────────────────────────▼───────────────────────────────────┐  │
│  │               🔌 Infrastructure Layer (External Concerns)             │  │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐  │  │
│  │  │ EF Core 9   │ │Repositories │ │  Services   │ │  Background     │  │  │
│  │  │ DbContext   │ │ Generic +   │ │ Cloudinary  │ │  Hangfire Jobs  │  │  │
│  │  │ Migrations  │ │ Specialized │ │ Stripe API  │ │  Email Queue    │  │  │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────────┘  │  │
│  │  Identity • JWT Service • Email Service • TOTP Service               │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                         🔗 External Services                                │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐         │
│  │SQL Server│ │  Redis   │ │Cloudinary│ │  Stripe  │ │HuggingFace│         │
│  │  (Data)  │ │ (Cache)  │ │ (Media)  │ │(Payments)│ │   (AI)    │         │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘         │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐                                   │
│  │   Seq    │ │  Gmail   │ │  Google  │                                   │
│  │(Logging) │ │  (SMTP)  │ │  OAuth   │                                   │
│  └──────────┘ └──────────┘ └──────────┘                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Key Architectural Patterns

| Pattern | Implementation |
|---------|----------------|
| **Clean Architecture** | 4-layer separation: API → Application → Domain → Infrastructure |
| **CQRS** | Commands and Queries separated via MediatR handlers |
| **Repository Pattern** | Generic and specialized repositories for data access |
| **Unit of Work** | EF Core DbContext manages transaction boundaries |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection throughout |
| **Options Pattern** | Strongly-typed configuration binding |
| **Middleware Pipeline** | Request/response pipeline for cross-cutting concerns |

---

## 🛠️ Tech Stack

### Core Framework
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 9.0 | Runtime and SDK |
| ASP.NET Core | 9.0 | Web framework |
| C# | 13.0 | Programming language |
| Entity Framework Core | 9.0 | ORM and migrations |

### Database & Caching
| Technology | Purpose |
|------------|---------|
| SQL Server | Primary relational database |
| Redis | Distributed caching |
| ASP.NET Core Identity | User and role management |

### API & Documentation
| Technology | Purpose |
|------------|---------|
| Scalar | Modern API documentation UI |
| Asp.Versioning | API version management |
| FluentValidation | Request validation |
| AutoMapper | Object-to-object mapping |

### Background Processing & Logging
| Technology | Purpose |
|------------|---------|
| Hangfire | Background job scheduling |
| Serilog | Structured logging |
| Seq | Log aggregation and search |

### External Services
| Service | Purpose |
|---------|---------|
| Stripe | Payment processing & Connect payouts |
| Cloudinary | Video/image hosting and streaming |
| HuggingFace | AI chat assistance |
| Google OAuth | Social authentication |
| Gmail SMTP | Email notifications |

### Application Patterns
| Library | Purpose |
|---------|---------|
| MediatR | CQRS and pipeline behaviors |
| FluentValidation | Input validation rules |
| AutoMapper | DTO mapping automation |

---

## 🚀 Getting Started

### Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (or LocalDB for development)
- **Redis** (optional, for distributed caching)
- **Stripe Account** - [Sign up](https://stripe.com)
- **Cloudinary Account** - [Sign up](https://cloudinary.com)

### 1. Clone & Install

```bash
git clone https://github.com/AhmedV20/Learnify.git
cd Learnify/src/Learnify.Api
dotnet restore
```

### 2. Configure

Create `appsettings.Development.json` with the following essential settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LearnifyDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  },
  "JWT": {
    "ValidAudience": "Learnify_Users",
    "ValidIssuer": "Learnify_Api",
    "Secret": "YOUR_JWT_SECRET_KEY_MINIMUM_64_CHARACTERS_FOR_SECURITY",
    "TokenExpiryHours": 168
  },
  "Stripe": {
    "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY",
    "PublishableKey": "pk_test_YOUR_STRIPE_PUBLISHABLE_KEY",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET"
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Learnify",
    "SenderPassword": "YOUR_EMAIL_APP_PASSWORD"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  },
  "HuggingFace:ApiKey": "YOUR_HUGGINGFACE_API_KEY",
  "App": {
    "BaseUrl": "http://localhost:5279"
  }
}
```

> **Note**: For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833) instead of your regular password.

### 3. Run Database Migrations

```bash
dotnet ef database update --project ../Learnify.Infrastructure
```

### 4. Start the API

```bash
dotnet run
```

### 5. Access the Application

| Endpoint | URL | Description |
|----------|-----|-------------|
| **API Base** | http://localhost:5279 | REST API endpoint |
| **Scalar API Docs** | http://localhost:5279/scalar/v1 | Interactive documentation |
| **Hangfire Dashboard** | http://localhost:5279/hangfire | Background jobs monitor |
| **Health Check** | http://localhost:5279/health | System health status |
| **Health Check (Detailed)** | http://localhost:5279/health/details | Detailed health report |

---

## 📚 API Documentation

Interactive API documentation is available at `/scalar/v1` when the application is running. Below are the key endpoint groups. For a complete list, see the live Scalar documentation.

> **Note**: The API has **25 controllers** with 100+ endpoints. Only the most important ones are shown below.

### 🔐 Authentication (`/api/v1/users`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/register` | Register a new user account | ❌ |
| `POST` | `/login` | Authenticate and receive JWT tokens | ❌ |
| `POST` | `/verify-2fa` | Verify 2FA code during login | ❌ |
| `POST` | `/google-login` | Sign in with Google OAuth | ❌ |
| `POST` | `/verify-otp` | Verify email with OTP | ❌ |
| `POST` | `/forgot-password` | Request password reset OTP | ❌ |
| `POST` | `/set-new-password` | Set new password with reset token | ❌ |
| `GET` | `/profile` | Get current user profile | ✅ |
| `PUT` | `/profile` | Update user profile | ✅ |

### 📚 Courses (`/api/v1/courses`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/` | List all courses (paginated) | ❌ |
| `GET` | `/{id}` | Get course details | ❌ |
| `GET` | `/{id}/learn` | Get course with full content | ✅ |
| `POST` | `/` | Create new course | ✅ Admin |
| `PUT` | `/{id}` | Update course | ✅ |
| `DELETE` | `/{id}` | Delete course | ✅ |
| `GET` | `/category` | Get courses by category | ❌ |

### 📝 Enrollments (`/api/v1/enrollments`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/user` | Get current user's enrollments | ✅ |
| `GET` | `/check/{courseId}` | Check enrollment status | ✅ |
| `POST` | `/` | Create enrollment | ✅ |
| `DELETE` | `/{id}` | Delete enrollment | ✅ |

### 💳 Payments (`/api/v1/payments`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/settings` | Get payment settings | ✅ |
| `POST` | `/checkout-session` | Create Stripe checkout | ✅ |
| `POST` | `/verify-payment/{sessionId}` | Verify and process payment | ✅ |
| `GET` | `/my-payments` | Get user's payment history | ✅ |

### 👨‍🏫 Instructor (`/api/v1/instructor`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/balance` | Get earnings balance | ✅ |
| `GET` | `/payouts` | Get payout history | ✅ |
| `POST` | `/request-withdrawal` | Request funds withdrawal | ✅ |
| `POST` | `/create-stripe-account` | Setup Stripe Connect | ✅ |
| `GET` | `/stripe-connect-status` | Get Stripe account status | ✅ |

### 🛡️ Admin (`/api/v1/admin`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/dashboard-stats` | Get platform statistics | ✅ Admin |
| `GET` | `/users` | List all users | ✅ Admin |
| `PATCH` | `/users/{id}/ban` | Ban user | ✅ Admin |
| `GET` | `/courses` | List all courses (admin) | ✅ Admin |
| `PATCH` | `/courses/{id}/approve` | Approve course | ✅ Admin |
| `GET` | `/withdrawal-requests` | List withdrawal requests | ✅ Admin |
| `PUT` | `/withdrawal-requests/{id}/approve` | Approve withdrawal | ✅ Admin |

### 📊 Additional Endpoints

The API also includes endpoints for:
- **Two-Factor Auth** - Email, Authenticator App, Backup Codes setup
- **Sections & Lectures** - Course content management with video upload
- **Lecture Progress** - Track watch position and completion
- **Shopping Cart** - Add/remove courses from cart
- **Manual Payments** - Alternative payment with proof upload
- **Course Ratings** - Reviews and ratings system
- **Categories** - Course organization
- **Bookmarks** - Save courses for later
- **Coupons** - Discount codes
- **Analytics** - Instructor revenue and enrollment stats
- **AI Chat** - HuggingFace-powered learning assistant
- **Health Checks** - System monitoring

---

## 📁 Project Structure

```
src/
├── Learnify.Api/                     # 🌐 Presentation Layer
│   ├── Controllers/                  # 25 REST API controllers
│   │   ├── UsersController.cs        # Authentication & profile
│   │   ├── CoursesController.cs      # Course management
│   │   ├── PaymentsController.cs     # Payment processing
│   │   ├── AdminController.cs        # Admin operations
│   │   └── ...                       # + 21 more controllers
│   ├── Extensions/                   # DI & service configuration
│   ├── Health/                       # Health check implementations
│   ├── Middleware/                   # Exception handling, logging
│   └── RateLimiting/                 # Rate limit policies
│
├── Learnify.Application/             # 📋 Business Logic Layer
│   ├── Users/                        # Auth commands & queries
│   ├── Courses/                      # Course CQRS operations
│   ├── Enrollments/                  # Enrollment logic
│   ├── Payments/                     # Payment processing
│   ├── Common/                       # Interfaces, pagination, behaviors
│   ├── Mappings/                     # AutoMapper profiles
│   └── ...                           # + 15 more feature modules
│
├── Learnify.Domain/                  # 🏛️ Core Domain
│   ├── Entities/                     # 23 domain entities
│   │   ├── ApplicationUser.cs        # Extended Identity user
│   │   ├── Course.cs                 # Course aggregate root
│   │   ├── Enrollment.cs             # Student enrollments
│   │   ├── Payment.cs                # Payment records
│   │   └── ...                       # + 19 more entities
│   └── Enums/                        # Domain enumerations
│
└── Learnify.Infrastructure/          # 🔌 External Concerns
    ├── Data/                         # EF Core DbContext & migrations
    ├── Repositories/                 # Data access implementations
    ├── Services/                     # Cloudinary, Stripe, Email
    ├── BackgroundJobs/               # Hangfire job implementations
    └── Seed/                         # Database seeding
```

> **Note**: For detailed file listings, browse the [repository on GitHub](https://github.com/AhmedV20/Learnify).

---

## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Fork** the repository
2. **Clone** your fork: `git clone https://github.com/YOUR_USERNAME/Learnify.git`
3. **Create** a feature branch: `git checkout -b feature/amazing-feature`
4. **Make** your changes and ensure the project builds
5. **Commit** using [Conventional Commits](https://www.conventionalcommits.org/): `git commit -m 'feat: add amazing feature'`
6. **Push** to your branch: `git push origin feature/amazing-feature`
7. **Open** a Pull Request against the `develop` branch

### Guidelines

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Write meaningful commit messages
- Add/update tests for new features
- Update documentation as needed

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines and [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md) for branching strategy.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

<div align="center">

**Built with ❤️ using .NET 9**

[⬆ Back to Top](#learnify-backend-api)

</div>