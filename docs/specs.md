# Especificações - GeekTop

Este documento resume as especificações funcionais e técnicas do GeekTop.

## 1. Identidade do projeto

Nome: GeekTop.

Proposta: plataforma de eventos geek/anime/games/cultura pop, com compra de ingressos, reservas, convidados, atividades, mapa de stands, avaliações e check-in.

## 2. Stack

- C#
- .NET 9
- ASP.NET Core Minimal API
- Blazor Server / Razor Components
- SQLite
- Dapper
- xUnit

## 3. Usuários

### Funcionalidades

- Cadastro de cliente.
- Login com e-mail ou CPF.
- Perfil do usuário.
- Proteção de reservas por CPF.
- Recuperação de senha não exposta na interface desta entrega.

### Regras

- CPF não pode duplicar.
- E-mail não pode duplicar.
- Senha de cliente deve ser armazenada como hash.
- Perfil e reservas devem ser acessados apenas pelo próprio cliente ou pelo administrador.

## 4. Administrador

### Funcionalidades

- Login administrativo.
- Painel de controle.
- Gestão de eventos.
- Gestão de cupons.
- Gestão de cidades.
- Gestão de categorias geek.
- Gestão de convidados.
- Gestão de mapa de stands e expositores.
- Relatório de vendas.
- Moderação de avaliações.
- Check-in.

### Regras

- Rotas administrativas exigem token administrativo.
- Cliente não deve acessar funções administrativas.

## 5. Eventos

### Funcionalidades

- Listar eventos públicos.
- Ver detalhe do evento.
- Abrir detalhe ao clicar no card ou no botão `Ver ingressos`.
- Cadastrar evento.
- Editar evento.
- Excluir evento.
- Exibir imagem/capa.
- Exibir categoria, cidade, local, data, preço e média de avaliação.

### Campos principais

- `Id`
- `Nome`
- `LocalEvento`
- `CidadeEvento`
- `Artista`
- `GeneroMusical`
- `CapacidadeTotal`
- `DataEvento`
- `PrecoPadrao`
- `ImagemUrl`
- `MapaImagemUrl`
- `EhDestaque`

### Regras

- Evento precisa ter dados obrigatórios.
- Capacidade deve ser respeitada nas reservas.
- Imagens devem representar o universo geek, games, anime ou cosplay.

## 6. Filtros de descoberta

### Funcionalidades

- Filtrar por cidade.
- Filtrar por dia da semana.
- Filtrar por categoria geek.
- Pesquisar eventos pela barra de busca.
- Usar links reais nos cards para manter navegação funcional mesmo durante reconexões do Blazor.

### Regras

- Filtros devem funcionar combinados.
- Botão de limpar filtros deve restaurar a lista.
- Cada card deve apontar para `/eventos/{id}`.

## 7. Tipos de ingresso

### Funcionalidades

- Criar tipos de ingresso por evento.
- Exibir benefícios.
- Usar preço do tipo selecionado no carrinho/reserva.

### Tipos esperados

- Normal
- VIP
- Premium
- Day Pass
- Meet and Greet

### Regras

- Tipo de ingresso deve pertencer ao evento.
- Não deve vender acima da quantidade disponível.
- Meet and Greet deve ter limite de vagas.

## 8. Escolha de ingressos

### Funcionalidades

- Escolher tipo de ingresso.
- Escolher quantidade de ingressos.
- Adicionar ingresso ao carrinho.
- Tratar a reserva principal como entrada geral sem assento marcado.

### Regras

- Cada CPF pode reservar até 2 ingressos por evento.
- A disponibilidade deve respeitar capacidade total do evento.
- A disponibilidade deve respeitar quantidade do tipo de ingresso.
- Vagas limitadas específicas devem ser controladas nas atividades internas, como workshop, torneio e meet and greet.

## 9. Carrinho e cupons

### Funcionalidades

- Adicionar ingresso ao carrinho.
- Aplicar cupom.
- Pré-visualizar desconto.
- Escolher forma de pagamento.
- Finalizar compra.

### Regras

- Cupom deve respeitar valor mínimo.
- Desconto deve ser aplicado antes da reserva.
- Forma de pagamento deve ser registrada.

## 10. Reservas

### Funcionalidades

- Criar reserva.
- Consultar reservas por CPF autorizado.
- Mostrar histórico do cliente.
- Mostrar QR Code da reserva.

### Regras

- Usuário deve existir.
- Evento deve existir.
- Reserva deve respeitar capacidade.
- Deve existir limite por CPF/evento.
- Reserva deve gerar código de pedido.
- Reserva deve criar QR Code único para check-in.

## 11. Atividades internas

### Funcionalidades

