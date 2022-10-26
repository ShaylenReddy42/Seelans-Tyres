using SeelansTyres.Frontends.Mvc.Models;

namespace SeelansTyres.Frontends.Mvc.ViewModels;

public class ResetPasswordViewModel
{
    public SendCodeModel SendCodeModel { get; set; } = null!;
    public ResetPasswordModel ResetPasswordModel { get; set; } = null!;
}
