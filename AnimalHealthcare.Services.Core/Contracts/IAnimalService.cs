using AnimalHealthcare.Web.ViewModels.Animal;
using AnimalHealthcare.Web.ViewModels.UserProfile;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IAnimalService
    {
        Task<List<AnimalSummaryViewModel>> GetAnimalSummariesByOwnerIdAsync(string userId);
        Task RegisterAnimalAsync(string userId, RegisterPetViewModel model);
        Task<UnregisterPetViewModel?> GetPetUnregisterViewModelByIdAsync(int id);
        Task<bool> UnregisterPetAsync(int id);
    }
}
