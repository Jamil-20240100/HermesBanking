using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.DTOs.Commerce
{
    public class CommerceDTO
    {
        public int Id { get; set; }  // Identificador único del comercio
        public string Name { get; set; }  // Nombre del comercio
        public string Address { get; set; }  // Dirección del comercio
        public string Phone { get; set; }  // Teléfono del comercio
        public string Email { get; set; }  // Correo electrónico del comercio
        public bool IsActive { get; set; }  // Estado del comercio (activo/inactivo)
        public DateTime CreatedAt { get; set; }  // Fecha de creación del comercio

        public string? UserId { get; set; }
    }
}
