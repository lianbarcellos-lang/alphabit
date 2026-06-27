# Lesson - Problemas Encontrados e Licoes Aprendidas

Este documento registra os principais problemas enfrentados durante a evolucao do GeekTop, o impacto de cada um, por que alguns demoraram para ser resolvidos e qual solucao foi adotada.

## 1. Rebranding e risco de misturar identidades

### Problema

O projeto nasceu com outra identidade e foi pivotado para GeekTop. Durante a pivotagem, alguns textos, nomes de tela e documentos ainda carregavam referencias antigas.

### Impacto

Isso poderia confundir a apresentacao, a avaliacao do professor e a propria navegacao do usuario.

### Por que demorou

A identidade antiga aparecia em varios lugares diferentes: textos de tela, README, requisitos, roadmap, specs, nomes internos e documentacao.

### Solucao aplicada

Foi feita uma revisao progressiva de telas e documentos para padronizar o nome GeekTop, mantendo apenas nomes tecnicos internos quando a troca poderia quebrar o projeto.

## 2. Fluxo de compra separado em telas demais

### Problema

O cliente precisava sair do detalhe do evento e ir para uma tela separada para escolher ingresso. Isso deixava o fluxo menos intuitivo.

### Impacto

O usuario via o evento em uma tela, mas so iniciava a compra em outra. Isso criava uma quebra desnecessaria na experiencia.

### Por que demorou

A tela antiga de assentos ja tinha logica de tipo de ingresso, quantidade e carrinho. Era preciso reaproveitar essa logica sem duplicar comportamento nem quebrar links existentes.

### Solucao aplicada

A compra foi movida para dentro do detalhe do evento. A rota antiga foi mantida como compatibilidade, mas o fluxo principal passou a acontecer na propria pagina do evento.

## 3. Entrada geral confundida com reserva de assento

### Problema

Eventos grandes de cultura geek normalmente funcionam com entrada geral, mas em alguns momentos o sistema tratava a compra principal como se precisasse de assento marcado.

### Impacto

Isso criava uma regra artificial para eventos com milhares de pessoas e deixava a interface mais confusa.

### Por que demorou

Era necessario separar dois conceitos parecidos: ingresso de entrada no evento e vaga limitada em atividade interna.

### Solucao aplicada

O ingresso principal passou a ser entrada geral sem assento marcado. A selecao de lugar ficou restrita a atividades internas, como workshop, torneio, painel e meet and greet.

## 4. Programacao interna sem cadastro administrativo

### Problema

O cliente ja via atividades internas, mas o painel administrativo ainda nao tinha uma area clara para criar programacao do evento.

### Impacto

As atividades ficavam dependentes de dados iniciais ou ajustes diretos, o que nao fazia sentido para o administrador.

### Por que demorou

Foi preciso integrar cadastro, listagem, exclusao, capacidade, horario inicial/final, descricao e inscricao com selecao de lugar.

### Solucao aplicada

Foi adicionada a area de Programacao no painel administrativo. O administrador cadastra a atividade e define capacidade; o sistema gera automaticamente os lugares disponiveis para inscricao do cliente.

## 5. Selecao de lugares nas atividades

### Problema

Ao se inscrever em uma atividade, o cliente precisava conseguir escolher um lugar, mas o sistema tambem precisava impedir que outra pessoa escolhesse o mesmo assento.

### Impacto

Sem essa regra, duas pessoas poderiam disputar a mesma vaga ou a capacidade poderia ficar incoerente.

### Por que demorou

Foi necessario controlar assentos por atividade, considerar inscricoes existentes, permitir cancelamento e manter a quantidade vinculada aos ingressos comprados.

### Solucao aplicada

Foi criado um modal de selecao de lugar. A inscricao salva os assentos escolhidos, bloqueia os ocupados e libera os lugares quando a inscricao e cancelada.

## 6. Mapa de stands por imagem

### Problema

A ideia inicial era tentar fazer o sistema detectar automaticamente todos os stands pela imagem da planta.

### Impacto

Na pratica, isso ficava instavel: imagens diferentes, linhas da arquitetura, baixa resolucao e pequenos desalinhamentos faziam a deteccao errar.

### Por que demorou

Foram avaliadas alternativas como deteccao automatica, matriz, SVG e desenho assistido. Cada uma tinha custo ou risco alto para o escopo atual.

### Solucao aplicada

O sistema passou a permitir upload da planta, cadastro manual de stands, drag/drop com coordenadas percentuais e organizacao automatica simples por grades compactas.

## 7. Organizacao automatica de stands

### Problema

As primeiras versoes da organizacao automatica deixavam os stands muito espalhados, muito juntos ou perto das bordas da planta.

### Impacto

O mapa ficava visualmente ruim e exigia muito ajuste manual.

### Por que demorou

Foi necessario equilibrar centralizacao, espacamento, tamanho dos blocos, limite da planta e liberdade para o administrador escolher uma grade adequada.

### Solucao aplicada

A organizacao visivel foi limitada a 3x3 e 4x4. O layout passou a usar uma area util central, com margens internas, espacamento uniforme e possibilidade de ajuste manual depois.

## 8. QR Code e camera no navegador

