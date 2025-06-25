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
            public const int SpeciesMaxLength = 50;
            public const int BreedMaxLength = 50;
        }

        public static class UserProfile
        {
            public const int FullNameMaxLength = 100;
            public const int FullNameMinLength = 2;
            public const int PhoneNumberMaxLength = 20;
            public const int AddressMaxLength = 250;
            public const int ProfilePictureUrlMaxLength = 255;
            public const string FullNameRegex = @"^[a-zA-Z\s'-]+$";
        }
    }
}
