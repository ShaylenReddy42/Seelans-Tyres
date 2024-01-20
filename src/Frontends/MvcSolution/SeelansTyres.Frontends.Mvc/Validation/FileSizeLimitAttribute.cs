using System.ComponentModel.DataAnnotations; // ValidationAttribute, ValidationResult, ValidationContext

namespace SeelansTyres.Frontends.Mvc.Validation;

/// <summary>
/// Specifies that a file cannot exceed a set limit
/// </summary>
/// <remarks>
/// Usage: The file size of an IFormFile [IFormFile.Length] is extracted into its own property and is then validated
/// </remarks>
public class FileSizeLimitAttribute(int limitInMB) : ValidationAttribute
{
    public int LimitInMB { get; } = limitInMB;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        double actualFileSizeInMB = (long)value / Math.Pow(1024, 2);

        long fileSizeLimitInBytes = (long)(LimitInMB * Math.Pow(1024, 2));

        if ((long)value > fileSizeLimitInBytes)
        {
            return new ValidationResult($"The file size of {actualFileSizeInMB:0.00}MB is greater than the limit of {LimitInMB}MB");
        }
        
        return ValidationResult.Success;
    }
}
