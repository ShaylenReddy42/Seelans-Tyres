using SeelansTyres.Frontends.Mvc.Models; // SendCodeModel, ResetPasswordModel

namespace SeelansTyres.Frontends.Mvc.ViewModels;

public class ResetPasswordViewModel
{
    public SendCodeModel SendCodeModel { get; set; } = null!;
    public ResetPasswordModel ResetPasswordModel { get; set; } = null!;
}
