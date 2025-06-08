USE AdventureWorks;
GO

IF OBJECT_ID('mms.ReplenishInventory', 'P') IS NOT NULL
    DROP PROCEDURE mms.ReplenishInventory;
GO

IF OBJECT_ID('mms.UpdateInventoryQuantity', 'P') IS NOT NULL
    DROP PROCEDURE mms.UpdateInventoryQuantity;
GO

IF OBJECT_ID('mms.StockChangeLog', 'U') IS NOT NULL
    DROP TABLE mms.StockChangeLog;
GO

IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'mms')
    DROP SCHEMA mms;
GO

CREATE SCHEMA mms;
GO

CREATE TABLE mms.StockChangeLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    LocationID SMALLINT NOT NULL,
    OldQuantity SMALLINT,
    NewQuantity SMALLINT,
    ChangeDate DATETIME DEFAULT GETDATE(),
    ChangedBy SYSNAME DEFAULT SYSTEM_USER
);
GO

CREATE OR ALTER PROCEDURE mms.ReplenishInventory
    @ReplenishQty SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @ProductID INT,
        @LocationID SMALLINT,
        @Quantity SMALLINT,
        @SafetyStock SMALLINT,
        @NewQty SMALLINT;

    DECLARE inventory_cursor CURSOR FOR
        SELECT pi.ProductID, pi.LocationID, pi.Quantity
        FROM Production.ProductInventory pi
        INNER JOIN Production.Product p ON pi.ProductID = p.ProductID;

    BEGIN TRY
        OPEN inventory_cursor;
        FETCH NEXT FROM inventory_cursor INTO @ProductID, @LocationID, @Quantity;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SELECT @SafetyStock = SafetyStockLevel
            FROM Production.Product
            WHERE ProductID = @ProductID;

            IF @Quantity < @SafetyStock
            BEGIN
                BEGIN TRANSACTION;

                SET @NewQty = @Quantity + @ReplenishQty;

                UPDATE Production.ProductInventory
                SET Quantity = @NewQty,
                    ModifiedDate = SYSDATETIME()
                OUTPUT 
                    inserted.ProductID,
                    inserted.LocationID,
                    deleted.Quantity AS OldQuantity,
                    inserted.Quantity AS NewQuantity,
                    GETDATE(),
                    SYSTEM_USER
                INTO mms.StockChangeLog(ProductID, LocationID, OldQuantity, NewQuantity, ChangeDate, ChangedBy)
                WHERE ProductID = @ProductID AND LocationID = @LocationID;

                COMMIT TRANSACTION;
            END

            FETCH NEXT FROM inventory_cursor INTO @ProductID, @LocationID, @Quantity;
        END

        CLOSE inventory_cursor;
        DEALLOCATE inventory_cursor;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        CLOSE inventory_cursor;
        DEALLOCATE inventory_cursor;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE mms.UpdateInventoryQuantity
    @ProductID INT,
    @LocationID SMALLINT,
    @NewQuantity SMALLINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldQuantity SMALLINT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1 FROM Production.ProductInventory
            WHERE ProductID = @ProductID AND LocationID = @LocationID
        )
        BEGIN
            SELECT @OldQuantity = Quantity
            FROM Production.ProductInventory
            WHERE ProductID = @ProductID AND LocationID = @LocationID;

            UPDATE Production.ProductInventory
            SET Quantity = @NewQuantity,
                ModifiedDate = SYSDATETIME()
            WHERE ProductID = @ProductID AND LocationID = @LocationID;

            INSERT INTO mms.StockChangeLog(ProductID, LocationID, OldQuantity, NewQuantity)
            VALUES(@ProductID, @LocationID, @OldQuantity, @NewQuantity);
        END
        ELSE
        BEGIN
            INSERT INTO Production.ProductInventory(ProductID, LocationID, Shelf, Bin, Quantity, Rowguid, ModifiedDate)
            VALUES(@ProductID, @LocationID, 'A', 0, @NewQuantity, NEWID(), SYSDATETIME());

            INSERT INTO mms.StockChangeLog(ProductID, LocationID, OldQuantity, NewQuantity)
            VALUES(@ProductID, @LocationID, 0, @NewQuantity);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
