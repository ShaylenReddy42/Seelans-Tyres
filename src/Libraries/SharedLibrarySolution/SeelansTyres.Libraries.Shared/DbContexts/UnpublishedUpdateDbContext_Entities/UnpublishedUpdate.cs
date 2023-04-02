﻿using System.ComponentModel.DataAnnotations.Schema; // DatabaseGenerated
using System.ComponentModel.DataAnnotations;        // Key, Required

namespace SeelansTyres.Libraries.Shared.DbContexts.UnpublishedUpdateDbContext_Entities;

public class UnpublishedUpdate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public string EncodedUpdate { get; set; } = string.Empty;
    [Required]
    public string Destination { get; set; } = string.Empty;
    public int Retries { get; set; } = 0;
}
