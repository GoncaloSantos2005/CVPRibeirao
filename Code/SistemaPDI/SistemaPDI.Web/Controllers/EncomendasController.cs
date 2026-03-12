using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Models;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class EncomendasController : BaseController
    {
        private readonly IFileService _fileService;

        public EncomendasController(IPdiApiService pdiService, IFileService fileService) : base(pdiService)
        {
            _fileService = fileService;
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILTRO DE ACESSO
        // ══════════════════════════════════════════════════════════════════════
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Todos os perfis podem ver encomendas, mas algumas ações são restritas
            RequererPerfil(context, "GESTOR", "GESTOR_FINANCEIRO", "SOCORRISTA", "ADMINISTRADOR");
        }

        // ══════════════════════════════════════════════════════════════════════
        // DASHBOARD PRINCIPAL
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas
        [HttpGet]
        public async Task<IActionResult> Index(string? estado, bool? apenasAtivos)
        {
            var ehAdmin = VerificarPerfil("ADMINISTRADOR");
            var ehGestorLogistica = VerificarPerfil("GESTOR_FINANCEIRO");
            var ehGestor = VerificarPerfil("GESTOR");

            // Por defeito, ocultar canceladas
            var ocultarCanceladas = apenasAtivos ?? true;

            // Obter todas as encomendas (sempre incluir todas para poder contar)
            var resultado = await _pdiService.ObterEncomendasAsync(incluirInativos: false);
            
            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro ?? "Erro ao carregar encomendas");
                return View(new List<EncomendaDto>());
            }
            
            var todasEncomendas = resultado.Dados ?? new List<EncomendaDto>();

            // Calcular totais (sempre com todas as encomendas)
            ViewBag.TotaisPorEstado = new Dictionary<string, int>
            {
                { "LISTA", todasEncomendas.Count(e => e.Estado == "LISTA") },
                { "RASCUNHO", todasEncomendas.Count(e => e.Estado == "RASCUNHO") },
                { "PENDENTE", todasEncomendas.Count(e => e.Estado == "PENDENTE") },
                { "CONFIRMADA", todasEncomendas.Count(e => e.Estado == "CONFIRMADA") },
                { "ENVIADA", todasEncomendas.Count(e => e.Estado == "ENVIADA") },
                { "PARCIAL", todasEncomendas.Count(e => e.Estado == "PARCIAL") },
                { "CONCLUIDA", todasEncomendas.Count(e => e.Estado == "CONCLUIDA") },
                { "CANCELADA", todasEncomendas.Count(e => e.Estado == "CANCELADA") }
            };

            // Lista a mostrar (começa com todas)
            var encomendasFiltradas = todasEncomendas.AsEnumerable();

            // Aplicar filtro de estado específico
            if (!string.IsNullOrEmpty(estado))
            {
                encomendasFiltradas = encomendasFiltradas.Where(e => e.Estado == estado);
            }
            // Se não há filtro de estado E toggle está ativo, esconder canceladas
            else if (ocultarCanceladas)
            {
                encomendasFiltradas = encomendasFiltradas.Where(e => e.Estado != "CANCELADA");
            }

            var listaFinal = encomendasFiltradas.ToList();

            // Total geral (excluindo canceladas se toggle ativo, exceto se estamos a ver canceladas)
            ViewBag.TotalGeral = ocultarCanceladas 
                ? todasEncomendas.Count(e => e.Estado != "CANCELADA")
                : todasEncomendas.Count;

            ViewData["Title"] = "Encomendas";
            ViewBag.EhAdmin = ehAdmin;
            ViewBag.EhGestorLogistica = ehGestorLogistica;
            ViewBag.EhGestor = ehGestor;
            ViewBag.FiltroEstado = estado;
            ViewBag.ApenasAtivos = ocultarCanceladas;

            return View(listaFinal);
        }

        // ══════════════════════════════════════════════════════════════════════
        // DETALHES
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas/Detalhes/5
        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var resultado = await _pdiService.ObterEncomendaPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = $"Encomenda {resultado.Dados!.NumeroEncomenda}";
            ViewBag.EhGestorLogistica = VerificarPerfil("GESTOR_FINANCEIRO") && !VerificarPerfil("ADMINISTRADOR");
            ViewBag.EhGestor = VerificarPerfil("GESTOR", "ADMINISTRADOR");
            ViewBag.EhAdmin = VerificarPerfil("ADMINISTRADOR");

            return View(resultado.Dados);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 1: CRIAR LISTA (Apenas GESTOR e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas/CriarLista
        [HttpGet]
        public async Task<IActionResult> CriarLista()
        {
            // GESTOR_FINANCEIRO não pode criar listas
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Apenas gestores podem criar listas de encomenda");
                return RedirectToAction(nameof(Index));
            }

            await CarregarDadosFormulario();

            // Carregar artigos com stock baixo/crítico para sugestões
            var artigosBaixo = await _pdiService.ObterArtigosComStockBaixoAsync();
            var artigosCritico = await _pdiService.ObterArtigosComStockCriticoAsync();

            ViewBag.ArtigosStockBaixo = artigosBaixo.Dados ?? new List<ArtigoDto>();
            ViewBag.ArtigosStockCritico = artigosCritico.Dados ?? new List<ArtigoDto>();

            ViewData["Title"] = "Nova Lista de Encomenda";
            return View();
        }

        // POST /Encomendas/CriarLista
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarLista(CriarListaViewModel viewModel)
        {
            // GESTOR_FINANCEIRO não pode criar listas
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Apenas gestores podem criar listas de encomenda");
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid || viewModel.Linhas == null || !viewModel.Linhas.Any())
            {
                MensagemErro("Adicione pelo menos um artigo à lista");
                await CarregarDadosFormulario();
                return View(viewModel);
            }

            try
            {
                var dto = new CriarListaDto(
                    viewModel.Linhas.Select(l => new LinhaListaDto(
                        l.ArtigoId,
                        l.QuantidadeRequisitada
                    )).ToList(),
                    viewModel.Observacoes
                );

                var resultado = await _pdiService.CriarListaAsync(dto);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                    await CarregarDadosFormulario();
                    return View(viewModel);
                }

                MensagemSucesso($"Lista {resultado.Dados!.NumeroEncomenda} criada com sucesso!");
                return RedirectToAction(nameof(Detalhes), new { id = resultado.Dados.Id });
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro ao criar lista: {ex.Message}");
                await CarregarDadosFormulario();
                return View(viewModel);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDITAR LISTA (Apenas GESTOR e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas/EditarLista/5
        [HttpGet]
        public async Task<IActionResult> EditarLista(int id)
        {
            // GESTOR_FINANCEIRO não pode editar listas
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Apenas gestores podem editar listas de encomenda");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterEncomendaPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            var encomenda = resultado.Dados!;

            // Só pode editar se estado LISTA
            if (encomenda.Estado != "LISTA")
            {
                MensagemErro("Só é possível editar encomendas no estado LISTA");
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            await CarregarDadosFormulario();

            // Carregar artigos com stock baixo/crítico para sugestões
            var artigosBaixo = await _pdiService.ObterArtigosComStockBaixoAsync();
            var artigosCritico = await _pdiService.ObterArtigosComStockCriticoAsync();

            ViewBag.ArtigosStockBaixo = artigosBaixo.Dados ?? new List<ArtigoDto>();
            ViewBag.ArtigosStockCritico = artigosCritico.Dados ?? new List<ArtigoDto>();
            ViewBag.Encomenda = encomenda;
            ViewBag.ModoEdicao = true;

            // Preparar ViewModel com dados existentes
            var viewModel = new CriarListaViewModel
            {
                Observacoes = encomenda.Observacoes,
                Linhas = encomenda.Linhas.Select(l => new LinhaListaViewModel
                {
                    ArtigoId = l.ArtigoId,
                    QuantidadeRequisitada = l.QuantidadeEncomendada
                }).ToList()
            };

            ViewData["Title"] = $"Editar Lista - {encomenda.NumeroEncomenda}";
            return View("CriarLista", viewModel);
        }

        // POST /Encomendas/EditarLista/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarLista(int id, CriarListaViewModel viewModel)
        {
            // GESTOR_FINANCEIRO não pode editar listas
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Apenas gestores podem editar listas de encomenda");
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid || viewModel.Linhas == null || !viewModel.Linhas.Any())
            {
                MensagemErro("Adicione pelo menos um artigo à lista");
                await CarregarDadosFormularioEdicao(id);
                return View("CriarLista", viewModel);
            }

            try
            {
                // Obter encomenda atual para verificar se tem orçamento PDF a apagar
                var encomendaAtual = await _pdiService.ObterEncomendaPorIdAsync(id);
                var caminhoOrcamentoAntigo = encomendaAtual.Dados?.CaminhoOrcamentoPdf;

                var dto = new CriarListaDto(
                    viewModel.Linhas.Select(l => new LinhaListaDto(
                        l.ArtigoId,
                        l.QuantidadeRequisitada
                    )).ToList(),
                    viewModel.Observacoes
                );

                var resultado = await _pdiService.AtualizarListaAsync(id, dto);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                    await CarregarDadosFormularioEdicao(id);
                    return View("CriarLista", viewModel);
                }

                // Apagar o PDF do orçamento antigo se existia
                if (!string.IsNullOrEmpty(caminhoOrcamentoAntigo))
                {
                    try
                    {
                        await _fileService.ApagarFicheiroAsync(caminhoOrcamentoAntigo);
                    }
                    catch
                    {
                        // Ignorar erro ao apagar ficheiro - não é crítico
                    }
                }

                MensagemSucesso($"Lista {resultado.Dados!.NumeroEncomenda} atualizada com sucesso! Pode agora avançar para rascunho.");
                return RedirectToAction(nameof(Detalhes), new { id = resultado.Dados.Id });
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro ao atualizar lista: {ex.Message}");
                await CarregarDadosFormularioEdicao(id);
                return View("CriarLista", viewModel);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 2: GERAR PDF E MARCAR COMO RASCUNHO (Apenas GESTOR e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas/GerarPdf/5
        [HttpGet]
        public async Task<IActionResult> GerarPdf(int id)
        {
            // GESTOR_FINANCEIRO não pode gerar PDF
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.GerarPdfAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            return File(resultado.Dados!, "application/pdf", $"Lista_Encomenda_{id}.pdf");
        }

        // POST /Encomendas/MarcarRascunho/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarRascunho(int id)
        {
            // GESTOR_FINANCEIRO não pode marcar como rascunho
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _pdiService.MarcarComoRascunhoAsync(id);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                }
                else
                {
                    MensagemSucesso("Encomenda marcada como RASCUNHO. PDF gerado e pronto para enviar a fornecedores.");
                }
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro: {ex.Message}");
            }

            return RedirectToAction(nameof(Detalhes), new { id });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 3: SUBMETER ORÇAMENTO (Apenas GESTOR e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas/SubmeterOrcamento/5
        [HttpGet]
        public async Task<IActionResult> SubmeterOrcamento(int id)
        {
            // GESTOR_FINANCEIRO não pode submeter orçamentos
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterEncomendaPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            if (resultado.Dados!.Estado != "RASCUNHO")
            {
                MensagemErro("Só é possível submeter orçamento para encomendas em RASCUNHO");
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            ViewBag.Encomenda = resultado.Dados;
            ViewData["Title"] = $"Submeter Orçamento - {resultado.Dados.NumeroEncomenda}";

            return View();
        }

        // POST /Encomendas/SubmeterOrcamento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmeterOrcamento(int id, SubmeterOrcamentoViewModel viewModel)
        {
            // GESTOR_FINANCEIRO não pode submeter orçamentos
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão");
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid || viewModel.OrcamentoPdf == null)
            {
                MensagemErro("Anexe o PDF do orçamento e preencha o valor");
                var encomenda = await _pdiService.ObterEncomendaPorIdAsync(id);
                ViewBag.Encomenda = encomenda.Dados;
                return View(viewModel);
            }

            // Validar PDF
            if (!_fileService.ValidarPdf(viewModel.OrcamentoPdf))
            {
                MensagemErro("Ficheiro inválido. Apenas PDFs até 10MB são permitidos.");
                var encomenda = await _pdiService.ObterEncomendaPorIdAsync(id);
                ViewBag.Encomenda = encomenda.Dados;
                return View(viewModel);
            }

            try
            {
                // Guardar o PDF localmente primeiro
                var caminhoOrcamento = await _fileService.GuardarFicheiroAsync(
                    viewModel.OrcamentoPdf, 
                    "orcamentos", 
                    $"ENC-{id}"
                );

                var dto = new SubmeterOrcamentoDto(
                    viewModel.ValorOrcamento,
                    viewModel.Observacoes,
                    caminhoOrcamento  // Passar o caminho já guardado
                );

                var resultado = await _pdiService.SubmeterOrcamentoAsync(id, dto);

                if (!resultado.Sucesso)
                {
                    // Se falhou na API, apagar o ficheiro que guardámos
                    await _fileService.ApagarFicheiroAsync(caminhoOrcamento);
                    MensagemErro(resultado.Erro!);
                    var encomenda = await _pdiService.ObterEncomendaPorIdAsync(id);
                    ViewBag.Encomenda = encomenda.Dados;
                    return View(viewModel);
                }

                MensagemSucesso("Orçamento submetido para aprovação do Gestor de Logística!");
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro ao submeter orçamento: {ex.Message}");
                var encomenda = await _pdiService.ObterEncomendaPorIdAsync(id);
                ViewBag.Encomenda = encomenda.Dados;
                return View(viewModel);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 4: APROVAR E PREENCHER (GESTOR_FINANCEIRO e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas/AprovarPreencher/5
        [HttpGet]
        public async Task<IActionResult> AprovarPreencher(int id)
        {
            // Apenas GESTOR_FINANCEIRO e ADMIN podem aprovar
            if (!VerificarPerfil("GESTOR_FINANCEIRO", "ADMINISTRADOR"))
            {
                MensagemErro("Apenas Gestor de Logística pode aprovar encomendas");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterEncomendaPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            if (resultado.Dados!.Estado != "PENDENTE")
            {
                MensagemErro("Só é possível aprovar encomendas PENDENTES");
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            // Carregar fornecedores
            var fornecedores = await _pdiService.ObterFornecedoresDropdownAsync();
            ViewBag.Fornecedores = fornecedores.Dados ?? new List<FornecedorDropdownDto>();

            ViewBag.Encomenda = resultado.Dados;
            ViewData["Title"] = $"Aprovar Encomenda - {resultado.Dados.NumeroEncomenda}";

            return View();
        }

        // POST /Encomendas/AprovarPreencher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarPreencher(int id, AprovarPreencherViewModel viewModel)
        {
            // Apenas GESTOR_FINANCEIRO e ADMIN podem aprovar
            if (!VerificarPerfil("GESTOR_FINANCEIRO", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão");
                return RedirectToAction(nameof(Index));
            }

            // Carregar sempre os dados necessários para a view
            var encomendaResult = await _pdiService.ObterEncomendaPorIdAsync(id);
            var fornecedores = await _pdiService.ObterFornecedoresDropdownAsync();
            ViewBag.Fornecedores = fornecedores.Dados ?? new List<FornecedorDropdownDto>();
            ViewBag.Encomenda = encomendaResult.Dados;

            if (!ModelState.IsValid)
            {
                // DEBUG: Mostrar exatamente quais campos falharam
                var erros = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value!.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();
                
                var mensagemErros = string.Join(" | ", erros);
                MensagemErro($"Erros de validação: {mensagemErros}");
                
                return View(viewModel);
            }

            try
            {
                var dto = new AprovarEPreencherDto(
                    viewModel.FornecedorId,
                    viewModel.DataEntregaPrevista,
                    viewModel.Linhas.Select(l => new PreencherLinhaDto(
                        l.LinhaId,
                        l.QuantidadeAprovada,
                        l.PrecoUnitario,
                        l.NumeroLote,
                        l.DataValidade
                    )).ToList(),
                    viewModel.ObservacoesInternas
                );

                var resultado = await _pdiService.AprovarEPreencherAsync(id, dto);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                    return View(viewModel);
                }

                MensagemSucesso("Encomenda aprovada e preenchida! Confirme para enviar ao fornecedor.");
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro: {ex.Message}");
                return View(viewModel);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 5: CONFIRMAR E ENVIAR (Apenas GESTOR e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // POST /Encomendas/ConfirmarEnviar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEnviar(int id)
        {
            // GESTOR_FINANCEIRO não pode confirmar/enviar - apenas GESTOR e ADMIN
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão para confirmar e enviar encomendas");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _pdiService.ConfirmarEEnviarAsync(id);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                }
                else
                {
                    MensagemSucesso("Encomenda confirmada e enviada ao fornecedor! Aguarda receção.");
                }
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro: {ex.Message}");
            }

            return RedirectToAction(nameof(Detalhes), new { id });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 6: REGISTAR RECEÇÃO (Apenas GESTOR e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // GET /Encomendas/RegistarRececao/5
        [HttpGet]
        public async Task<IActionResult> RegistarRececao(int id)
        {
            // GESTOR_FINANCEIRO não pode registar receções
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão para registar receções");
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pdiService.ObterEncomendaPorIdAsync(id);

            if (!resultado.Sucesso)
            {
                MensagemErro(resultado.Erro!);
                return RedirectToAction(nameof(Index));
            }

            if (resultado.Dados!.Estado != "ENVIADA" && resultado.Dados.Estado != "PARCIAL")
            {
                MensagemErro("Só é possível recepcionar encomendas ENVIADAS ou PARCIAIS");
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            // Carregar localizações
            var localizacoes = await _pdiService.ObterLocalizacoesDropdownAsync();
            ViewBag.Localizacoes = localizacoes.Dados ?? new List<LocalizacaoDropdownDto>();

            ViewBag.Encomenda = resultado.Dados;
            ViewData["Title"] = $"Registar Receção - {resultado.Dados.NumeroEncomenda}";

            return View();
        }

        // POST /Encomendas/RegistarRececao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistarRececao(RegistarRececaoViewModel viewModel)
        {
            // GESTOR_FINANCEIRO não pode registar receções
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão para registar receções");
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var localizacoes = await _pdiService.ObterLocalizacoesDropdownAsync();
                ViewBag.Localizacoes = localizacoes.Dados ?? new List<LocalizacaoDropdownDto>();
                return View(viewModel);
            }

            try
            {
                var dto = new RegistarRecepcaoDto(
                    viewModel.EncomendaId,
                    viewModel.Linhas.Select(l => new RecepcaoLinhaDto(
                        l.LinhaId,
                        l.QuantidadeRecebida,
                        l.NumeroLote,
                        l.DataValidade,
                        l.LocalizacaoId,
                        l.Observacoes
                    )).ToList(),
                    viewModel.Observacoes
                );

                var resultado = await _pdiService.RegistarRecepcaoAsync(dto);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                    var localizacoes = await _pdiService.ObterLocalizacoesDropdownAsync();
                    ViewBag.Localizacoes = localizacoes.Dados ?? new List<LocalizacaoDropdownDto>();
                    return View(viewModel);
                }

                var estadoFinal = resultado.Dados!.Estado;
                if (estadoFinal == "CONCLUIDA")
                {
                    MensagemSucesso("Receção concluída! Encomenda finalizada com sucesso. Stocks atualizados.");
                }
                else
                {
                    MensagemSucesso("Receção parcial registada. Ainda faltam artigos por recepcionar.");
                }

                return RedirectToAction(nameof(Detalhes), new { id = viewModel.EncomendaId });
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro: {ex.Message}");
                var localizacoes = await _pdiService.ObterLocalizacoesDropdownAsync();
                ViewBag.Localizacoes = localizacoes.Dados ?? new List<LocalizacaoDropdownDto>();
                return View(viewModel);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // REJEITAR (GESTOR_FINANCEIRO e ADMIN - apenas PENDENTES)
        // ══════════════════════════════════════════════════════════════════════

        // POST /Encomendas/Rejeitar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeitar(int id, string motivo)
        {
            // Apenas GESTOR_FINANCEIRO e ADMIN podem rejeitar
            if (!VerificarPerfil("GESTOR_FINANCEIRO", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _pdiService.RejeitarEncomendaAsync(id, motivo);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                }
                else
                {
                    MensagemSucesso("Encomenda rejeitada. Criador foi notificado para correção.");
                }
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // CANCELAR (Apenas GESTOR e ADMIN)
        // ══════════════════════════════════════════════════════════════════════

        // POST /Encomendas/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            // GESTOR_FINANCEIRO não pode cancelar encomendas
            if (!VerificarPerfil("GESTOR", "ADMINISTRADOR"))
            {
                MensagemErro("Sem permissão para cancelar encomendas");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await _pdiService.CancelarEncomendaAsync(id, motivo);

                if (!resultado.Sucesso)
                {
                    MensagemErro(resultado.Erro!);
                }
                else
                {
                    MensagemSucesso("Encomenda cancelada com sucesso!");
                }
            }
            catch (Exception ex)
            {
                MensagemErro($"Erro: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPER - Carregar dados para formulários
        // ══════════════════════════════════════════════════════════════════════

        private async Task CarregarDadosFormulario()
        {
            var artigos = await _pdiService.ObterArtigosAsync();
            ViewBag.Artigos = artigos.Dados ?? new List<ArtigoDto>();
        }

        // Helper para carregar dados no modo edição
        private async Task CarregarDadosFormularioEdicao(int encomendaId)
        {
            await CarregarDadosFormulario();

            var artigosBaixo = await _pdiService.ObterArtigosComStockBaixoAsync();
            var artigosCritico = await _pdiService.ObterArtigosComStockCriticoAsync();
            var encomenda = await _pdiService.ObterEncomendaPorIdAsync(encomendaId);

            ViewBag.ArtigosStockBaixo = artigosBaixo.Dados ?? new List<ArtigoDto>();
            ViewBag.ArtigosStockCritico = artigosCritico.Dados ?? new List<ArtigoDto>();
            ViewBag.Encomenda = encomenda.Dados;
            ViewBag.ModoEdicao = true;
        }
    }
}