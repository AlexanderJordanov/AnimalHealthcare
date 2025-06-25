namespace AnimalHealthcare.Web.ViewModels.UserProfile
{
    using System.ComponentModel.DataAnnotations;
    using static AnimalHealthcare.GCommon.ValidationConstants.ErrorMessages;
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;

    public class EditAddressViewModel
    {
        [StringLength(AddressMaxLength,MinimumLength = AddressMinLength, ErrorMessage = StringLength)]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }
}
