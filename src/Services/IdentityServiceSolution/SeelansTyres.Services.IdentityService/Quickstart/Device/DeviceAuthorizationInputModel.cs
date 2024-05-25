// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI;

public class DeviceAuthorizationInputModel : ConsentInputModel
{
    [Required] 
    public string UserCode { get; set; } = string.Empty;
}