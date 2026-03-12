using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Domain.Enums
{
    /// <summary>
    /// Perfis/roles de utilizador no sistema.
    /// </summary>
    public enum Perfil
    {
        /// <summary>Socorrista - acesso básico de consulta.</summary>
        SOCORRISTA = 1,

        /// <summary>Gestor - gestão de artigos e categorias.</summary>
        GESTOR = 2,

        /// <summary>Gestor Financeiro - gestão de encomendas e aprovações.</summary>
        GESTOR_FINANCEIRO = 3,

        /// <summary>Administrador - acesso total ao sistema.</summary>
        ADMINISTRADOR = 4
    }
}
