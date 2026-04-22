-- Tabela de Usuários
CREATE TABLE IF NOT EXISTS Usuarios (
    Cpf TEXT PRIMARY KEY,
    Nome TEXT NOT NULL,
    Email TEXT NOT NULL
);

-- Tabela de Eventos
CREATE TABLE IF NOT EXISTS Eventos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    CapacidadeTotal INTEGER NOT NULL,
    DataEvento TEXT NOT NULL,
    PrecoPadrao REAL NOT NULL
);

-- Tabela de Cupons
CREATE TABLE IF NOT EXISTS Cupons (
    Codigo TEXT PRIMARY KEY,
    PorcentagemDesconto REAL NOT NULL,
    ValorMinimoRegra REAL NOT NULL
);

-- Tabela de Reservas
CREATE TABLE IF NOT EXISTS Reservas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
    UsuarioCpf TEXT NOT NULL,
    EventoId INTEGER NOT NULL,
    CupomUtilizado TEXT,
    ValorFinalPago DECIMAL NOT NULL,

    FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf),
    FOREIGN KEY (EventoId) REFERENCES Eventos(Id),
    FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo)
);