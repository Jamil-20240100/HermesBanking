using HermesBanking.Core.Application.DTOs.Commerce;
using HermesBanking.Core.Application.DTOs.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ICommerceService
    {
        // Método para obtener comercios paginados
        Task<CommercePagedResponseDTO> GetAllCommercesAsync(int page, int pageSize);

        // Método para obtener un comercio por ID
        Task<CommerceDTO> GetCommerceByIdAsync(int id);

        // Método para crear un nuevo comercio
        Task<CommerceDTO> CreateCommerceAsync(CreateCommerceForApiDTO dto);

        // Método para actualizar un comercio existente
        Task<CommerceDTO> UpdateCommerceAsync(int id, UpdateCommerceForApiDTO dto);

        // Método para cambiar el estado de un comercio
        Task<bool> ChangeCommerceStatusAsync(int id, ChangeCommerceStatusDTO dto);

        // Método para obtener usuarios asociados a un comercio
        Task<List<UserDto>> GetUsersByCommerceIdAsync(int commerceId);
    }
}
