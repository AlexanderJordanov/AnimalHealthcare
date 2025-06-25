using System.ComponentModel.DataAnnotations;

namespace AnimalHealthcare.Web.ViewModels.UserProfile
{
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;
    using static AnimalHealthcare.GCommon.ValidationConstants.ErrorMessages;
    public class EditPhoneNumberViewModel
    {
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(PhoneNumberMaxLength, ErrorMessage = StringLength)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
