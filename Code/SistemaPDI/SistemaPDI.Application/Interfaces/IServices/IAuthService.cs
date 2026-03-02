using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.Application.Interfaces.IUtilizadorServices
{
    /// <summary>
    /// Interface do serviço de autenticação.
    /// Define operações de login e registo.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>Regista um novo utilizador no sistema.</summary>
        Task<UtilizadorDto?> RegistarAsync(RegistarUtilizadorDto dto);

        /// <summary>Autentica um utilizador com email e password.</summary>
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    }
}
