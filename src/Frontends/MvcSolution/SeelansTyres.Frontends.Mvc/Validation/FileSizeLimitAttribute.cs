﻿using System.ComponentModel.DataAnnotations;

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

        double actualFileSizeInMB = (long)value / Math.Pow(1024, 2);

        long fileSizeLimitInBytes = (long)(LimitInMB * Math.Pow(1024, 2));

        if ((long)value > fileSizeLimitInBytes)
        {
            return new ValidationResult($"The file size of {actualFileSizeInMB:0.00}MB is greater than the limit of {LimitInMB}MB");
        }
        
        return ValidationResult.Success;
    }
}