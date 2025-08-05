using System.ComponentModel.DataAnnotations;


namespace AnimalHealthcare.Web.ViewModels.UserProfile
{
    using static AnimalHealthcare.GCommon.ValidationConstants;
    using static AnimalHealthcare.GCommon.ValidationConstants.ErrorMessages;
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;

    public class EditEmailViewModel
    {
        public string ProfileId { get; set; } = null!;

        [Required(ErrorMessage = Required)]
        [StringLength(EmailMaxLength, MinimumLength = EmailMinLength, ErrorMessage = StringLength)]
        [EmailAddress(ErrorMessage = ErrorMessages.Email)]
        public string Email { get; set; } = null!;
    }
}
