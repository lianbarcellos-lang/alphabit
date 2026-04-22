CREATE TABLE Usuarios (
    Cpf TEXT PRIMARY KEY,
    Nome TEXT NOT NULL,
    Email TEXT NOT NULL
);

CREATE TABLE Eventos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    CapacidadeTotal INTEGER NOT NULL,
    DataEvento TEXT NOT NULL,
    PrecoPadrao REAL NOT NULL
);

CREATE TABLE Cupons (
    Codigo TEXT PRIMARY KEY,
    PorcentagemDesconto REAL NOT NULL,
    ValorMinimoRegra REAL NOT NULL
);

CREATE TABLE Reservas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioCpf TEXT NOT NULL,
    EventoId INTEGER NOT NULL,
    CupomUtilizado TEXT NULL,
    ValorFinalPago REAL NOT NULL,
    FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf),
    FOREIGN KEY (EventoId) REFERENCES Eventos(Id),
    FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo)
);
