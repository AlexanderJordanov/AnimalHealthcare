namespace AnimalHealthcare.GCommon
{
    public static class ValidationConstants
   {
        public static class Doctor
        {
            public const int NameMaxLength = 100;
            public const int SpecializationMaxLength = 100;
            public const int PhoneNumberMaxLength = 20;
            public const int ImageUrlMaxLength = 255;

            public const int DefaultPage = 1;
            public const int PageSize = 5;
        }

        public static class AnimalClinic
        {
            public const int NameMaxLength = 100;
            public const int AddressMaxLength = 250;
            public const int PhoneNumberMaxLength = 20;
            public const int ImageUrlMaxLength = 255;
        }

        public static class Procedure
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 1000;
        }

        public static class Animal
        {
            public const int NameMaxLength = 100;
            public const int NameMinLength = 2;
            public const int SpeciesMaxLength = 50;
            public const int SpeciesMinLength = 2;
            public const int BreedMaxLength = 50;
            public const int BreedMinLength = 2;
            public const int AgeMin = 0;
            public const int AgeMax = 100;
        }

        public static class UserProfile
        {
            public const int FullNameMaxLength = 100;
            public const int FullNameMinLength = 2;
            public const int PhoneNumberMinLength = 10;
            public const int PhoneNumberMaxLength = 20;
            public const int AddressMinLength = 20;
            public const int AddressMaxLength = 250;
            public const int ProfilePictureUrlMaxLength = 255;
            public const string FullNameRegex = @"^[a-zA-Z\s'-]+$";
            public const int EmailMaxLength = 100;
            public const int EmailMinLength = 5;
        }


        /// <summary>
        /// Centralized validation error messages for view models.
        /// Use placeholders: {0} = field name, {1} = max length, {2} = min length
        /// </summary>
        public static class ErrorMessages
        {
            public const string Required = "{0} is required.";
            public const string StringLength = "{0} must be between {2} and {1} characters.";
            public const string Email = "Please enter a valid email address.";
            public const string PhoneNumber = "Please enter a valid phone number.";
        }
    }
}
