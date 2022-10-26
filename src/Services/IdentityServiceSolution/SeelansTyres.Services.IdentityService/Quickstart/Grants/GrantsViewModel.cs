// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

public class GrantsViewModel
{
    public IEnumerable<GrantViewModel> Grants { get; set; } = Enumerable.Empty<GrantViewModel>();
}

public class GrantViewModel
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientUrl { get; set; } = string.Empty;
    public string? ClientLogoUrl { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Expires { get; set; }
    public IEnumerable<string> IdentityGrantNames { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> ApiGrantNames { get; set; } = Enumerable.Empty<string>();
}