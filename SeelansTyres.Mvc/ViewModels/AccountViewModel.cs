﻿using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.ViewModels;

public class AccountViewModel
{
    public UpdateAccountModel UpdateAccountModel { get; set; } = null!;
    public CreateAddressModel CreateAddressModel { get; set; } = null!;
}
