using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using Learnify.Infrastructure.AppliedCoupons.Persistence;
using Learnify.Infrastructure.Carts;
using Learnify.Infrastructure.Categories.Persistence;
using Learnify.Infrastructure.CourseRatings.Persistence;
using Learnify.Infrastructure.Courses.Persistence;
using Learnify.Infrastructure.Data;
using Learnify.Infrastructure.Enrollments.Persistence;
using Learnify.Infrastructure.InvoiceItems.Persistence;
using Learnify.Infrastructure.Invoices.Persistence;
using Learnify.Infrastructure.Lectures.Persistence;
using Learnify.Infrastructure.OrderDetails.Persistence;
using Learnify.Infrastructure.Payouts.Persistence;
using Learnify.Infrastructure.Payments.Persistence;
using Learnify.Infrastructure.Sections.Persistence;
using Learnify.Infrastructure.Seed;
using Learnify.Infrastructure.Services;
using Learnify.Infrastructure.UserBookmarks.Persistence;
using Learnify.Application.ManualPayments;
using Learnify.Infrastructure.ManualPayments;
using Learnify.Infrastructure.Repositories;
using Learnify.Application.TwoFactorAuth;
using Learnify.Infrastructure.TwoFactorAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
namespace Learnify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,  IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentity<ApplicationUser, IdentityRole>(options => 
        {
            // Disable ALL password requirements
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 1;                 // Minimum possible
            options.Password.RequiredUniqueChars = 1;            // Minimum possible
            
            // Optional: Keep email unique
            options.User.RequireUniqueEmail = true;
        })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            //options.SaveToken = true;
            //options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = configuration["JWT:ValidAudience"],
                ValidIssuer = configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
            };
        });


        // For Redis - DISABLED: Using in-memory cart for development
        // Uncomment and configure Redis connection in appsettings.json when Redis is available
        // services.AddSingleton<IConnectionMultiplexer>(sp =>
        // {
        //     var redisConfiguration = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"), true);
        //     redisConfiguration.AbortOnConnectFail = false; // Don't fail if Redis is not available
        //     redisConfiguration.ConnectRetry = 3;
        //     return ConnectionMultiplexer.Connect(redisConfiguration);
        // });


        services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        services.AddScoped<DataSeeder>();
        services.AddScoped<IdentitySeeder>();
        services.AddScoped<ICourseRepository,CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
        services.AddScoped<IUserBookmarkRepository, UserBookmarkRepository>();
        services.AddScoped<ILectureRepository, LectureRepository>();
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<IAppliedCouponRepository, AppliedCouponRepository>();
        services.AddScoped<ICourseRatingRepository, CourseRatingRepository>();
        services.AddScoped<IInstructorPayoutRepository, InstructorPayoutRepository>();
        services.AddScoped<IManualPaymentRepository, ManualPaymentRepository>();
        services.AddScoped<ILectureProgressRepository, LectureProgressRepository>();


        // Cart Repository - Using in-memory for development (switch to RedisCartRepository for production)
        services.AddSingleton<ICartRepository, InMemoryCartRepository>();

        
        // Services
        services.AddHttpContextAccessor();
        services.AddScoped<IImageUrlService, ImageUrlService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IStripeConnectService, StripeConnectService>();
        
        // Two-Factor Authentication Services
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<IBackupCodeService, BackupCodeService>();
        
        // Video/File Upload Service - Switch between providers via config
        // Options: "Cloudinary" (default), "GoogleCloud", "Local"
        var storageProvider = configuration["Storage:Provider"]?.ToLower() ?? "local";
        
        switch (storageProvider)
        {
            case "cloudinary":
                services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
                services.AddScoped<IVideoUploadService, CloudinaryStorageService>();
                break;
                
            case "googlecloud":
                // Use Google Cloud Storage (requires credentials file)
                services.AddScoped<IVideoUploadService, GoogleCloudStorageService>();
                break;
                
            default: // "local"
                // Use Local File Storage (default - no cloud credentials needed)
                services.AddScoped<IVideoUploadService, LocalFileStorageService>();
                break;
        }
        
        return services;
    }
}

