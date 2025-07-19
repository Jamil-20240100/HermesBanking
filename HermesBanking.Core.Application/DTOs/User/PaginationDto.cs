using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.DTOs.User
{
    public class PaginationDto<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationInfoDto Paginacion { get; set; } = new();
    }
}
