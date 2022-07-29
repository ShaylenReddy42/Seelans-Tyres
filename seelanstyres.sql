DROP DATABASE IF EXISTS SeelansTyres;
GO
CREATE DATABASE SeelansTyres;
GO

USE SeelansTyres;
GO

CREATE TABLE Customers
(
    CustomerId int IDENTITY(1,1) NOT NULL 
        CONSTRAINT PK_Customers_CustomerId PRIMARY KEY CLUSTERED,
    FirstName varchar(50) NOT NULL,
    LastName varchar(50) NOT NULL
);
GO


INSERT INTO Customers (
    FirstName,
    LastName
)
VALUES
    ('Former', 'User', 'formeruser@seelanstyres.co.za', '0000000000', 'N/A', 'N/A');

CREATE TABLE Addresses
(
    AddressId int IDENTITY(1,1) NOT NULL 
        CONSTRAINT PK_Addresses_AddressId PRIMARY KEY CLUSTERED,
    Home varchar(50) NOT NULL,
    Street varchar(50) NOT NULL,
    City varchar(50) NOT NULL,
    PostalCode int NOT NULL,
    PrefferedAddress int NOT NULL,
    CustomerId int NOT NULL
        CONSTRAINT FK_Addresses_CustomerId_Customers_CustomerId
            FOREIGN KEY REFERENCES Customers (CustomerId)
                ON DELETE CASCADE
);
GO

INSERT INTO Addresses (
    Home,
    Street,
    City,
    PostalCode,
    PrefferedAddress,
    CustomerId
)
VALUES
    ('Dawood Cl', 'Dolphin Coast', 'Ballito', 4420, 1, 1);

CREATE TABLE Brands (
  BrandId int NOT NULL
    CONSTRAINT PK_Brands_BrandId PRIMARY KEY CLUSTERED,
  BrandName varchar(100) NOT NULL
);
GO

INSERT INTO Brands (BrandId, BrandName)
VALUES
    (1, 'BFGoodrich'),
    (2, 'Continental'),
    (3, 'Goodyear'),
    (4, 'Hankook'),
    (5, 'Michelin'),
    (6, 'Pirelli');

CREATE TABLE Tyres
(
    TyreID int IDENTITY(1,1) NOT NULL 
        CONSTRAINT PK_Tyres_TyreId PRIMARY KEY CLUSTERED,
    Name text NOT NULL,
    Width int NOT NULL,
    Ratio int NOT NULL,
    Diameter int NOT NULL,
    VehicleType varchar(40) NOT NULL,
    Price numeric(7,2) NOT NULL,
    Available bit NOT NULL DEFAULT 1,
    ImageURL varchar(MAX),
    BrandId int NOT NULL
        CONSTRAINT FK_Tyres_BrandId_Brands_BrandId
            FOREIGN KEY REFERENCES Brands (BrandId)
                ON DELETE CASCADE
);
GO

CREATE TABLE Orders
(
    OrderId int IDENTITY(1,1) NOT NULL 
        CONSTRAINT PK_Orders_OrderId PRIMARY KEY CLUSTERED,
    OrderTime datetime NOT NULL,
    TotalItems int NOT NULL,
    TotalPrice real NOT NULL,
    Delivered bit NOT NULL 
        CONSTRAINT DF_Orders_Delivered_False DEFAULT 0,
    CustomerId int NOT NULL
        CONSTRAINT FK_Orders_CustomerId_Customers_CustomerId
            FOREIGN KEY REFERENCES Customers (CustomerId)
                ON DELETE CASCADE,
    AddressId int NOT NULL
        CONSTRAINT FK_Orders_AddressId_Addresses_AddressId
            FOREIGN KEY REFERENCES Addresses (AddressId)
);
GO

CREATE NONCLUSTERED INDEX IDX_Orders_CustomerId ON Orders (CustomerId);
GO

CREATE TABLE OrderItems
(
    ItemsId int IDENTITY(1,1) NOT NULL 
        CONSTRAINT PK_OrderItems_DetailId PRIMARY KEY CLUSTERED,
    Quantity int NOT NULL,
    TyreId int NOT NULL
        CONSTRAINT FK_OrderItems_TyreId_Tyres_TyreId
            FOREIGN KEY REFERENCES Tyres (TyreId)
                ON DELETE CASCADE
        CONSTRAINT UQ_OrderItems_TyreId UNIQUE,
    OrderId int NOT NULL
        CONSTRAINT FK_OrderItems_OrderId_Orders_OrderId
            FOREIGN KEY REFERENCES Orders (OrderId)
                ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX IDX_OrderItems_OrderId ON OrderItems (OrderId);
GO