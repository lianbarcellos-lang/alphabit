# Historias de Usuario - GeekTop

Este documento consolida as historias de usuario atuais do GeekTop. Os arquivos antigos `docs/Historia usuario`, `docs/História de Usuário` e `docs/Raphael` foram mantidos apenas como historico/apoio e nao representam mais o escopo final da pivotagem.

## Epico 1 - Identidade e descoberta

### HU01 - Vitrine de eventos geek
Como cliente, quero visualizar eventos geek com imagem, categoria, cidade, data, preco e avaliacao, para escolher rapidamente uma experiencia interessante.

Critérios de aceite:

- os eventos devem aparecer com imagem/capa;
- o preco deve ficar visivel;
- a media de avaliacoes deve aparecer junto ao preco;
- eventos sem avaliacao devem exibir `Sem avaliações`.

### HU02 - Filtros de descoberta
Como cliente, quero filtrar eventos por cidade, dia da semana e categoria geek, para encontrar opcoes mais relevantes.

Critérios de aceite:

- os filtros devem funcionar juntos;
- a lista deve atualizar conforme os filtros;
- deve existir acao para limpar filtros.

### HU03 - Detalhe do evento
Como cliente, quero abrir o detalhe de um evento, para ver local, cidade, data, preco, convidados, atividades, mapa de stands e avaliacoes antes da compra.

Critérios de aceite:

- o detalhe deve mostrar os dados principais do evento;
- card e botao `Ver ingressos` devem abrir o detalhe por link real;
- deve haver caminho para comprar;
- convidados, atividades, mapa de stands e avaliacoes devem aparecer quando existirem.

## Epico 2 - Conta e seguranca

### HU04 - Cadastro de cliente
Como cliente, quero criar uma conta com CPF, nome, email e senha, para poder comprar ingressos.

Critérios de aceite:

- CPF duplicado deve ser bloqueado;
- email duplicado deve ser bloqueado;
- senha deve ser armazenada como hash.

### HU05 - Login de cliente
Como cliente, quero entrar com email ou CPF e senha, para acessar reservas, perfil e compras.

Critérios de aceite:

- credenciais validas autenticam o usuario;
- credenciais invalidas retornam erro claro.
- recuperacao de senha nao deve aparecer enquanto nao houver envio de email validado.

### HU06 - Protecao de dados
Como sistema, quero restringir perfil e reservas ao proprio cliente ou administrador, para proteger dados pessoais.

Critérios de aceite:

- cliente nao acessa reserva de outro CPF;
- administrador pode acessar dados necessarios para operacao.

## Epico 3 - Compra e reservas

### HU08 - Tipos de ingresso
Como cliente, quero escolher entre tipos de ingresso com precos e beneficios diferentes, para comprar a experiencia desejada.

Critérios de aceite:

- tipos devem pertencer ao evento;
- preco final deve usar o tipo selecionado;
- disponibilidade do tipo deve ser respeitada.

### HU09 - Escolha de ingressos
Como cliente, quero escolher tipo e quantidade de ingresso sem depender de assento marcado, para montar minha compra de forma simples.

Critérios de aceite:

- a reserva principal deve funcionar como entrada geral sem assento marcado;
- o cliente deve escolher tipo de ingresso;
- o cliente deve escolher quantidade;
- vagas limitadas devem ficar nos tipos de ingresso e nas atividades internas.

### HU10 - Carrinho e cupom
Como cliente, quero aplicar cupom no carrinho e ver o desconto antes de finalizar, para conhecer o valor final.

Critérios de aceite:

- cupom inexistente deve ser rejeitado;
- valor minimo deve ser respeitado;
- desconto deve aparecer antes da compra.

### HU11 - Finalizacao da reserva
Como cliente, quero finalizar a compra, para garantir minha reserva no evento.

Critérios de aceite:

- usuario deve existir;
- evento deve existir;
- capacidade deve ser respeitada;
- reserva deve registrar forma de pagamento, status e codigo do pedido.

### HU12 - Historico de reservas
Como cliente, quero consultar minhas reservas, para acompanhar meus ingressos e acessar o QR Code.

Critérios de aceite:

- historico deve mostrar evento, data, valor e status;
- QR Code deve estar disponivel para reservas;
- cliente deve conseguir ampliar o QR Code para leitura.

## Epico 4 - Programacao e convidados

### HU13 - Atividades internas
Como cliente, quero me inscrever em atividades internas, para participar da programacao do evento.

Critérios de aceite:

- atividade deve ter limite de vagas;
- inscricao duplicada deve ser bloqueada;
- limite de participantes deve ser respeitado.
- cliente inscrito deve poder cancelar a propria inscricao.

### HU14 - Convidados do evento
Como cliente, quero visualizar convidados associados ao evento, para decidir se o evento e relevante para mim.

Critérios de aceite:

- detalhe do evento deve mostrar convidados;
- administrador deve conseguir cadastrar convidado;
- administrador deve conseguir associar e remover convidado de evento.

## Epico 5 - Mapa de stands e expositores

### HU15 - Visualizacao do mapa do evento
Como cliente, quero visualizar o mapa de stands do evento, para saber onde ficam empresas, lojas, arenas e atrações.

