using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Schedule.Domain.Attributes;

public abstract class ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IsLoginCorrectAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) =>
            value is string str && Regex.IsMatch(str, @"^[a-zA-Z0-9]+$");
    }
}