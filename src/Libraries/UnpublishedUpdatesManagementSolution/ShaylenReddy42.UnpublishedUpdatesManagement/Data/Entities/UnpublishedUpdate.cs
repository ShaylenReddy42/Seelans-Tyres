using System.ComponentModel.DataAnnotations.Schema; // DatabaseGenerated
using System.ComponentModel.DataAnnotations;        // Key, Required

namespace ShaylenReddy42.UnpublishedUpdatesManagement.Data.Entities;

/// <summary>
/// Represents the data structure for how an unpublished update is stored in the database
/// </summary>
public class UnpublishedUpdate
{
    /// <summary>
    /// The id of the unpublished update
    /// </summary>
    /// <remarks>
    /// <see cref="long"/> was chosen as the datatype for future-proofing
    /// </remarks>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// This is a base64url-encoded string representation of the json of the original update
    /// </summary>
    [Required]
    public string EncodedUpdate { get; set; } = string.Empty;

    /// <summary>
    /// The exchange, topic or queue that the update is meant to be published to
    /// </summary>
    [Required]
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// The number of attempts it took to publish the update to the destination
    /// </summary>
    public int Retries { get; set; } = 0;
}
