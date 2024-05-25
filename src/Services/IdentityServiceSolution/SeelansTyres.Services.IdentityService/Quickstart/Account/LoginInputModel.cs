// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI;

public class LoginInputModel
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public bool RememberLogin { get; set; } = false;
    [Required]
    public string ReturnUrl { get; set; } = "/";
}