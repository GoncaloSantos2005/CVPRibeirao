using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.Application.Interfaces.IUtilizadorServices
{
    public interface IAuthService
    {
        Task<UtilizadorDto?> RegistarAsync(RegistarUtilizadorDto dto);
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    }
}
