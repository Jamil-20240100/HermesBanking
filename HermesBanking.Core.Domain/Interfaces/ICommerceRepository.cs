namespace HermesBanking.Core.Domain.Interfaces
{
    public interface ICommerceRepository
    {
        Task<bool> ExistsByIdAsync(string commerceId);
    }
}
