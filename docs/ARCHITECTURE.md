# Learnify Backend Architecture

## System Overview

```mermaid
graph TB
    subgraph "API Gateway"
        API[🌐 Learnify.Api<br/>.NET 9]
        MW[Middleware<br/>Auth, Logging, CORS, RateLimit]
    end

    subgraph "Application Layer"
        CMD[Commands<br/>Create, Update, Delete]
        QRY[Queries<br/>Read Operations]
        SVC[Services<br/>Email, Background Jobs]
    end

    subgraph "Infrastructure"
        REPO[Repositories]
        HF[Hangfire<br/>Background Jobs]
        EMAIL[Email Service]
    end

    subgraph "External Services"
        DB[(SQL Server)]
        REDIS[(Redis Cache)]
        CLOUD[☁️ Cloudinary]
        STRIPE[💳 Stripe]
        SEQ[📋 Seq Logging]
        AI[🤖 HuggingFace]
    end

    API --> MW
    MW --> CMD
    MW --> QRY
    CMD --> SVC
    QRY --> REPO
    SVC --> HF
    SVC --> EMAIL
    REPO --> DB
    REPO --> REDIS
    EMAIL --> CLOUD
    SVC --> STRIPE
    API --> SEQ
    SVC --> AI
```

---

## Clean Architecture Layers

```mermaid
graph TD
    subgraph "Learnify.Api - Presentation"
        Controllers
        Extensions
        Middleware
        Logging
        Health
        RateLimiting
    end

    subgraph "Learnify.Application - Business Logic"
        Commands
        Queries
        Interfaces
        DTOs
        Validators
        BackgroundJobs
        Mappings
    end

    subgraph "Learnify.Infrastructure - External"
        Repositories
        Services
        Data
        BackgroundJobServices
        Seed
    end

    subgraph "Learnify.Domain - Core"
        Entities
        Enums
    end

    Controllers --> Commands
    Controllers --> Queries
    Commands --> Interfaces
    Queries --> Interfaces
    Interfaces -.-> Repositories
    Interfaces -.-> Services
    Repositories --> Entities
    Services --> Entities
```

---

## Data Flow: User Registration

```mermaid
sequenceDiagram
    participant C as Client
    participant API as API
    participant CMD as RegisterUserCommand
    participant HF as Hangfire
    participant EMAIL as EmailJobService
    
    C->>API: POST /api/users/register
    API->>CMD: Handle(RegisterUserCommand)
    CMD->>CMD: Validate & Create user
    CMD->>HF: Enqueue OTP email job
    CMD-->>API: Return success
    API-->>C: 200 OK
    
    Note over HF,EMAIL: Background Processing
    HF->>EMAIL: SendOtpEmailAsync()
    EMAIL->>EMAIL: Send email via SMTP
    
    C->>API: POST /api/users/verify-otp
    API->>HF: Enqueue Welcome email job
    API-->>C: 200 OK + JWT Token
```

---

## Payment Flow

```mermaid
flowchart LR
    subgraph "Manual Payment"
        A[User] --> B[Upload Proof]
        B --> C[Pending Review]
        C --> D{Admin Review}
        D -->|Approve| E[Send Confirmation Email]
        D -->|Reject| F[Send Rejection Email]
        E --> G[Enroll in Courses]
    end

    subgraph "Stripe Payment"
        H[User] --> I[Stripe Checkout]
        I --> J[Webhook Received]
        J --> K[Auto Enroll]
        K --> L[Send Receipt]
    end
```

---

## Tech Stack

```mermaid
mindmap
  root((Learnify API))
    Core
      .NET 9
      ASP.NET Core
      C# 12
    Data
      EF Core 9
      SQL Server
      Redis
    Patterns
      Clean Architecture
      CQRS
      MediatR
    Infrastructure
      Hangfire
      Serilog
      Seq
    Integrations
      Stripe
      Cloudinary
      HuggingFace
      SMTP
```

---

## Project Structure

