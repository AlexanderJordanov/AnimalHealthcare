using AnimalHealthcare.Web.ViewModels.AnimalClinic;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IAnimalClinicService
    {
        Task<AnimalClinicListViewModel> GetAllClinicsAsync();

        Task<AnimalClinicDetailsViewModel?> GetClinicDetailsAsync(int id);
    }
}
