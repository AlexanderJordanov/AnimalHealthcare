namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IUserProfileService
    {
        Task CreateUserProfileAsync(string userId, string fullName, string? phoneNumber, string? address, string? profilePictureUrl);
    }
}

