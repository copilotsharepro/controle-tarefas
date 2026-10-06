(() => {
  const kanban = document.querySelector("#kanban");
  if (!kanban) {
    return;
  }

  const modalElement = document.querySelector("#modalCadastro");
  const formularioCadastro = document.querySelector("#form-cadastro");
  const alertaCadastro = document.querySelector("#alerta-cadastro");
  const botaoSalvarCadastro = document.querySelector("#botao-salvar-cadastro");
  const feedbackOperacao = document.querySelector("#feedback-operacao");
  const listas = () => Array.from(document.querySelectorAll(".js-lista-cards"));
  const modalCadastro = modalElement ? bootstrap.Modal.getOrCreateInstance(modalElement) : null;

  atualizarColunas();

  if (formularioCadastro) {
    formularioCadastro.addEventListener("submit", async (event) => {
      event.preventDefault();

      if (botaoSalvarCadastro?.disabled) {
        return;
      }

      limparAlertaCadastro();
      limparFeedbackOperacao();
      botaoSalvarCadastro.disabled = true;

      try {
        const resposta = await fetch(formularioCadastro.action, {
          method: "POST",
          body: new FormData(formularioCadastro),
        });

        if (!resposta.ok) {
          const mensagem = await lerMensagemErro(resposta, "Não foi possível salvar a tarefa.");
          mostrarAlertaCadastro(mensagem);
          return;
        }

        const htmlCartao = await resposta.text();
        const cartao = criarCartaoPorHtml(htmlCartao);
        if (!cartao) {
          mostrarAlertaCadastro("Não foi possível renderizar o cartão da tarefa.");
          return;
        }

        const seletorStatus = formularioCadastro.querySelector('select[name="Status"]');
        const status = seletorStatus?.value || "Novo";
        const listaDestino = document.querySelector(`.js-lista-cards[data-status="${status}"]`);
        if (!listaDestino) {
          mostrarAlertaCadastro("Não foi possível localizar a coluna de destino.");
          return;
        }

        listaDestino.appendChild(cartao);
        atualizarColunas();
        formularioCadastro.reset();
        if (seletorStatus) {
          seletorStatus.value = "Novo";
        }
        const inputData = formularioCadastro.querySelector('input[name="Data"]');
        if (inputData) {
          inputData.value = formatarDataAtual();
        }

        modalCadastro?.hide();
      } catch {
        mostrarAlertaCadastro("Falha de rede ao salvar a tarefa.");
      } finally {
        botaoSalvarCadastro.disabled = false;
      }
    });
  }

  modalElement?.addEventListener("hidden.bs.modal", () => {
    limparAlertaCadastro();
  });

  document.addEventListener("dragstart", (event) => {
    const card = event.target.closest(".js-cartao");
    if (!card) {
      return;
    }

    event.dataTransfer?.setData("text/plain", card.dataset.tarefaId ?? "");
    event.dataTransfer.effectAllowed = "move";
    card.classList.add("arrastando");
  });

  document.addEventListener("dragend", (event) => {
    const card = event.target.closest(".js-cartao");
    card?.classList.remove("arrastando");
    listas().forEach((lista) => lista.classList.remove("destino-arraste"));
  });

  listas().forEach((lista) => {
    lista.addEventListener("dragover", (event) => {
      event.preventDefault();
      lista.classList.add("destino-arraste");
    });

    lista.addEventListener("dragleave", () => {
      lista.classList.remove("destino-arraste");
    });

    lista.addEventListener("drop", async (event) => {
      event.preventDefault();
      lista.classList.remove("destino-arraste");
      limparFeedbackOperacao();

      const id = event.dataTransfer?.getData("text/plain");
      if (!id) {
        return;
      }

      const card = document.querySelector(`.js-cartao[data-tarefa-id="${id}"]`);
      if (!card) {
        return;
      }

      const statusDestino = lista.dataset.status;
      if (!statusDestino || statusDestino === card.dataset.status) {
        return;
      }

      const mensagemErro = await moverNoServidor(id, statusDestino);
      if (mensagemErro) {
        mostrarFeedbackOperacao(mensagemErro);
        return;
      }

      card.dataset.status = statusDestino;
      const badgeStatus = card.querySelector(".js-status-cartao");
      if (badgeStatus) {
        badgeStatus.textContent = statusDestino;
      }

      lista.appendChild(card);
      atualizarColunas();
    });
  });

  async function moverNoServidor(id, status) {
    const token = obterTokenAntiforgery();
    if (!token) {
      return "Token antiforgery não encontrado.";
    }

    const resposta = await fetch(kanban.dataset.moverUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
      },
      body: `id=${encodeURIComponent(id)}&status=${encodeURIComponent(status)}&__RequestVerificationToken=${encodeURIComponent(token)}`,
    });

    if (resposta.ok) {
      return null;
    }

    return await lerMensagemErro(resposta, "Não foi possível mover a tarefa.");
  }

  function atualizarColunas() {
    listas().forEach((lista) => {
      const cartoes = lista.querySelectorAll(".js-cartao");
      const contador = lista.closest(".kanban-coluna")?.querySelector(".js-contador");
      if (contador) {
        contador.textContent = String(cartoes.length);
      }

      const avisoVazio = lista.querySelector(".js-aviso-vazio");
      if (avisoVazio) {
        avisoVazio.classList.toggle("d-none", cartoes.length > 0);
      }
    });
  }

  function criarCartaoPorHtml(html) {
    const template = document.createElement("template");
    template.innerHTML = html.trim();
    return template.content.firstElementChild;
  }

  function obterTokenAntiforgery() {
    const formularioToken = document.querySelector("#form-antiforgery input[name='__RequestVerificationToken']");
    return formularioToken?.value || null;
  }

  function mostrarAlertaCadastro(mensagem) {
    if (!alertaCadastro) {
      return;
    }

    alertaCadastro.textContent = mensagem;
    alertaCadastro.classList.remove("d-none");
  }

  function limparAlertaCadastro() {
    if (!alertaCadastro) {
      return;
    }

    alertaCadastro.textContent = "";
    alertaCadastro.classList.add("d-none");
  }

  function mostrarFeedbackOperacao(mensagem) {
    if (!feedbackOperacao) {
      return;
    }

    feedbackOperacao.textContent = mensagem;
    feedbackOperacao.classList.remove("d-none");
  }

  function limparFeedbackOperacao() {
    if (!feedbackOperacao) {
      return;
    }

    feedbackOperacao.textContent = "";
    feedbackOperacao.classList.add("d-none");
  }

  async function lerMensagemErro(resposta, mensagemPadrao) {
    const texto = (await resposta.text()).trim();
    return texto || mensagemPadrao;
  }

  function formatarDataAtual() {
    const data = new Date();
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, "0");
    const dia = String(data.getDate()).padStart(2, "0");
    return `${ano}-${mes}-${dia}`;
  }
})();
