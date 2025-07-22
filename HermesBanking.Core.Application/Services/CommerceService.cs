using AutoMapper;
using HermesBanking.Core.Application.DTOs.Commerce;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Services
{
    public class CommerceService : ICommerceService
    {
        private readonly ICommerceRepository _commerceRepository;
        private readonly IMapper _mapper;
        private readonly IAccountServiceForWebApi _accountServiceForWebApi;

        public CommerceService(ICommerceRepository commerceRepository, IMapper mapper, IAccountServiceForWebApi accountServiceForWebApi)
        {
            _commerceRepository = commerceRepository;
            _mapper = mapper;
            _accountServiceForWebApi = accountServiceForWebApi;
        }

        // Obtener todos los comercios
        public async Task<CommercePagedResponseDTO> GetAllCommercesAsync(int page, int pageSize)
        {
            // Obtener todos los comercios desde el repositorio
            var commerces = await _commerceRepository.GetAllCommercesAsync();

            // Aquí manejamos la paginación
            var totalRecords = commerces.Count;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var pagedCommerces = commerces.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Mapeo usando AutoMapper
            var pagedCommercesDTO = _mapper.Map<List<CommerceDTO>>(pagedCommerces);

            // Devolvemos la respuesta paginada
            return new CommercePagedResponseDTO
            {
                Data = pagedCommercesDTO,  // Usamos la lista mapeada de CommerceDTO
                Paginacion = new PaginationDTO
                {
                    PaginaActual = page,
                    TotalPaginas = totalPages,
                    TotalRegistros = totalRecords
                }
            };
        }

        // Obtener un comercio por su ID
        public async Task<CommerceDTO> GetCommerceByIdAsync(int id)
        {
            var commerce = await _commerceRepository.GetCommerceByIdAsync(id);
            if (commerce == null)
            {
                return null;  // Devuelve null si no se encuentra el comercio
            }
            return new CommerceDTO
            {
                Id = commerce.Id,
                Name = commerce.Name,
                Address = commerce.Address,
                Phone = commerce.Phone,
                Email = commerce.Email,
                IsActive = commerce.IsActive
            };
        }

        // Crear un nuevo comercio
        public async Task<CommerceDTO> CreateCommerceAsync(CreateCommerceForApiDTO dto)
        {
            // Verifica si ya existe un comercio con el mismo nombre y usuario
            var existingCommerce = await _commerceRepository.GetCommerceByNameAndUserIdAsync(dto.Name, dto.UserId);
            if (existingCommerce != null)
            {
                throw new Exception("El comercio ya existe.");
            }

            var commerce = new Commerce
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                IsActive = true,
                UserId = dto.UserId,
                CreatedAt = DateTime.Now  // Añadido para mantener la fecha de creación
            };

            await _commerceRepository.AddCommerceAsync(commerce);


            return new CommerceDTO
            {
                Id = commerce.Id,
                Name = commerce.Name,
                Address = commerce.Address,
                Phone = commerce.Phone,
                Email = commerce.Email,
                IsActive = commerce.IsActive,
                UserId = dto.UserId
            };
        }

        // Actualizar un comercio
        public async Task<CommerceDTO> UpdateCommerceAsync(int id, UpdateCommerceForApiDTO dto)  // Cambiar a 'int'
        {
            var commerce = await _commerceRepository.GetCommerceByIdAsync(id);  // Usar 'id' como int
            if (commerce == null)
                throw new Exception("Comercio no encontrado");

            commerce.Name = dto.Name;
            commerce.Address = dto.Address;
            commerce.Phone = dto.Phone;
            commerce.Email = dto.Email;
            commerce.IsActive = dto.IsActive;

            await _commerceRepository.UpdateCommerceAsync(commerce);

            return new CommerceDTO
            {
                Id = commerce.Id,
                Name = commerce.Name,
                Address = commerce.Address,
                Phone = commerce.Phone,
                Email = commerce.Email,
                IsActive = commerce.IsActive,
                UserId = commerce.UserId
            };
        }


        public async Task<bool> ChangeCommerceStatusAsync(int id, ChangeCommerceStatusDTO dto)  // Cambiar a 'int'
        {
            var commerce = await _commerceRepository.GetCommerceByIdAsync(id);  // Usar 'id' como int
            if (commerce == null)
                throw new Exception("Comercio no encontrado");

            commerce.IsActive = dto.Status;
            await _commerceRepository.UpdateCommerceAsync(commerce);
            return true;
        }

        // Obtener los usuarios asociados a un comercio
        public async Task<List<UserDto>> GetUsersByCommerceIdAsync(int commerceId)
        {
            // Convertimos commerceId a string
            return await _accountServiceForWebApi.GetUsersByCommerceIdAsync(commerceId.ToString());
        }
    }
}
