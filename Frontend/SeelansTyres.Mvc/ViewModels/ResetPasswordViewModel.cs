using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.ViewModels;

public class ResetPasswordViewModel
{
    public SendCodeModel SendCodeModel { get; set; } = null!;
    public ResetPasswordModel ResetPasswordModel { get; set; } = null!;
}
