using System.ComponentModel.DataAnnotations; // RegularExpression

namespace SeelansTyres.Models.IdentityModels.V1_0_0;

public class UpdateAccountModel
{
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string FirstName { get; set; } = string.Empty;
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string LastName { get; set; } = string.Empty;
    [RegularExpression(@"^\d{10}$")]
    public string PhoneNumber { get; set; } = string.Empty;

}
