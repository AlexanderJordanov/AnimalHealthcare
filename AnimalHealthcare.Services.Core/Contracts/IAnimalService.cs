using AnimalHealthcare.Web.ViewModels.Animal;
using AnimalHealthcare.Web.ViewModels.UserProfile;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IAnimalService
    {
        Task<List<AnimalSummaryViewModel>> GetAnimalSummariesByOwnerIdAsync(string userId);

        Task RegisterAnimalAsync(string userId, RegisterPetViewModel model);

        Task<UnregisterPetViewModel?> GetPetUnregisterViewModelByIdAsync(int id, string? requestingUserId = null);

        Task<bool> UnregisterPetAsync(int id, string? requestingUserId = null);

        Task<AnimalDetailsViewModel?> GetAnimalDetailsViewModelAsync(int animalId, string? requestingUserId = null);

        Task<EditPetViewModel?> BuildEditPetViewModelAsync(int id, string requestingUserId);

        Task<bool> UpdateAnimalAsync(EditPetViewModel model, string requestingUserId);
    }
}
