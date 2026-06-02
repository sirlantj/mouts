-- ============================================================
-- SCHEMA + SEED DATA - Ambev Developer Evaluation
-- Executa no PostgreSQL (database: developer_evaluation)
-- ============================================================

-- Garante extensao pgcrypto (gen_random_uuid)
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Drop em ordem (FK first)
DROP TABLE IF EXISTS "SaleItems" CASCADE;
DROP TABLE IF EXISTS "Sales" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;

-- ============================================================
-- SCHEMA
-- ============================================================

CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Username" varchar(50) NOT NULL,
    "Password" varchar(100) NOT NULL,
    "Phone" varchar(20) NOT NULL,
    "Email" varchar(100) NOT NULL,
    "Status" varchar(20) NOT NULL,
    "Role" varchar(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "UpdatedAt" timestamp with time zone NULL
);

CREATE TABLE "Sales" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "SaleNumber" varchar(50) NOT NULL,
    "SaleDate" timestamp with time zone NOT NULL,
    "CustomerExternalId" varchar(100) NOT NULL,
    "CustomerName" varchar(200) NOT NULL,
    "BranchExternalId" varchar(100) NOT NULL,
    "BranchName" varchar(200) NOT NULL,
    "TotalAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "Status" varchar(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "UQ_Sales_SaleNumber" UNIQUE ("SaleNumber")
);

CREATE INDEX "IX_Sales_CustomerExternalId" ON "Sales" ("CustomerExternalId");
CREATE INDEX "IX_Sales_BranchExternalId" ON "Sales" ("BranchExternalId");

CREATE TABLE "SaleItems" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "SaleId" uuid NOT NULL,
    "ProductExternalId" varchar(100) NOT NULL,
    "ProductName" varchar(200) NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "Discount" numeric(18,2) NOT NULL DEFAULT 0,
    "TotalAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "IsCancelled" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT "FK_SaleItems_Sales" FOREIGN KEY ("SaleId") REFERENCES "Sales" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_SaleItems_SaleId" ON "SaleItems" ("SaleId");

-- ============================================================
-- USERS
-- Senha para todos: "Test@123"  (BCrypt hash, work factor 11)
-- ============================================================

INSERT INTO "Users" ("Id", "Username", "Email", "Phone", "Password", "Status", "Role", "CreatedAt")
VALUES
    ('a1b2c3d4-e5f6-4890-abcd-ef1234567890', 'Admin User',     'admin@ambev.com',     '(11) 99999-0001', '$2b$11$HkOj7ym5kgSthQQauSx9bOuRw1c7k0kZl/C5Qr3uXyXUY3hiqK.7G', 'Active',    'Admin',    '2025-01-15 10:00:00+00'),
    ('b2c3d4e5-f6a7-4901-bcde-f12345678901', 'Manager User',   'manager@ambev.com',   '(11) 99999-0002', '$2b$11$HkOj7ym5kgSthQQauSx9bOuRw1c7k0kZl/C5Qr3uXyXUY3hiqK.7G', 'Active',    'Manager',  '2025-02-01 10:00:00+00'),
    ('c3d4e5f6-a7b8-4012-cdef-123456789012', 'Customer User',  'customer@ambev.com',  '(11) 99999-0003', '$2b$11$HkOj7ym5kgSthQQauSx9bOuRw1c7k0kZl/C5Qr3uXyXUY3hiqK.7G', 'Active',    'Customer', '2025-03-01 10:00:00+00'),
    ('d4e5f6a7-b8c9-4123-defa-234567890123', 'Inactive User',  'inactive@ambev.com',  '(11) 99999-0004', '$2b$11$HkOj7ym5kgSthQQauSx9bOuRw1c7k0kZl/C5Qr3uXyXUY3hiqK.7G', 'Inactive',  'Customer', '2025-01-01 10:00:00+00'),
    ('e5f6a7b8-c9d0-4234-efab-345678901234', 'Suspended User', 'suspended@ambev.com', '(11) 99999-0005', '$2b$11$HkOj7ym5kgSthQQauSx9bOuRw1c7k0kZl/C5Qr3uXyXUY3hiqK.7G', 'Suspended', 'Manager',  '2025-01-01 10:00:00+00');

