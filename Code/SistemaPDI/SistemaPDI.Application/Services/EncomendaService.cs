using Microsoft.AspNetCore.Http;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Application.Services
{
    public class EncomendaService : IEncomendaService
    {
        private readonly IEncomendaRepository _encomendaRepository;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IArtigoRepository _artigoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly ILocalizacaoRepository _localizacaoRepository;
        private readonly IHistoricoPrecoRepository _historicoPrecoRepository;
        private readonly IHistoricoPrecoService _historicoPrecoService;

        public EncomendaService(
            IEncomendaRepository encomendaRepository,
            IFornecedorRepository fornecedorRepository,
            IArtigoRepository artigoRepository,
            ILoteRepository loteRepository,
            ILocalizacaoRepository localizacaoRepository,
            IHistoricoPrecoRepository historicoPrecoRepository,
            IHistoricoPrecoService historicoPrecoService)
        {
            _encomendaRepository = encomendaRepository;
            _fornecedorRepository = fornecedorRepository;
            _artigoRepository = artigoRepository;
            _loteRepository = loteRepository;
            _localizacaoRepository = localizacaoRepository;
            _historicoPrecoRepository = historicoPrecoRepository;
            _historicoPrecoService = historicoPrecoService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<List<EncomendaDto>>> ObterTodosAsync(bool incluirInativos = false)
        {
            var encomendas = await _encomendaRepository.ObterTodosAsync(incluirInativos);
            var dtos = encomendas.Select(MapearParaDto).ToList();
            return Result<List<EncomendaDto>>.Ok(dtos);
        }

        public async Task<Result<EncomendaDto>> ObterPorIdAsync(int id)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            return Result<EncomendaDto>.Ok(MapearParaDto(encomenda));
        }

        public async Task<Result<List<EncomendaDto>>> ObterPorEstadoAsync(string estado)
        {
            if (!Enum.TryParse<EstadoEncomenda>(estado, out var estadoEnum))
                return Result<List<EncomendaDto>>.Falhou("Estado inválido");

            var encomendas = await _encomendaRepository.ObterPorEstadoAsync(estadoEnum);
            var dtos = encomendas.Select(MapearParaDto).ToList();
            return Result<List<EncomendaDto>>.Ok(dtos);
        }

        public async Task<Result<List<EncomendaDto>>> ObterMinhasEncomendasAsync(string emailUtilizador)
        {
            var todasEncomendas = await _encomendaRepository.ObterTodosAsync(incluirInativos: false);
            var minhasEncomendas = todasEncomendas
                .Where(e => e.CriadoPor == emailUtilizador)
                .OrderByDescending(e => e.DataCriacao)
                .ToList();

            var dtos = minhasEncomendas.Select(MapearParaDto).ToList();
            return Result<List<EncomendaDto>>.Ok(dtos);
        }

        public async Task<Result<List<EncomendaDto>>> ObterPendentesAprovacaoAsync()
        {
            var encomendas = await _encomendaRepository.ObterPorEstadoAsync(EstadoEncomenda.PENDENTE);
            var dtos = encomendas.Select(MapearParaDto).ToList();
            return Result<List<EncomendaDto>>.Ok(dtos);
        }

        public async Task<Result<List<EncomendaDto>>> ObterEncomendasEnviadasAsync()
        {
            var enviadas = await _encomendaRepository.ObterPorEstadoAsync(EstadoEncomenda.ENVIADA);
            var parciais = await _encomendaRepository.ObterPorEstadoAsync(EstadoEncomenda.PARCIAL);

            var todas = enviadas.Concat(parciais).OrderBy(e => e.DataEnvioFornecedor).ToList();
            var dtos = todas.Select(MapearParaDto).ToList();
            return Result<List<EncomendaDto>>.Ok(dtos);
        }

        public async Task<Result<List<EncomendaDropdownDto>>> ObterPendentesParaDropdownAsync()
        {
            var pendentes = await _encomendaRepository.ObterPendentesAsync();

            var dtos = pendentes.Select(e => new EncomendaDropdownDto(
                e.Id,
                e.NumeroEncomenda,
                e.Fornecedor?.Nome,
                e.Estado.ToString()
            )).ToList();

            return Result<List<EncomendaDropdownDto>>.Ok(dtos);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 1: CRIAR LISTA (Estado: LISTA)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<EncomendaDto>> CriarListaAsync(CriarListaDto dto, string emailUtilizador)
        {
            // Validações
            if (dto.Linhas == null || !dto.Linhas.Any())
                return Result<EncomendaDto>.Falhou("A lista deve ter pelo menos um artigo");

            // Validar artigos
            foreach (var linha in dto.Linhas)
            {
                var artigo = await _artigoRepository.ObterPorIdAsync(linha.ArtigoId);
                if (artigo == null)
                    return Result<EncomendaDto>.Falhou($"Artigo com ID {linha.ArtigoId} não encontrado");

                if (!artigo.Ativo)
                    return Result<EncomendaDto>.Falhou($"Artigo '{artigo.Nome}' está inativo");

                if (linha.QuantidadeEncomendada <= 0)
                    return Result<EncomendaDto>.Falhou($"Quantidade do artigo '{artigo.Nome}' deve ser maior que zero");
            }

            // Criar encomenda
            var numeroEncomenda = await _encomendaRepository.GerarProximoNumeroAsync();

            var encomenda = new Encomenda
            {
                NumeroEncomenda = numeroEncomenda,
                DataCriacao = DateTime.UtcNow,
                Estado = EstadoEncomenda.LISTA,
                Observacoes = dto.Observacoes?.Trim(),
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                CriadoPor = emailUtilizador
            };

            // Criar linhas
            foreach (var linhaDto in dto.Linhas)
            {
                var linha = new LinhaEncomenda
                {
                    ArtigoId = linhaDto.ArtigoId,
                    QuantidadeEncomendada = linhaDto.QuantidadeEncomendada,
                    QuantidadeAprovada = 0,
                    QuantidadeRecebida = 0
                };

                encomenda.Linhas.Add(linha);
            }

            await _encomendaRepository.AdicionarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            // Recarregar com relacionamentos
            var encomendaCriada = await _encomendaRepository.ObterPorIdAsync(encomenda.Id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaCriada!));
        }

        // O método AtualizarListaAsync fica assim (a versão que já tens):

        public async Task<Result<EncomendaDto>> AtualizarListaAsync(int id, CriarListaDto dto)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            // Só pode editar se LISTA
            if (encomenda.Estado != EstadoEncomenda.LISTA)
                return Result<EncomendaDto>.Falhou("Só é possível editar listas no estado LISTA");

            // Validar linhas
            if (dto.Linhas == null || !dto.Linhas.Any())
                return Result<EncomendaDto>.Falhou("A lista deve ter pelo menos um artigo");

            foreach (var linha in dto.Linhas)
            {
                var artigo = await _artigoRepository.ObterPorIdAsync(linha.ArtigoId);
                if (artigo == null)
                    return Result<EncomendaDto>.Falhou($"Artigo com ID {linha.ArtigoId} não encontrado");

                if (!artigo.Ativo)
                    return Result<EncomendaDto>.Falhou($"Artigo '{artigo.Nome}' está inativo");

                if (linha.QuantidadeEncomendada <= 0)
                    return Result<EncomendaDto>.Falhou($"Quantidade do artigo '{artigo.Nome}' deve ser maior que zero");
            }

            // Atualizar observações
            encomenda.Observacoes = dto.Observacoes?.Trim();
            encomenda.AtualizadoEm = DateTime.UtcNow;

            // LIMPAR campos de rejeição (a lista foi corrigida)
            encomenda.MotivoRejeicao = null;
            encomenda.RejeitadoPor = null;
            encomenda.RejeitadoEm = null;

            // Limpar campos de submissão anterior (volta ao início do fluxo)
            encomenda.CaminhoOrcamentoPdf = null;
            encomenda.ValorOrcamento = null;
            encomenda.SubmetidoEm = null;
            encomenda.SubmetidoPor = null;

            // Remover linhas antigas e adicionar novas
            encomenda.Linhas.Clear();

            foreach (var linhaDto in dto.Linhas)
            {
                var linha = new LinhaEncomenda
                {
                    ArtigoId = linhaDto.ArtigoId,
                    QuantidadeEncomendada = linhaDto.QuantidadeEncomendada,
                    QuantidadeAprovada = 0,
                    QuantidadeRecebida = 0
                };

                encomenda.Linhas.Add(linha);
            }

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 2: GERAR PDF E MARCAR COMO RASCUNHO (LISTA → RASCUNHO)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<byte[]>> GerarPdfListaAsync(int id)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<byte[]>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.LISTA)
                return Result<byte[]>.Falhou("Só é possível gerar PDF de listas no estado LISTA");

            // Configurar licença (Community é gratuita para uso comercial até certo volume)
            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Cabeçalho
                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("CVP Ribeirão - Sistema PDI")
                                    .FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                                col.Item().Text("Lista de Encomenda")
                                    .FontSize(12).SemiBold();
                            });

                            row.ConstantItem(150).Column(col =>
                            {
                                col.Item().AlignRight().Text($"Nº: {encomenda.NumeroEncomenda}")
                                    .FontSize(14).Bold();
                                col.Item().AlignRight().Text($"Data: {encomenda.DataCriacao:dd/MM/yyyy}");
                            });
                        });
                    });

                    // Conteúdo
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // Info geral
                        col.Item().PaddingBottom(10).Text($"Criado por: {encomenda.CriadoPor ?? "N/A"}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);

                        if (!string.IsNullOrEmpty(encomenda.Observacoes))
                        {
                            col.Item().PaddingBottom(10).Text($"Observações: {encomenda.Observacoes}")
                                .FontSize(9).Italic();
                        }

                        // Tabela de artigos
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);   // #
                                columns.ConstantColumn(80);   // SKU
                                columns.RelativeColumn();     // Nome
                                columns.ConstantColumn(80);   // Qtd
                            });

                            // Cabeçalho da tabela
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Red.Darken2).Padding(5)
                                    .Text("#").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Red.Darken2).Padding(5)
                                    .Text("SKU").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Red.Darken2).Padding(5)
                                    .Text("Artigo").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Red.Darken2).Padding(5)
                                    .Text("Quantidade").FontColor(Colors.White).Bold().AlignRight();
                            });

                            // Linhas
                            var index = 1;
                            foreach (var linha in encomenda.Linhas)
                            {
                                var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                table.Cell().Background(bgColor).Padding(5).Text(index.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(linha.Artigo?.SKU ?? "N/A");
                                table.Cell().Background(bgColor).Padding(5).Text(linha.Artigo?.Nome ?? "N/A");
                                table.Cell().Background(bgColor).Padding(5).AlignRight()
                                    .Text(linha.QuantidadeEncomendada.ToString());

                                index++;
                            }
                        });

                        // Resumo
                        col.Item().PaddingTop(20).AlignRight().Text($"Total de Artigos: {encomenda.Linhas.Count}")
                            .Bold();
                        col.Item().AlignRight().Text($"Total de Unidades: {encomenda.Linhas.Sum(l => l.QuantidadeEncomendada)}")
                            .Bold();
                    });

                    // Rodapé
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                        text.Span($" | Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
                });
            }).GeneratePdf();

            return Result<byte[]>.Ok(pdfBytes);
        }

        public async Task<Result<EncomendaDto>> MarcarComoRascunhoAsync(int id, string emailUtilizador)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.LISTA)
                return Result<EncomendaDto>.Falhou("Só é possível marcar como rascunho listas no estado LISTA");

            if (!encomenda.Linhas.Any())
                return Result<EncomendaDto>.Falhou("Encomenda deve ter pelo menos um artigo");

            encomenda.Estado = EstadoEncomenda.RASCUNHO;
            encomenda.GeradoPdfEm = DateTime.UtcNow;
            encomenda.GeradoPdfPor = emailUtilizador;
            encomenda.AtualizadoEm = DateTime.UtcNow;

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 3: SUBMETER ORÇAMENTO (RASCUNHO → PENDENTE)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<EncomendaDto>> SubmeterOrcamentoAsync(
            int id,
            SubmeterOrcamentoDto dto,
            IFormFile orcamentoPdf,
            string emailUtilizador)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.RASCUNHO)
                return Result<EncomendaDto>.Falhou("Só é possível submeter orçamento para encomendas em RASCUNHO");

            if (orcamentoPdf == null || orcamentoPdf.Length == 0)
                return Result<EncomendaDto>.Falhou("O arquivo PDF do orçamento é obrigatório");

            if (dto.ValorOrcamento <= 0)
                return Result<EncomendaDto>.Falhou("Valor do orçamento deve ser maior que zero");

            // Exemplo de armazenamento do PDF (ajuste conforme sua lógica)
            var caminhoPdf = $"orcamentos/{Guid.NewGuid()}_{orcamentoPdf.FileName}";
            // TODO: Salvar o arquivo orcamentoPdf no caminhoPdf

            encomenda.Estado = EstadoEncomenda.PENDENTE;
            encomenda.CaminhoOrcamentoPdf = caminhoPdf;
            encomenda.ValorOrcamento = dto.ValorOrcamento;
            encomenda.SubmetidoEm = DateTime.UtcNow;
            encomenda.SubmetidoPor = emailUtilizador;
            encomenda.AtualizadoEm = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Observacoes))
                encomenda.Observacoes = dto.Observacoes.Trim();

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        /// <summary>
        /// Versão simplificada que aceita apenas o caminho do PDF já guardado
        /// </summary>
        public async Task<Result<EncomendaDto>> SubmeterOrcamentoSimplesAsync(
            int id,
            SubmeterOrcamentoDto dto,
            string emailUtilizador)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.RASCUNHO)
                return Result<EncomendaDto>.Falhou("Só é possível submeter orçamento para encomendas em RASCUNHO");

            if (string.IsNullOrEmpty(dto.CaminhoOrcamentoPdf))
                return Result<EncomendaDto>.Falhou("Caminho do ficheiro PDF é obrigatório");

            if (dto.ValorOrcamento <= 0)
                return Result<EncomendaDto>.Falhou("Valor do orçamento deve ser maior que zero");

            encomenda.Estado = EstadoEncomenda.PENDENTE;
            encomenda.CaminhoOrcamentoPdf = dto.CaminhoOrcamentoPdf;
            encomenda.ValorOrcamento = dto.ValorOrcamento;
            encomenda.SubmetidoEm = DateTime.UtcNow;
            encomenda.SubmetidoPor = emailUtilizador;
            encomenda.AtualizadoEm = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Observacoes))
                encomenda.Observacoes = dto.Observacoes.Trim();

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 4: REJEITAR (PENDENTE → LISTA)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<EncomendaDto>> RejeitarAsync(int id, string emailGestorLogistica, string motivo)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.PENDENTE)
                return Result<EncomendaDto>.Falhou("Só é possível rejeitar encomendas PENDENTES");

            if (string.IsNullOrWhiteSpace(motivo))
                return Result<EncomendaDto>.Falhou("Motivo da rejeição é obrigatório");

            // Volta para LISTA para correção
            encomenda.Estado = EstadoEncomenda.LISTA;
            encomenda.RejeitadoEm = DateTime.UtcNow;
            encomenda.RejeitadoPor = emailGestorLogistica;
            encomenda.MotivoRejeicao = motivo.Trim();
            encomenda.AtualizadoEm = DateTime.UtcNow;

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            // TODO: Notificar criador

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 5: APROVAR E PREENCHER (PENDENTE → CONFIRMADA)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<EncomendaDto>> AprovarEPreencherAsync(
            int id,
            AprovarEPreencherDto dto,
            string emailGestorLogistica)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.PENDENTE)
                return Result<EncomendaDto>.Falhou("Só é possível aprovar encomendas PENDENTES");

            // Validar fornecedor
            var fornecedor = await _fornecedorRepository.ObterPorIdAsync(dto.FornecedorId);
            if (fornecedor == null)
                return Result<EncomendaDto>.Falhou("Fornecedor não encontrado");

            if (!fornecedor.Ativo)
                return Result<EncomendaDto>.Falhou("Fornecedor está inativo");

            // Validar linhas
            if (dto.Linhas == null || !dto.Linhas.Any())
                return Result<EncomendaDto>.Falhou("Deve preencher todas as linhas");

            if (dto.Linhas.Count != encomenda.Linhas.Count)
                return Result<EncomendaDto>.Falhou("Número de linhas não coincide");

            decimal valorTotal = 0;

            foreach (var linhaDto in dto.Linhas)
            {
                var linha = encomenda.Linhas.FirstOrDefault(l => l.Id == linhaDto.LinhaId);
                if (linha == null)
                    return Result<EncomendaDto>.Falhou($"Linha {linhaDto.LinhaId} não encontrada");

                if (linhaDto.QuantidadeAprovada <= 0)
                    return Result<EncomendaDto>.Falhou("Quantidade aprovada do artigo deve ser maior que zero");

                if (linhaDto.PrecoUnitario < 0)
                    return Result<EncomendaDto>.Falhou("Preço unitário não pode ser negativo");

                // Atualizar linha
                linha.QuantidadeAprovada = linhaDto.QuantidadeAprovada;
                linha.PrecoUnitario = linhaDto.PrecoUnitario;
                linha.Subtotal = linhaDto.QuantidadeAprovada * linhaDto.PrecoUnitario;
                linha.NumeroLote = linhaDto.NumeroLote?.Trim();
                linha.DataValidade = linhaDto.DataValidade;

                valorTotal += linha.Subtotal.Value;
            }

            // Atualizar encomenda
            encomenda.Estado = EstadoEncomenda.CONFIRMADA;
            encomenda.FornecedorId = dto.FornecedorId;
            encomenda.DataEntregaPrevista = dto.DataEntregaPrevista;
            encomenda.ValorTotal = valorTotal;
            encomenda.ObservacoesInternas = dto.ObservacoesInternas?.Trim();
            encomenda.AprovadoEm = DateTime.UtcNow;
            encomenda.AprovadoPor = emailGestorLogistica;
            encomenda.AtualizadoEm = DateTime.UtcNow;

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 6: CONFIRMAR E ENVIAR (CONFIRMADA → ENVIADA)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<EncomendaDto>> ConfirmarEEnviarAsync(int id, string emailGestorLogistica)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.CONFIRMADA)
                return Result<EncomendaDto>.Falhou("Só é possível enviar encomendas CONFIRMADAS");

            // Validar se todas as linhas têm preço
            if (encomenda.Linhas.Any(l => !l.PrecoUnitario.HasValue || l.PrecoUnitario <= 0))
                return Result<EncomendaDto>.Falhou("Todas as linhas devem ter preço unitário preenchido");

            // ══════════════════════════════════════════════════════════════════════
            // ✅ CRIAR LOTES EM TRÂNSITO E ATUALIZAR STOCK VIRTUAL
            // ══════════════════════════════════════════════════════════════════════

            foreach (var linha in encomenda.Linhas.Where(l => l.QuantidadeAprovada > 0))
            {
                var artigo = await _artigoRepository.ObterPorIdAsync(linha.ArtigoId);
                if (artigo == null)
                    return Result<EncomendaDto>.Falhou($"Artigo {linha.ArtigoId} não encontrado");

                // ✅ CRIAR LOTE EM TRÂNSITO
                var loteEmTrafico = new Lote
                {
                    ArtigoId = linha.ArtigoId,
                    EmTrafico = true, // ⬅️ FLAG: Este lote está a caminho

                    // NULL (preenchidos na receção)
                    NumeroLote = null,
                    DataValidade = null,
                    LocalizacaoId = null,

                    // Preço já conhecemos (aprovação)
                    PrecoUnitario = linha.PrecoUnitario,

                    // QUANTIDADES
                    QtdDisponivel = 0,                      // ⬅️ ZERO (não chegou fisicamente)
                    QtdReservada = linha.QuantidadeAprovada, // ⬅️ COMPROMETIDA (esperada)

                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                };

                await _loteRepository.AdicionarAsync(loteEmTrafico);
                await _loteRepository.SaveChangesAsync();

                // Associar lote à linha (para encontrar depois)
                linha.LoteId = loteEmTrafico.Id;

                // ✅ ATUALIZAR STOCK VIRTUAL DO ARTIGO
                artigo.StockVirtual += linha.QuantidadeAprovada;
                await _artigoRepository.AtualizarAsync(artigo);
            }

            // Mudar estado da encomenda
            encomenda.Estado = EstadoEncomenda.ENVIADA;
            encomenda.DataEnvioFornecedor = DateTime.UtcNow;
            encomenda.ConfirmadaEm = DateTime.UtcNow;
            encomenda.ConfirmadaPor = emailGestorLogistica;
            encomenda.AtualizadoEm = DateTime.UtcNow;

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            // TODO: Enviar encomenda ao fornecedor (email/PDF)

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(id);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 7: REGISTAR RECEÇÃO (ENVIADA → PARCIAL/CONCLUIDA)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<EncomendaDto>> RegistarRecepcaoAsync(
            RegistarRecepcaoDto dto,
            string emailUtilizador)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(dto.EncomendaId);

            if (encomenda == null)
                return Result<EncomendaDto>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != EstadoEncomenda.ENVIADA && encomenda.Estado != EstadoEncomenda.PARCIAL)
                return Result<EncomendaDto>.Falhou("Só é possível recepcionar encomendas ENVIADAS ou PARCIAIS");

            // ══════════════════════════════════════════════════════════════════════
            // ✅ PROCESSAR CADA LINHA RECEBIDA
            // ══════════════════════════════════════════════════════════════════════

            foreach (var linhaDto in dto.Linhas)
            {
                var linha = encomenda.Linhas.FirstOrDefault(l => l.Id == linhaDto.LinhaId);
                if (linha == null)
                    continue;

                // Validar localização
                var localizacao = await _localizacaoRepository.ObterPorIdAsync(linhaDto.LocalizacaoId);
                if (localizacao == null)
                    return Result<EncomendaDto>.Falhou($"Localização {linhaDto.LocalizacaoId} não encontrada");

                var artigo = await _artigoRepository.ObterPorIdAsync(linha.ArtigoId);
                if (artigo == null)
                    return Result<EncomendaDto>.Falhou($"Artigo {linha.ArtigoId} não encontrado");

                // ══════════════════════════════════════════════════════════════════
                // ✅ PROCURAR LOTE EM TRÂNSITO ASSOCIADO A ESTA LINHA
                // ══════════════════════════════════════════════════════════════════

                Lote? loteEmTrafico = null;

                if (linha.LoteId.HasValue)
                {
                    loteEmTrafico = await _loteRepository.ObterPorIdAsync(linha.LoteId.Value);

                    // Validar se realmente é um lote em trânsito
                    if (loteEmTrafico != null && !loteEmTrafico.EmTrafico)
                        loteEmTrafico = null; // Não é lote em trânsito, ignorar
                }

                if (loteEmTrafico != null && loteEmTrafico.EmTrafico)
                {
                    // ══════════════════════════════════════════════════════════════
                    // ✅ CENÁRIO A: CONVERTER LOTE EM TRÂNSITO → LOTE FÍSICO
                    // ══════════════════════════════════════════════════════════════

                    // Preencher dados que faltavam
                    loteEmTrafico.NumeroLote = linhaDto.NumeroLote.Trim();
                    loteEmTrafico.DataValidade = linhaDto.DataValidade;
                    loteEmTrafico.LocalizacaoId = linhaDto.LocalizacaoId;

                    // ✅ ATUALIZAR QUANTIDADES (conforme tua visão)
                    loteEmTrafico.QtdDisponivel = linhaDto.QuantidadeRecebida; // ⬅️ Físico
                    loteEmTrafico.QtdReservada = 0;                            // ⬅️ Liberar

                    // ✅ MUDAR FLAG - Agora é lote físico
                    loteEmTrafico.EmTrafico = false;

                    await _loteRepository.AtualizarAsync(loteEmTrafico);

                    // ✅ ATUALIZAR STOCKS DO ARTIGO (Virtual → Físico)
                    artigo.StockVirtual -= linhaDto.QuantidadeRecebida;
                    artigo.StockFisico += linhaDto.QuantidadeRecebida;
                }
                else
                {
                    // ══════════════════════════════════════════════════════════════
                    // ✅ CENÁRIO B: CRIAR NOVO LOTE FÍSICO
                    // (Se não havia lote em trânsito associado)
                    // ══════════════════════════════════════════════════════════════

                    // Verificar se já existe lote com este número
                    var loteExistente = await _loteRepository.ObterPorNumeroAsync(linhaDto.NumeroLote);

                    if (loteExistente == null)
                    {
                        // Criar novo lote físico
                        var novoLote = new Lote
                        {
                            ArtigoId = linha.ArtigoId,
                            EmTrafico = false, // ⬅️ Lote físico direto

                            NumeroLote = linhaDto.NumeroLote.Trim(),
                            DataValidade = linhaDto.DataValidade,
                            LocalizacaoId = linhaDto.LocalizacaoId,
                            PrecoUnitario = linha.PrecoUnitario,

                            QtdDisponivel = linhaDto.QuantidadeRecebida,
                            QtdReservada = 0,

                            Ativo = true,
                            CriadoEm = DateTime.UtcNow
                        };

                        await _loteRepository.AdicionarAsync(novoLote);
                        await _loteRepository.SaveChangesAsync();

                        linha.LoteId = novoLote.Id;
                    }
                    else
                    {
                        // Adicionar quantidade ao lote existente
                        loteExistente.QtdDisponivel += linhaDto.QuantidadeRecebida;
                        loteExistente.Ativo = true;
                        await _loteRepository.AtualizarAsync(loteExistente);

                        linha.LoteId = loteExistente.Id;
                    }

                    // Neste cenário, não havia stock virtual, então só incrementa físico
                    artigo.StockFisico += linhaDto.QuantidadeRecebida;
                }

                // ══════════════════════════════════════════════════════════════════
                // ✅ ATUALIZAR LINHA DA ENCOMENDA
                // ══════════════════════════════════════════════════════════════════

                linha.QuantidadeRecebida += linhaDto.QuantidadeRecebida;
                linha.NumeroLote = linhaDto.NumeroLote.Trim();
                linha.DataValidade = linhaDto.DataValidade;
                linha.LocalizacaoId = linhaDto.LocalizacaoId;

                if (!string.IsNullOrWhiteSpace(linhaDto.Observacoes))
                    linha.Observacoes = linhaDto.Observacoes.Trim();

                // Salvar artigo atualizado
                await _artigoRepository.AtualizarAsync(artigo);
            }

            // ══════════════════════════════════════════════════════════════════════
            // ✅ VERIFICAR SE ENCOMENDA ESTÁ CONCLUÍDA
            // ══════════════════════════════════════════════════════════════════════

            var totalAprovado = encomenda.Linhas.Sum(l => l.QuantidadeAprovada);
            var totalRecebido = encomenda.Linhas.Sum(l => l.QuantidadeRecebida);

            if (totalRecebido >= totalAprovado)
            {
                // ✅ ENCOMENDA CONCLUÍDA
                encomenda.Estado = EstadoEncomenda.CONCLUIDA;
                encomenda.DataEntregaReal = DateTime.UtcNow;

                await _encomendaRepository.AtualizarAsync(encomenda);
                await _encomendaRepository.SaveChangesAsync();

                // ✅ REGISTAR HISTÓRICO DE PREÇOS
                await _historicoPrecoService.RegistarHistoricoDeEncomendaAsync(
                    dto.EncomendaId,
                    emailUtilizador
                );
            }
            else
            {
                // Receção parcial
                encomenda.Estado = EstadoEncomenda.PARCIAL;
            }

            encomenda.AtualizadoEm = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Observacoes))
            {
                encomenda.Observacoes = string.IsNullOrEmpty(encomenda.Observacoes)
                    ? dto.Observacoes.Trim()
                    : $"{encomenda.Observacoes}\n{dto.Observacoes.Trim()}";
            }

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            var encomendaAtualizada = await _encomendaRepository.ObterPorIdAsync(dto.EncomendaId);
            return Result<EncomendaDto>.Ok(MapearParaDto(encomendaAtualizada!));
        }

        // ══════════════════════════════════════════════════════════════════════
        // OUTRAS OPERAÇÕES
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<bool>> CancelarAsync(int id, string motivo)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<bool>.Falhou("Encomenda não encontrada");

            // Só pode cancelar se LISTA, RASCUNHO ou PENDENTE
            if (encomenda.Estado != EstadoEncomenda.LISTA &&
                encomenda.Estado != EstadoEncomenda.RASCUNHO &&
                encomenda.Estado != EstadoEncomenda.PENDENTE)
                return Result<bool>.Falhou("Só é possível cancelar encomendas em LISTA, RASCUNHO ou PENDENTE");

            encomenda.Estado = EstadoEncomenda.CANCELADA;
            encomenda.MotivoRejeicao = motivo?.Trim();
            encomenda.AtualizadoEm = DateTime.UtcNow;

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> AlterarEstadoAsync(int id, bool ativo)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(id);

            if (encomenda == null)
                return Result<bool>.Falhou("Encomenda não encontrada");

            encomenda.Ativo = ativo;
            encomenda.AtualizadoEm = DateTime.UtcNow;

            await _encomendaRepository.AtualizarAsync(encomenda);
            await _encomendaRepository.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }

        // ══════════════════════════════════════════════════════════════════════
        // MAPEAMENTO
        // ══════════════════════════════════════════════════════════════════════

        private EncomendaDto MapearParaDto(Encomenda e)
        {
            var totalRequisitados = e.Linhas.Sum(l => l.QuantidadeEncomendada);
            var totalAprovados = e.Linhas.Sum(l => l.QuantidadeAprovada);
            var totalRecebidos = e.Linhas.Sum(l => l.QuantidadeRecebida);
            var percentualRecebido = totalAprovados > 0
                ? (decimal)totalRecebidos / totalAprovados * 100
                : 0;

            var linhas = e.Linhas.Select(l => new LinhaEncomendaDto(
                l.Id,
                l.ArtigoId,
                l.Artigo?.Nome ?? "N/A",
                l.Artigo?.SKU ?? "N/A",
                l.QuantidadeEncomendada,
                l.QuantidadeAprovada,
                l.QuantidadeRecebida,
                l.QuantidadeAprovada - l.QuantidadeRecebida,
                l.PrecoUnitario,
                l.Subtotal,
                l.NumeroLote,
                l.DataValidade,
                l.LocalizacaoId,
                l.Localizacao?.Codigo,
                l.Observacoes
            )).ToList();

            return new EncomendaDto(
                e.Id,
                e.NumeroEncomenda,
                e.DataCriacao,
                e.DataEnvioFornecedor,
                e.DataEntregaPrevista,
                e.DataEntregaReal,
                e.Estado.ToString(),
                e.FornecedorId,
                e.Fornecedor?.Nome,
                e.CaminhoOrcamentoPdf,
                e.ValorOrcamento,
                e.ValorTotal,
                e.Observacoes,
                e.ObservacoesInternas,
                e.MotivoRejeicao,
                e.RejeitadoPor,           // NOVO
                e.RejeitadoEm,            // NOVO
                e.Ativo,
                e.CriadoEm,
                e.CriadoPor,
                e.Linhas.Count,
                totalRequisitados,
                totalAprovados,
                totalRecebidos,
                percentualRecebido,
                linhas
            );
        }
    }
}