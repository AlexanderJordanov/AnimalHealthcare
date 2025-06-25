using AnimalHealthcare.Web.ViewModels.UserProfile;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IAnimalService
    {
        public Task<List<AnimalSummaryViewModel>> GetAnimalSummariesByOwnerIdAsync(string userId);
    }
}