- Cadastrar atividades internas.
- Informar nome, tipo, data, horário inicial, horário final, descrição opcional e capacidade máxima.
- Listar atividades no detalhe do evento.
- Inscrever cliente em atividade.
- Exibir modal para seleção de lugar ao se inscrever.
- Cancelar inscrição do cliente em atividade.
- Excluir atividade pelo painel administrativo.

### Regras

- Atividade deve pertencer a evento existente.
- Não permitir inscrição duplicada.
- Não ultrapassar limite de participantes.
- A capacidade cadastrada deve gerar automaticamente a quantidade de lugares disponíveis.
- Lugar já ocupado não pode ser escolhido por outro cliente.
- O cliente não deve conseguir reservar mais lugares em atividades do que a quantidade de ingressos comprada para o evento.
- Ao cancelar inscrição, a vaga deve voltar a ficar disponível.

## 12. Convidados

### Funcionalidades

- Cadastrar convidado.
- Associar convidado ao evento.
- Remover associação.
- Exibir convidados no detalhe do evento.

### Tipos esperados

- Cosplayer
- Streamer
- Influencer
- Voice Actor
- Banda
- Artista
- Youtuber

## 13. Mapa de stands e expositores

### Funcionalidades

- Gerar mapa de stands por evento.
- Organizar stands por linhas ou setores, como Linha Azul, Linha Vermelha, Linha Verde e Linha Amarela.
- Permitir que o administrador envie uma imagem de planta do evento.
- Permitir que o administrador cadastre stands manualmente.
- Permitir que o administrador crie, renomeie e exclua linhas/setores do mapa.
- Permitir organização automática dos stands por grades compactas visíveis 3x3 e 4x4.
- Reposicionar todos os stands proporcionalmente dentro da planta ao aplicar uma grade automática.
- Manter o ajuste manual por drag/drop após a aplicação da grade automática.
- Permitir que o administrador arraste stands sobre a planta e salve as coordenadas.
- Permitir que o administrador edite o código/nome do stand sem recadastrar.
- Permitir que o administrador exclua um stand diretamente pela lista de linhas.
- Carregar automaticamente a imagem e as posições salvas quando o mapa administrativo for aberto novamente.
- Permitir que o administrador reserve cada stand para empresa, marca, loja, arena ou atração.
- Permitir que o administrador libere um stand reservado.
- Exibir o mapa atualizado no detalhe do evento para o cliente.
- Exibir o mesmo mapa no detalhe e na área de ingressos do cliente.
- Exibir também uma lista por setor com o stand e seu ocupante.
- Recarregar o mapa administrativo quando o administrador troca o evento selecionado.

### Campos principais

- `Id`
- `EventoId`
- `Setor`
- `Codigo`
- `PosicaoX`
- `PosicaoY`
- `Largura`
- `Altura`
- `TipoArea`
- `AreaX`
- `AreaY`
- `AreaLargura`
- `AreaAltura`
- `AreaMetrosQuadrados`
- `PrecoPorMetroQuadrado`
- `PrecoFixo`
- `Reservado`
- `NomeOcupante`
- `TipoOcupante`
- `Descricao`

### Setores iniciais

- Linha Azul
- Linha Vermelha
- Linha Verde
- Linha Amarela

### Exemplos de ocupantes

- LG
- Samsung
- Ubisoft
- Campeonato de Games
- Área Cosplay
- Loja Geek

### Regras

- Stand deve pertencer a um evento.
- Rotas de alocação exigem token administrativo.
- Stand reservado precisa ter nome de empresa, marca ou atração.
- Linha com stands não deve ser excluída antes de mover ou apagar os stands.
- Cliente visualiza o mapa, mas não altera alocações.
- A reserva principal de ingresso continua sendo entrada geral, sem assento marcado.
- O mapa de stands representa organização interna do evento, não reserva de assento do cliente.
- O administrador deve poder definir preço por metro quadrado ou preço fixo por stand, útil para esquinas, ruas principais e áreas premium.
- O sistema deve manter o mapa padrão em blocos quando não houver planta enviada.
- A planta enviada deve usar coordenadas percentuais para funcionar em telas de tamanhos diferentes.
- Ao criar ou carregar um evento sem stands, o sistema deve gerar o mapa padrão.
- A grade automática deve respeitar a quantidade de stands cadastrados: 3x3 até 9 e 4x4 até 16.
- A grade automática deve ficar desabilitada quando a quantidade de stands ultrapassar sua capacidade.
- A grade automática deve recalcular posição e tamanho dos stands para reduzir sobreposições, sem impedir ajustes manuais posteriores.

### Endpoints