```
src/
├── Learnify.Api/                     # Presentation Layer
│   ├── Controllers/                  # 25 REST API controllers
│   │   ├── UsersController.cs        # Auth, registration, profile
│   │   ├── TwoFactorController.cs    # 2FA setup/verify
│   │   ├── CoursesController.cs      # Course CRUD
│   │   ├── AdminController.cs        # Admin operations
│   │   └── ...
│   ├── Extensions/                   # Service registrations
│   │   ├── HangfireExtensions.cs     # Background job config
│   │   └── SerilogExtensions.cs      # Logging config
│   ├── Health/                       # Health check implementations
│   │   ├── DatabaseHealthCheck.cs
│   │   ├── StripeHealthCheck.cs
│   │   └── CloudinaryHealthCheck.cs
│   ├── Logging/                      # Structured logging
│   │   ├── RequestLoggingMiddleware.cs
│   │   └── CorrelationIdEnricher.cs
│   ├── Middleware/                   # Request pipeline
│   │   └── ExceptionHandlerMiddleware.cs
│   ├── RateLimiting/                 # Rate limit policies
│   └── Program.cs                    # Application entry point
│
├── Learnify.Application/             # Business Logic Layer
│   ├── Users/                        # User feature module
│   │   ├── Commands/                 # Register, Login, etc.
│   │   ├── Queries/                  # GetUser, GetProfile
│   │   └── DTOs/                     # UserDto, LoginResponse
│   ├── Courses/                      # Course management
│   ├── Enrollments/                  # Student enrollments
│   ├── Payments/                     # Payment processing
│   ├── TwoFactorAuth/                # 2FA logic
│   ├── BackgroundJobs/               # Job interfaces
│   │   ├── IEmailJobService.cs
│   │   └── ICleanupJobService.cs
│   ├── Common/                       # Shared components
│   │   ├── Behaviors/                # MediatR behaviors
│   │   ├── Responses/                # API response models
│   │   └── Validators/               # FluentValidation
│   └── Mappings/                     # AutoMapper profiles
│
├── Learnify.Infrastructure/          # External Concerns
│   ├── Data/                         # Database layer
│   │   ├── ApplicationDbContext.cs   # EF Core context
│   │   ├── Migrations/               # Database migrations
│   │   └── EntityConfigurations/     # Fluent API configs
│   ├── Repositories/                 # Data access
│   │   └── GenericRepository.cs
│   ├── Services/                     # External integrations
│   │   ├── EmailService.cs
│   │   ├── StripeService.cs
│   │   ├── CloudinaryService.cs
│   │   └── AiChatService.cs
│   ├── BackgroundJobs/               # Hangfire implementations
│   │   ├── EmailJobService.cs
│   │   └── CleanupJobService.cs
│   ├── Seed/                         # Database seeding
│   └── DependencyInjections.cs       # DI registration
│
└── Learnify.Domain/                  # Core Domain
    ├── Entities/                     # 23 domain entities
    │   ├── ApplicationUser.cs
    │   ├── Course.cs
    │   ├── Section.cs
    │   ├── Lecture.cs
    │   ├── Enrollment.cs
    │   ├── Payment.cs
    │   └── ...
    └── Enums/                        # Domain enumerations
        └── PaymentStatus.cs
```

---

## API Controllers Overview

| Controller | Endpoints | Description |
|------------|-----------|-------------|
| UsersController | 8 | Auth, registration, profile |
| TwoFactorController | 6 | 2FA setup, verify, disable |
| CoursesController | 7 | Course CRUD operations |
| CategoriesController | 5 | Category management |
| SectionsController | 4 | Course section management |
| LecturesController | 6 | Lecture CRUD, video upload |
| EnrollmentsController | 5 | Student enrollments |
| PaymentsController | 4 | Stripe checkout |
| ManualPaymentsController | 4 | Manual payment requests |
| AdminController | 12 | User/course management |
| AdminReportsController | 4 | Analytics & reports |
| InstructorController | 8 | Instructor dashboard |
| AiChatController | 3 | AI assistant |
| CartsController | 4 | Shopping cart |

---

## Domain Entities

```mermaid
erDiagram
    ApplicationUser ||--o{ Course : creates
    ApplicationUser ||--o{ Enrollment : has
    ApplicationUser ||--o{ Cart : has
    
    Course ||--|{ Section : contains
    Section ||--|{ Lecture : contains
    Course ||--o{ CourseRating : has
    Course }|--|| Category : belongs_to
    
    Enrollment }|--|| Course : for
    Enrollment }|--|| ApplicationUser : by
    
    Payment }|--|| ApplicationUser : made_by
    Payment ||--o{ OrderDetail : contains
    
    Cart ||--|{ CartItem : contains
    CartItem }|--|| Course : references
```

---

## External Service Integrations

| Service | Purpose | Configuration |
|---------|---------|---------------|
| SQL Server | Primary database | ConnectionStrings:DefaultConnection |
| Redis | Caching, session | ConnectionStrings:Redis |
| Cloudinary | Video/image hosting | Cloudinary:* |
| Stripe | Payment processing | Stripe:SecretKey |
| Seq | Log aggregation | Serilog:Seq:* |
| SMTP | Email delivery | Email:* |
| HuggingFace | AI chat | AiChat:ApiKey |
