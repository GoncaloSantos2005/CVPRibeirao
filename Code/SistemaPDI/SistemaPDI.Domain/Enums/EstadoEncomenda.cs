namespace SistemaPDI.Domain.Enums
{
    /// <summary>
    /// Estados do ciclo de vida de uma encomenda
    /// </summary>
    public enum EstadoEncomenda
    {
        /// <summary>Lista de necessidades - só artigos e quantidades</summary>
        LISTA = 0,

        /// <summary>PDF gerado, enviado a fornecedores, aguarda orçamentos</summary>
        RASCUNHO = 1,

        /// <summary>Orçamento anexado, aguarda aprovação do Gestor Logística</summary>
        PENDENTE = 2,

        /// <summary>Aprovada, Gestor Logística está a preencher preços e detalhes</summary>
        CONFIRMADA = 3,

        /// <summary>Enviada ao fornecedor, aguarda entrega física</summary>
        ENVIADA = 4,

        /// <summary>Receção parcial iniciada</summary>
        PARCIAL = 5,

        /// <summary>Totalmente recepcionada e validada</summary>
        CONCLUIDA = 6,

        /// <summary>Cancelada</summary>
        CANCELADA = 7
    }
}