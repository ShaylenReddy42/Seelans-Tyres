﻿using System.ComponentModel.DataAnnotations; // Required, StringLength, RegularExpression

namespace SeelansTyres.Models.AddressModels.V1_0_0;

public class AddressModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;
    [RegularExpression(@"^\d{4}$")]
    public string PostalCode { get; set; } = string.Empty;
    public bool PreferredAddress { get; set; } = default;
}