-- ============================================================
-- SALES + ITEMS - Cenarios cobertos:
--   SALE-001: Sem desconto (qty < 4)
--   SALE-002: 10% desconto (qty 4-9)
--   SALE-003: 20% desconto (qty 10-20)
--   SALE-004: Mix de descontos
--   SALE-005: Venda cancelada
--   SALE-006: Venda ativa com item cancelado
--   SALE-007: Filial diferente (Salvador)
--   SALE-008: Valor alto
--   SALE-009: Quantidade no limite (20)
--   SALE-010: Venda de hoje
-- ============================================================

-- SALE-001: Sem desconto
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000001', 'SALE-001', '2025-06-01 14:30:00+00', 'CUST-001', 'Distribuidora Silva', 'BR-SP-001', 'Filial Sao Paulo Centro', 75.00, 'Active', '2025-06-01 14:30:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt") VALUES
    ('20000000-0000-4000-8000-000000000001', '10000000-0000-4000-8000-000000000001', 'PROD-001', 'Brahma Lata 350ml', 2, 15.00, 0.00, 30.00, false, '2025-06-01 14:30:00+00'),
    ('20000000-0000-4000-8000-000000000002', '10000000-0000-4000-8000-000000000001', 'PROD-002', 'Skol Lata 350ml',   3, 15.00, 0.00, 45.00, false, '2025-06-01 14:30:00+00');

-- SALE-002: Desconto 10%
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000002', 'SALE-002', '2025-05-28 09:15:00+00', 'CUST-002', 'Bar do Joao', 'BR-RJ-001', 'Filial Rio de Janeiro', 216.00, 'Active', '2025-05-28 09:15:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt")
VALUES ('20000000-0000-4000-8000-000000000003', '10000000-0000-4000-8000-000000000002', 'PROD-003', 'Budweiser Long Neck 330ml', 6, 40.00, 0.10, 216.00, false, '2025-05-28 09:15:00+00');

-- SALE-003: Desconto 20%
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000003', 'SALE-003', '2025-05-20 16:45:00+00', 'CUST-003', 'Restaurante Mineiro', 'BR-MG-001', 'Filial Belo Horizonte', 384.00, 'Active', '2025-05-20 16:45:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt")
VALUES ('20000000-0000-4000-8000-000000000004', '10000000-0000-4000-8000-000000000003', 'PROD-004', 'Antarctica Original 600ml', 12, 40.00, 0.20, 384.00, false, '2025-05-20 16:45:00+00');

-- SALE-004: Mix de descontos
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000004', 'SALE-004', '2025-05-15 11:00:00+00', 'CUST-001', 'Distribuidora Silva', 'BR-SP-001', 'Filial Sao Paulo Centro', 645.00, 'Active', '2025-05-15 11:00:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt") VALUES
    ('20000000-0000-4000-8000-000000000005', '10000000-0000-4000-8000-000000000004', 'PROD-005', 'Guarana Antarctica 2L',         2, 25.00, 0.00,  50.00, false, '2025-05-15 11:00:00+00'),
    ('20000000-0000-4000-8000-000000000006', '10000000-0000-4000-8000-000000000004', 'PROD-006', 'Stella Artois Lata 350ml',      5, 30.00, 0.10, 135.00, false, '2025-05-15 11:00:00+00'),
    ('20000000-0000-4000-8000-000000000007', '10000000-0000-4000-8000-000000000004', 'PROD-007', 'Corona Extra Long Neck 330ml', 15, 38.33, 0.20, 460.00, false, '2025-05-15 11:00:00+00');