### Problema

O check-in por QR Code dependia de suporte do navegador e permissao de camera.

### Impacto

Em alguns navegadores ou contextos sem HTTPS, a leitura por camera poderia falhar.

### Por que demorou

Era preciso manter a leitura por camera, mas sem deixar o administrador travado quando o dispositivo nao oferecesse suporte.

### Solucao aplicada

O sistema manteve duas formas de check-in: leitura por camera/webcam e digitacao manual do codigo.

## 9. Lentidao e travamentos depois do crescimento do sistema

### Problema

Com mais funcionalidades, surgiram pontos de lentidao: filtros recalculados no Blazor, dashboard carregando muitas compras, imagens grandes em base64, chamadas JavaScript repetidas e consultas sem indices suficientes.

### Impacto

O painel administrativo e algumas telas poderiam parecer lentos ou travar em bases maiores.

### Por que demorou

Antes de otimizar, era preciso identificar se a lentidao vinha do frontend, da API, do banco, das imagens ou do ciclo de vida dos componentes.

### Solucao aplicada

Foram aplicadas melhorias incrementais:

- cache de filtros na pagina de eventos;
- dashboard com limite/paginacao;
- totais calculados por SQL;
- indices SQLite idempotentes;
- limite para imagens novas;
- controle para nao reinicializar drag/drop do mapa a cada render.

## 10. Railway e persistencia de dados

### Problema

O deploy no Railway roda a aplicacao, mas SQLite em arquivo local pode nao ser persistente sem volume configurado.

### Impacto

Em ambiente real, dados poderiam ser perdidos em redeploy ou reinicializacao se nao houver volume persistente.

### Por que demorou

Localmente o SQLite funciona muito bem, mas em deploy a persistencia depende da configuracao da plataforma.

### Solucao aplicada

A documentacao passou a registrar esse risco. Para apresentacao, SQLite continua aceitavel; para producao, recomenda-se volume persistente ou banco gerenciado.

## 11. Documentacao sempre ficava atras das telas

### Problema

Como o projeto evoluiu muito rapido, alguns documentos ficavam desatualizados em relacao ao sistema real.

### Impacto

Roadmap, requisitos, specs e ADR podiam citar funcionalidades antigas ou opcoes removidas, como grades 5x5 e 8x8.

### Por que demorou

Cada mudanca de tela exigia revisar varios documentos diferentes.

### Solucao aplicada

Foram criados e atualizados documentos centrais: `adr.md`, `arch.md`, `budget.md`, `lesson.md`, `roadmap.md`, `vision.md`, `specs.md` e `requisitos.md`.

## 12. Testes ajudaram a evitar regressao

### Problema

Mudancas em reservas, atividades, dashboard e API poderiam quebrar fluxos existentes sem aviso visual imediato.

### Impacto

Uma correcao em uma parte poderia gerar erro em outra, principalmente porque o projeto concentra muita logica na API.

### Por que demorou

Alguns testes eram de inspecao textual e precisaram ser atualizados quando a implementacao mudou de listas em memoria para consultas SQL agregadas.

### Solucao aplicada

Os testes foram mantidos e ajustados para validar o comportamento novo. A regra de manutencao ficou: rodar build e testes antes de considerar uma fase concluida.

## 13. Recuperacao de senha sem infraestrutura validada

### Problema

A interface chegou a ter caminho de recuperacao de senha, mas o envio de email nao estava configurado e validado no dominio de producao.

### Impacto

Se o professor ou um usuario clicasse nessa opcao, a funcionalidade poderia parecer quebrada e prejudicar a avaliacao do sistema.

### Por que demorou

O fluxo parecia pequeno visualmente, mas dependia de infraestrutura externa, configuracao de email, dominio e testes ponta a ponta.

### Solucao aplicada

A recuperacao de senha foi removida da interface e da documentacao de funcionalidades entregues. O login e cadastro ficaram como fluxos principais, totalmente demonstraveis.

## 14. Cards de eventos pareciam clicaveis, mas dependiam demais do Blazor

### Problema

Os cards de eventos navegavam por eventos de clique do Blazor. Em producao, qualquer atraso ou reconexao podia deixar o card sem responder imediatamente.

### Impacto

O usuario poderia achar que o site travou ou que o evento nao abre.

### Por que demorou

Localmente o clique funcionava, mas o comportamento em producao exigia validar o carregamento real no dominio.

### Solucao aplicada

Os cards e destaques passaram a usar links reais para `/eventos/{id}`, mantendo o visual de card e o botao `Ver ingressos`.

## 15. Deploy Railway em um servico unico

### Problema

A documentacao antiga falava em dois servicos no Railway, mas o deploy real foi consolidado em um unico container.

### Impacto

Seguir o documento antigo poderia fazer o deploy ficar mais complexo e com variaveis erradas.

### Por que demorou

O projeto tem API e App separados na solucao, mas no Railway ficou mais simples empacotar os dois juntos.

### Solucao aplicada

O `Dockerfile` e o `railway-start.sh` iniciam API e App no mesmo servico. A API fica interna em `8081`, o App usa `$PORT` e o dominio `https://geektop.store` aponta para o servico publico.