- `GET /api/eventos/{id}/stands`
- `POST /api/admin/eventos/{id}/stands`
- `PUT /api/admin/eventos/{id}/mapa-imagem`
- `PUT /api/admin/eventos/{id}/stands/{standId}`
- `DELETE /api/admin/eventos/{id}/stands/{standId}`
- `POST /api/admin/eventos/{id}/stand-setores`
- `PUT /api/admin/eventos/{id}/stand-setores/{nomeAtual}`
- `DELETE /api/admin/eventos/{id}/stand-setores/{nome}`

### Fluxo administrativo

1. Administrador acessa o painel ADM.
2. Abre a aba `Mapa de stands`.
3. Seleciona o evento.
4. Envia a imagem da planta do evento.
5. Cadastra ou seleciona um stand.
6. Preenche empresa ou atração, tipo e descrição.
7. Opcionalmente aplica uma grade automática 3x3 ou 4x4 para organizar os stands.
8. Arrasta o stand para o ponto desejado na planta quando precisar de ajuste fino.
9. Salva a alocação.
10. Ao reabrir o mapa, a planta e os stands aparecem nas posições salvas.

### Fluxo do cliente

1. Cliente abre o detalhe do evento.
2. Visualiza o mapa do evento, usando a planta enviada quando houver imagem cadastrada.
3. Consulta setores e stands reservados.
4. Usa o mapa para entender onde ficam empresas, lojas, arenas e atrações.
5. Na área de ingressos, abre o mapa em janela para consulta.

### Testes esperados

- Validar existência da tabela `StandsEspacos`.
- Validar índice único por evento e código de stand.
- Validar endpoint público do mapa.
- Validar endpoint administrativo de alocação.
- Validar exigência de token administrativo.
- Validar que stand reservado precisa de ocupante.
- Validar que o detalhe do evento consome e exibe os stands.
- Validar que a tela administrativa exibe as opções de grade automática e mantém drag/drop manual.

## Deploy e dominio

- Deploy em Railway com um unico servico Docker.
- API interna em `8081`.
- App Blazor publico na porta `$PORT`.
- Dominio de apresentacao: `https://geektop.store`.
- Cards e destaques usam links reais para `/eventos/{id}`.

## 14. Check-in com QR Code

### Funcionalidades

- Gerar QR Code por reserva.
- Exibir QR Code para o cliente.
- Ampliar QR Code para facilitar leitura.
- Validar código manualmente no painel administrativo.
- Validar QR Code por câmera/webcam no painel administrativo.

### Regras

- QR Code deve ser único.
- Check-in duplicado deve ser bloqueado.
- Reserva cancelada deve ser rejeitada.
- Código inválido deve retornar mensagem clara.

## 15. Avaliações

### Funcionalidades

- Cliente avalia evento reservado.
- Sistema mostra avaliações no detalhe do evento.
- Sistema mostra média da avaliação junto ao preço.
- Administrador pode moderar/remover avaliação.

### Regras

- Nota deve ser válida.
- Usuário só pode avaliar evento reservado.
- Usuário não pode avaliar o mesmo evento mais de uma vez.

## 16. Dashboard administrativo

### Funcionalidades

- Mostrar eventos mais vendidos.
- Mostrar últimas compras.
- Mostrar capacidade.
- Mostrar formas de pagamento.
- Mostrar uso de cupons.
- Mostrar avaliações recentes.
- Mostrar check-ins realizados.

### Regras

- Dados cancelados/reembolsados devem ser tratados corretamente.
- Relatório deve evitar poluição visual.
- Cliente não deve ver funções administrativas.
- Lista de compras deve ser limitada/paginada para evitar lentidão em bases maiores.

## 17. Performance e manutenção

### Funcionalidades técnicas

- Cachear listas e filtros da página de eventos.
- Evitar reinicialização repetida do drag/drop do mapa de stands.
- Limitar imagens novas enviadas em base64.
- Criar índices SQLite idempotentes.
- Registrar logs simples de tempo em rotas pesadas.
- Calcular totais do dashboard por SQL agregado.

### Regras

- Melhorias de performance não devem remover rotas existentes.
- Índices devem usar `CREATE INDEX IF NOT EXISTS`.
- Imagens antigas devem continuar carregando.
- Query params novos devem manter compatibilidade quando não forem enviados.

## 18. Testes

Os testes automatizados devem cobrir:

- hash de senha;
- credencial administrativa;
- validação de usuário;
- validação de evento;
- validação de cupom;
- regras de reserva;
- tipos de ingresso;
- atividades;
- convidados;
- stands e expositores;
- check-in;
- avaliações.

## 19. Documentos relacionados

- `docs/adr.md`
- `docs/arch.md`
- `docs/vision.md`
- `docs/budget.md`
- `docs/lesson.md`
- `docs/visao.md`
- `docs/arquitetura.md`
- `docs/roadmap.md`
- `docs/pivotagem.md`
- `docs/roadmap-pivotagem.md`
- `docs/requisitos.md`
- `docs/checklist-entrega.md`
