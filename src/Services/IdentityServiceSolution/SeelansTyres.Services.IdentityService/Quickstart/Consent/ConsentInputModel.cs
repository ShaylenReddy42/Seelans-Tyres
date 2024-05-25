// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI;

public class ConsentInputModel
{
    [Required]
    public string Button { get; set; } = string.Empty;
    public IEnumerable<string>? ScopesConsented { get; set; }
    [Required]
    public bool RememberConsent { get; set; } = true;
    [Required]
    public string ReturnUrl { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
}