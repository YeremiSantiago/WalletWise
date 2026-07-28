using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;

namespace WalletWise.Application.Dtos.Category
{
    public class CreateCategoryRequestDto
    {
        public string Name { get; set;  }
        public TypeTransaction Type { get; set; }
        public string? Description { get; set; }
    }
}
