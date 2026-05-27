# Specs - GeekTop

Este documento resume as especificacoes funcionais e tecnicas do GeekTop.

## 1. Identidade do projeto

Nome: GeekTop.

Proposta: plataforma de eventos geek/anime/games/cultura pop, com compra de ingressos, reservas, convidados, atividades, avaliacoes e check-in.

## 2. Stack

- C#
- .NET 9
- ASP.NET Core Minimal API
- Blazor Server / Razor Components
- SQLite
- Dapper
- xUnit

## 3. Usuarios

### Funcionalidades

- Cadastro de cliente.
- Login com email ou CPF.
- Recuperacao de senha.
- Perfil do usuario.
- Protecao de reservas por CPF.

### Regras

- CPF nao pode duplicar.
- Email nao pode duplicar.
- Senha de cliente e armazenada como hash.
- Perfil e reservas devem ser acessados apenas pelo proprio cliente ou admin.

## 4. Administrador

### Funcionalidades

- Login administrativo.
- Painel de controle.
- Gestao de eventos.
- Gestao de cupons.
- Gestao de cidades.
- Gestao de categorias geek.
- Gestao de convidados.
- Relatorio de vendas.
- Moderacao de avaliacoes.
- Check-in.

### Regras

- Rotas administrativas exigem token administrativo.
- Cliente nao deve acessar funcoes administrativas.

## 5. Eventos

### Funcionalidades

- Listar eventos publicos.
- Ver detalhe do evento.
- Cadastrar evento.
- Editar evento.
- Excluir evento.
- Exibir imagem/capa.
- Exibir categoria, cidade, local, data, preco e media de avaliacao.

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

### Regras

- Evento precisa ter dados obrigatorios.
- Capacidade deve ser respeitada nas reservas.
- Imagens devem representar o universo geek, games, anime ou cosplay.

## 6. Filtros de descoberta

### Funcionalidades

- Filtrar por cidade.
- Filtrar por dia da semana.
- Filtrar por atracao.
- Filtrar por categoria geek.

### Regras

- Filtros devem funcionar combinados.
- Botao de limpar filtros deve restaurar a lista.

## 7. Tipos de ingresso

### Funcionalidades

- Criar tipos de ingresso por evento.
- Exibir beneficios.
- Usar preco do tipo selecionado no carrinho/reserva.

### Tipos esperados

- Normal
- VIP
- Premium
- Day Pass
- Meet and Greet

### Regras

- Tipo de ingresso deve pertencer ao evento.
- Nao deve vender acima da quantidade disponivel.
- Meet and Greet deve ter limite de vagas.

## 8. Escolha de ingressos

### Funcionalidades

- Escolher tipo de ingresso.
- Escolher quantidade de ingressos.
- Adicionar ingresso ao carrinho.
- Tratar a reserva principal como entrada geral sem assento marcado.

### Regras

- Cada CPF pode reservar ate 2 ingressos por evento.
- A disponibilidade deve respeitar capacidade total do evento.
- A disponibilidade deve respeitar quantidade do tipo de ingresso.
- Vagas limitadas especificas devem ser controladas nas atividades internas, como workshop, torneio e meet and greet.

## 9. Carrinho e cupons

### Funcionalidades

- Adicionar ingresso ao carrinho.
- Aplicar cupom.
- Pre-visualizar desconto.
- Escolher forma de pagamento.
- Finalizar compra.

### Regras

- Cupom deve respeitar valor minimo.
- Desconto deve ser aplicado antes da reserva.
- Forma de pagamento deve ser registrada.

## 10. Reservas

### Funcionalidades

- Criar reserva.
- Consultar reservas por CPF autorizado.
- Mostrar historico do cliente.
- Mostrar QR Code da reserva.

### Regras

- Usuario deve existir.
- Evento deve existir.
- Reserva deve respeitar capacidade.
- Deve existir limite por CPF/evento.
- Reserva deve gerar codigo de pedido.
- Reserva deve criar QR Code unico para check-in.

## 11. Atividades internas

### Funcionalidades

- Cadastrar atividades internas.
- Listar atividades no detalhe do evento.
- Inscrever cliente em atividade.
- Cancelar inscricao do cliente em atividade.

### Regras

- Atividade deve pertencer a evento existente.
- Nao permitir inscricao duplicada.
- Nao ultrapassar limite de participantes.
- Ao cancelar inscricao, a vaga deve voltar a ficar disponivel.

## 12. Convidados

### Funcionalidades

- Cadastrar convidado.
- Associar convidado ao evento.
- Remover associacao.
- Exibir convidados no detalhe do evento.

### Tipos esperados

- Cosplayer
- Streamer
- Influencer
- Voice Actor
- Banda
- Artista
- Youtuber

## 13. Check-in com QR Code

### Funcionalidades

- Gerar QR Code por reserva.
- Exibir QR Code para o cliente.
- Ampliar QR Code para facilitar leitura.
- Validar codigo manualmente no admin.
- Validar QR Code por camera/webcam no admin.

### Regras

- QR Code deve ser unico.
- Check-in duplicado deve ser bloqueado.
- Reserva cancelada deve ser rejeitada.
- Codigo invalido deve retornar mensagem clara.

## 14. Avaliacoes

### Funcionalidades

- Cliente avalia evento reservado.
- Sistema mostra avaliacoes no detalhe do evento.
- Sistema mostra media da avaliacao junto ao preco.
- Admin pode moderar/remover avaliacao.

### Regras

- Nota deve ser valida.
- Usuario so pode avaliar evento reservado.
- Usuario nao pode avaliar o mesmo evento mais de uma vez.

## 15. Dashboard administrativo

### Funcionalidades

- Mostrar eventos mais vendidos.
- Mostrar ultimas compras.
- Mostrar capacidade.
- Mostrar formas de pagamento.
- Mostrar uso de cupons.
- Mostrar avaliacoes recentes.
- Mostrar check-ins realizados.

### Regras

- Dados cancelados/reembolsados devem ser tratados corretamente.
- Relatorio deve evitar poluicao visual.
- Cliente nao deve ver funcoes administrativas.

## 16. Testes

Os testes automatizados devem cobrir:

- hash de senha;
- credencial administrativa;
- validacao de usuario;
- validacao de evento;
- validacao de cupom;
- regras de reserva;
- tipos de ingresso;
- atividades;
- convidados;
- check-in;
- avaliacoes.

## 17. Documentos relacionados

- `docs/visao.md`
- `docs/arquitetura.md`
- `docs/roadmap.md`
- `docs/pivotagem.md`
- `docs/roadmap-pivotagem.md`
- `docs/requisitos.md`
- `docs/checklist-entrega.md`
