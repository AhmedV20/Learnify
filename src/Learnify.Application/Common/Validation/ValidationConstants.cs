using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Common.Validation;

public static class ValidationConstants
{
    public static class Password
    {
        public const int MinLength = 8;
        public const int MaxLength = 128;
        public const string Pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_\-#])[A-Za-z\d@$!%*?&_\-#]{8,}$";

        public const string RequirementsMessage =
            "Password must be at least 8 characters and contain: " +
            "one uppercase letter, one lowercase letter, one digit, " +
            "and one special character (@$!%*?&_-#)";
    }

    public static class Username
    {
        public const int MinLength = 3;
        public const int MaxLength = 30;
        public const string Pattern = @"^[a-zA-Z][a-zA-Z0-9._]{2,29}$";

        public static readonly string[] ReservedUsernames =
        {
            "admin", "administrator", "root", "system", "support",
            "help", "info", "contact", "sales", "billing",
            "api", "www", "mail", "email", "ftp",
            "learnify", "ilmpath", "moderator", "mod",
            "null", "undefined", "anonymous", "guest", "user",
            "test", "demo", "example"
        };

        public const string FormatMessage =
            "Username must be 3-30 characters, start with a letter, " +
            "and contain only letters, numbers, dots, or underscores";
    }

    public static class Email
    {
        public const int MaxLength = 254;
        public const string Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    }

    public static class Name
    {
        public const int MinLength = 1;
        public const int MaxLength = 50;
        public const string Pattern = @"^[\p{L}\s'-]+$";
    }

    public static class Course
    {
        public const int TitleMinLength = 5;
        public const int TitleMaxLength = 200;
        public const int DescriptionMinLength = 20;
        public const int DescriptionMaxLength = 5000;
        public const decimal MinPrice = 0;
        public const decimal MaxPrice = 9999.99m;
    }

    public static class Category
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const string SlugPattern = @"^[a-z0-9]+(?:-[a-z0-9]+)*$";
    }
}