Critérios de aceite:

- o detalhe do evento deve mostrar o mapa em blocos;
- quando houver planta cadastrada, o painel administrativo deve mostrar a imagem com os stands posicionados;
- os blocos devem aparecer organizados por linhas/setores;
- stands reservados devem mostrar o nome do ocupante;
- stands livres devem aparecer como disponíveis;
- cliente nao pode alterar alocacoes.

### HU16 - Alocacao administrativa de stands
Como administrador, quero selecionar stands no mapa e reservar espaços para empresas expositoras ou atrações, para organizar o evento.

Critérios de aceite:

- administrador deve escolher o evento;
- administrador deve poder enviar uma imagem da planta;
- administrador deve poder cadastrar um stand;
- administrador deve poder criar, renomear e excluir linhas vazias;
- administrador deve poder aplicar uma organização automática por grade compacta visivel 3x3 ou 4x4;
- sistema deve desabilitar grades que nao comportam a quantidade atual de stands;
- administrador deve poder arrastar o stand para a posição desejada na planta;
- administrador deve poder editar ou excluir um stand diretamente na lista da linha;
- administrador deve informar nome, tipo e descricao do ocupante;
- sistema deve salvar as coordenadas do stand e recarregar a posição ao abrir novamente;
- administrador deve poder liberar um stand;
- alteracoes exigem token administrativo.

## Epico 6 - Check-in

### HU17 - QR Code da reserva
Como cliente, quero receber um QR Code da reserva, para apresentar na entrada do evento.

Critérios de aceite:

- cada reserva deve ter codigo unico;
- QR Code deve representar esse codigo;
- cliente deve conseguir ampliar o QR Code.

### HU18 - Check-in administrativo
Como administrador, quero validar a entrada por codigo manual ou camera/webcam, para controlar acesso ao evento.

Critérios de aceite:

- check-in valido deve ser aceito;
- check-in duplicado deve ser bloqueado;
- reserva cancelada deve ser rejeitada;
- codigo invalido deve retornar mensagem clara.

## Epico 7 - Avaliacoes

### HU19 - Avaliacao pos-evento
Como cliente, quero avaliar um evento reservado, para registrar minha experiencia e ajudar outros clientes.

Critérios de aceite:

- cliente precisa ter reserva;
- nota deve ser valida;
- avaliacao duplicada por cliente/evento deve ser bloqueada.

### HU20 - Media de avaliacoes
Como cliente ou administrador, quero ver a media de avaliacoes junto ao preco do evento, para comparar preco e qualidade percebida.

Critérios de aceite:

- media deve aparecer na vitrine, detalhe e admin;
- total de avaliacoes deve aparecer quando houver avaliacao;
- eventos sem avaliacao devem exibir `Sem avaliações`.

### HU21 - Moderacao de avaliacoes
Como administrador, quero remover avaliacoes indevidas, para manter a vitrine confiavel.

Critérios de aceite:

- apenas administrador pode remover avaliacao;
- dashboard deve refletir a remocao.

## Epico 8 - Administracao e relatorios

### HU22 - Gestao de eventos
Como administrador, quero cadastrar, editar e excluir eventos, para manter a agenda atualizada.

Critérios de aceite:

- evento cadastrado deve aparecer na vitrine;
- edicao deve atualizar os dados;
- exclusao deve remover o evento das listas.

### HU23 - Catalogos administrativos
Como administrador, quero gerenciar cidades e categorias geek, para padronizar os eventos.

Critérios de aceite:

- cidade/categoria deve poder ser criada, editada e removida;
- cadastro de evento deve reutilizar esses catalogos.

### HU24 - Gestao de cupons
Como administrador, quero criar, editar e excluir cupons, para gerenciar campanhas promocionais.

Critérios de aceite:

- cupom deve ter codigo, desconto e valor minimo;
- carrinho deve validar as regras do cupom.

### HU25 - Dashboard administrativo
Como administrador, quero visualizar relatorio de vendas e operacao, para acompanhar desempenho dos eventos.

Critérios de aceite:

- dashboard deve mostrar eventos mais vendidos;
- deve mostrar compras recentes sem poluir com dados desnecessarios;
- deve mostrar formas de pagamento, capacidade, cupons, avaliacoes e check-ins.

## Epico 9 - Documentacao e qualidade

### HU26 - Documentacao do projeto
Como avaliador ou desenvolvedor, quero consultar visao, arquitetura, specs, roadmap e requisitos, para entender o projeto e sua evolucao.

Critérios de aceite:

- `docs/visao.md` deve explicar a ideia do projeto;
- `docs/arquitetura.md` deve explicar a estrutura tecnica;
- `docs/specs.md` deve listar especificacoes;
- `docs/roadmap.md` deve mostrar ordem, dependencias e status;
- `docs/requisitos.md` deve registrar requisitos e criterios de aceite.

### HU27 - Testes automatizados
Como desenvolvedor, quero manter testes automatizados, para validar regras principais e reduzir regressao.

Critérios de aceite:

- `dotnet build .\Alphabit.sln` deve passar;
- `dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore` deve passar;
- regras de usuario, evento, cupom, reserva, convidados, atividades, stands, check-in e avaliacoes devem ter cobertura.
