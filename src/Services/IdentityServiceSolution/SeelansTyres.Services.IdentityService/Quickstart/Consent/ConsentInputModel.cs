﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

public class ConsentInputModel
{
    public string Button { get; set; } = string.Empty;
    public IEnumerable<string>? ScopesConsented { get; set; }
    public bool RememberConsent { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}