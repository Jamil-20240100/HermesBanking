using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.DTOs.Commerce
{
    public class UpdateCommerceForApiDTO
    {
        
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
