﻿using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Models.IdentityModels.V1;

public class UpdateAccountModel
{
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string FirstName { get; set; } = string.Empty;
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string LastName { get; set; } = string.Empty;
    [RegularExpression(@"^\d{10}$")]
    public string PhoneNumber { get; set; } = string.Empty;

}