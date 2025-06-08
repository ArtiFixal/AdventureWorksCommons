USE AdventureWorks;
GO

IF OBJECT_ID('igs.GenerateInvoice', 'P') IS NOT NULL
    DROP PROCEDURE igs.GenerateInvoice;
GO

IF OBJECT_ID('igs.InvoiceLine', 'U') IS NOT NULL
    DROP TABLE igs.InvoiceLine;
GO

IF OBJECT_ID('igs.Invoice', 'U') IS NOT NULL
    DROP TABLE igs.Invoice;
GO

IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'igs')
    DROP SCHEMA igs;
GO

CREATE SCHEMA igs;
GO

CREATE TABLE igs.Invoice (
    InvoiceID INT IDENTITY PRIMARY KEY,
    SalesOrderID INT NOT NULL,
    CustomerID INT NOT NULL,
    InvoiceDate DATETIME DEFAULT GETDATE(),
    TotalAmount MONEY,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE igs.InvoiceLine (
    InvoiceLineID INT IDENTITY PRIMARY KEY,
    InvoiceID INT FOREIGN KEY REFERENCES igs.Invoice(InvoiceID),
    ProductID INT,
    Quantity INT,
    UnitPrice MONEY,
    LineTotal MONEY
);
GO

CREATE OR ALTER PROCEDURE igs.GenerateInvoice
    @SalesOrderID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CustomerID INT;
    DECLARE @InvoiceID INT;
    DECLARE @TotalAmount MONEY;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @CustomerID = soh.CustomerID
        FROM Sales.SalesOrderHeader soh
        WHERE soh.SalesOrderID = @SalesOrderID;

        IF @CustomerID IS NULL
        BEGIN
            THROW 50001, 'Sales Order with the specified ID does not exist.', 1;
        END

        SELECT @TotalAmount = SUM(UnitPrice * OrderQty)
        FROM Sales.SalesOrderDetail
        WHERE SalesOrderID = @SalesOrderID;

        INSERT INTO igs.Invoice (SalesOrderID, CustomerID, TotalAmount)
        VALUES (@SalesOrderID, @CustomerID, @TotalAmount);

        SET @InvoiceID = SCOPE_IDENTITY();

        INSERT INTO igs.InvoiceLine (InvoiceID, ProductID, Quantity, UnitPrice, LineTotal)
        SELECT
            @InvoiceID,
            ProductID,
            OrderQty,
            UnitPrice,
            UnitPrice * OrderQty
        FROM Sales.SalesOrderDetail
        WHERE SalesOrderID = @SalesOrderID;

        COMMIT TRANSACTION;

        PRINT 'Invoice generated successfully. Invoice ID: ' + CAST(@InvoiceID AS VARCHAR);
    END TRY
    BEGIN CATCH
		ROLLBACK TRANSACTION;
		DECLARE @ErrorMessage NVARCHAR(255)='An error occurred while generating the invoice: ' + CAST(ERROR_NUMBER() AS NVARCHAR);

        THROW 50010, @ErrorMessage, 1;
    END CATCH
END;
GO
