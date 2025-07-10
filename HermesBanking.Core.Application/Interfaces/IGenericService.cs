namespace HermesBanking.Core.Application.Interfaces
{
    public interface IGenericService<DTOModel> where DTOModel : class
    {
        Task<DTOModel?> AddAsync(DTOModel dto);
        Task<DTOModel?> UpdateAsync(DTOModel dto, int id);
        Task<bool> DeleteAsync(int? id);
        Task<DTOModel?> GetById(int id);
        Task<List<DTOModel>> GetAll();
    }
}