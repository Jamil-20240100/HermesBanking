using HermesBanking.Core.Application.DTOs.HermesPay;
using System.Security.Claims;
using System.Threading.Tasks;

public interface IHermesPayService
{
    Task<HermesPayResponse> ProccesHermesPay(HermesPayRequest request, ClaimsPrincipal user);

}
