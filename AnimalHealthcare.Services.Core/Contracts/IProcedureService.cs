using AnimalHealthcare.Web.ViewModels.Procedure;

namespace AnimalHealthcare.Services.Core.Contracts
{
    public interface IProcedureService
    {
        Task<IEnumerable<ProcedureListItemViewModel>> GetAllProceduresAsync();

        Task<ProcedureDetailsViewModel?> GetProcedureDetailsAsync(int procedureId);

        Task<string?> GetProcedureNameByIdAsync(int procedureId);
    }
}
