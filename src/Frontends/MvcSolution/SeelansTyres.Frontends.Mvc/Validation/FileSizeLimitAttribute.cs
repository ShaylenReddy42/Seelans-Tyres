using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Frontends.Mvc.Validation;

public class FileSizeLimitAttribute : ValidationAttribute
{
    public int LimitInMB { get; }
    
    public FileSizeLimitAttribute(int limitInMB) => LimitInMB = limitInMB;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }
        
        long fileSizeLimitInBytes = (long)(LimitInMB * Math.Pow(1024, 2));

        if ((long)value > fileSizeLimitInBytes)
        {
            return new ValidationResult($"File size is greater than the limit of {LimitInMB}MB");
        }
        
        return ValidationResult.Success;
    }
}
