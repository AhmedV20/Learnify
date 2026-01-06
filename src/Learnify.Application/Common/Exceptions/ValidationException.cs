using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Failures = Array.Empty<ValidationFailure>();
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            var failureList = failures.ToList();
            
            // Store full failures to preserve CustomState (descriptions)
            Failures = failureList;
            
            // Keep backward compatible Errors dictionary
            Errors = failureList
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        /// <summary>
        /// Full validation failures including CustomState with descriptions
        /// </summary>
        public IReadOnlyList<ValidationFailure> Failures { get; }

        /// <summary>
        /// Backward compatible errors dictionary (field -> messages)
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }
    }
}