-- SALE-005: Venda cancelada
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt", "UpdatedAt")
VALUES ('10000000-0000-4000-8000-000000000005', 'SALE-005', '2025-04-10 08:00:00+00', 'CUST-004', 'Padaria Central', 'BR-SP-002', 'Filial Sao Paulo Zona Sul', 0.00, 'Cancelled', '2025-04-10 08:00:00+00', '2025-04-11 10:00:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt", "UpdatedAt")
VALUES ('20000000-0000-4000-8000-000000000008', '10000000-0000-4000-8000-000000000005', 'PROD-001', 'Brahma Lata 350ml', 8, 15.00, 0.10, 108.00, true, '2025-04-10 08:00:00+00', '2025-04-11 10:00:00+00');

-- SALE-006: Venda ativa com item cancelado parcial
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt", "UpdatedAt")
VALUES ('10000000-0000-4000-8000-000000000006', 'SALE-006', '2025-05-25 13:20:00+00', 'CUST-002', 'Bar do Joao', 'BR-RJ-001', 'Filial Rio de Janeiro', 60.00, 'Active', '2025-05-25 13:20:00+00', '2025-05-26 09:00:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt", "UpdatedAt") VALUES
    ('20000000-0000-4000-8000-000000000009', '10000000-0000-4000-8000-000000000006', 'PROD-008', 'Pepsi Lata 350ml',  3, 20.00, 0.00, 60.00, false, '2025-05-25 13:20:00+00', NULL),
    ('20000000-0000-4000-8000-000000000010', '10000000-0000-4000-8000-000000000006', 'PROD-009', 'Bohemia Lata 350ml', 4, 25.00, 0.10, 90.00, true,  '2025-05-25 13:20:00+00', '2025-05-26 09:00:00+00');

-- SALE-007: Filial Salvador
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000007', 'SALE-007', '2025-05-10 17:30:00+00', 'CUST-005', 'Mercadinho Nordeste', 'BR-BA-001', 'Filial Salvador', 320.00, 'Active', '2025-05-10 17:30:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt")
VALUES ('20000000-0000-4000-8000-000000000011', '10000000-0000-4000-8000-000000000007', 'PROD-010', 'Spaten Puro Malte 600ml', 10, 40.00, 0.20, 320.00, false, '2025-05-10 17:30:00+00');

-- SALE-008: Valor alto
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000008', 'SALE-008', '2025-05-05 10:00:00+00', 'CUST-006', 'Atacadao Bebidas LTDA', 'BR-PR-001', 'Filial Curitiba', 5600.00, 'Active', '2025-05-05 10:00:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt") VALUES
    ('20000000-0000-4000-8000-000000000012', '10000000-0000-4000-8000-000000000008', 'PROD-011', 'Whisky Old Parr 750ml',             20, 150.00, 0.20, 2400.00, false, '2025-05-05 10:00:00+00'),
    ('20000000-0000-4000-8000-000000000013', '10000000-0000-4000-8000-000000000008', 'PROD-012', 'Whisky Johnnie Walker Black 750ml', 20, 200.00, 0.20, 3200.00, false, '2025-05-05 10:00:00+00');

-- SALE-009: Quantidade no limite (20 unidades)
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000009', 'SALE-009', '2025-05-18 15:00:00+00', 'CUST-003', 'Restaurante Mineiro', 'BR-MG-001', 'Filial Belo Horizonte', 480.00, 'Active', '2025-05-18 15:00:00+00');

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt")
VALUES ('20000000-0000-4000-8000-000000000014', '10000000-0000-4000-8000-000000000009', 'PROD-013', 'Wals Trippel 600ml', 20, 30.00, 0.20, 480.00, false, '2025-05-18 15:00:00+00');

-- SALE-010: Venda de hoje
INSERT INTO "Sales" ("Id", "SaleNumber", "SaleDate", "CustomerExternalId", "CustomerName", "BranchExternalId", "BranchName", "TotalAmount", "Status", "CreatedAt")
VALUES ('10000000-0000-4000-8000-000000000010', 'SALE-010', now(), 'CUST-001', 'Distribuidora Silva', 'BR-SP-001', 'Filial Sao Paulo Centro', 45.00, 'Active', now());

INSERT INTO "SaleItems" ("Id", "SaleId", "ProductExternalId", "ProductName", "Quantity", "UnitPrice", "Discount", "TotalAmount", "IsCancelled", "CreatedAt")
VALUES ('20000000-0000-4000-8000-000000000015', '10000000-0000-4000-8000-000000000010', 'PROD-014', 'Cerveja Patagonia IPA 355ml', 1, 45.00, 0.00, 45.00, false, now());

-- ============================================================
-- Resumo
-- ============================================================
SELECT 'Users'     AS tabela, COUNT(*) AS total FROM "Users"
UNION ALL
SELECT 'Sales'     AS tabela, COUNT(*) AS total FROM "Sales"
UNION ALL
SELECT 'SaleItems' AS tabela, COUNT(*) AS total FROM "SaleItems";
