# Visao do Projeto - GeekTop

## Ideia central

O GeekTop e uma plataforma de eventos geek criada para divulgar, vender e controlar experiencias como BGS, eventos de anime, cosplay, card games, cultura pop, streamers, dubladores, artistas e comunidades de fas.

A proposta e permitir que o cliente encontre eventos, veja detalhes, escolha ingressos, use cupons, finalize reservas, acesse o historico e apresente um QR Code para entrada no evento.

## Problema que o projeto resolve

Eventos geek normalmente envolvem varias informacoes importantes: local, cidade, categoria, convidados, atividades, tipos de ingresso, capacidade, reservas, check-in e avaliacoes. O GeekTop centraliza esse fluxo em uma unica aplicacao, separando a experiencia do cliente da operacao administrativa.

## Publico-alvo

- Clientes interessados em eventos geek, anime, games, cosplay e cultura pop.
- Administradores responsaveis por cadastrar eventos, acompanhar vendas e validar entradas.
- Organizadores que precisam controlar convidados, atividades, cupons, reservas e check-ins.

## Experiencia do cliente

O cliente pode:

- criar conta e fazer login;
- navegar pela vitrine de eventos;
- filtrar eventos por cidade, data, atracao e categoria geek;
- abrir o detalhe do evento;
- visualizar convidados e atividades;
- escolher tipo e quantidade de ingresso;
- aplicar cupom no carrinho;
- finalizar reserva;
- acessar historico de compras;
- abrir e ampliar o QR Code da reserva;
- inscrever-se e cancelar inscricao em atividades internas;
- avaliar eventos reservados.

## Experiencia administrativa

O administrador pode:

- acessar o painel administrativo;
- cadastrar, editar e excluir eventos;
- gerenciar cidades e categorias geek;
- cadastrar e editar cupons;
- cadastrar convidados;
- associar convidados aos eventos;
- acompanhar relatorio de vendas;
- visualizar compras, formas de pagamento, capacidade e ranking de eventos;
- moderar avaliacoes;
- validar check-in por digitacao manual ou leitura de QR Code pela camera.

## Objetivo da pivotagem

O projeto nasceu com uma base de venda de ingressos e foi pivotado para uma plataforma de eventos geek. A pivotagem manteve a stack original e adaptou dominio, textos, imagens, regras e funcionalidades para o universo GeekTop.

## Stack mantida

- C#
- .NET 9
- ASP.NET Core Minimal API
- Blazor Server / Razor Components
- SQLite
- Dapper
- xUnit

## Fora do escopo atual

Nao fazem parte desta entrega:

- Entity Framework;
- JWT;
- MariaDB;
- SQL Server;
- Docker obrigatorio;
- Clean Architecture;
- Microservices;
- CQRS;
- MediatR.

## Estado atual

O projeto ja possui vitrine publica, fluxo de compra, reservas, cupons, tipos de ingresso, convidados, atividades, avaliacoes, dashboard administrativo e check-in com QR Code.
