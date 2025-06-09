IF OBJECT_ID('ebs.EmployeeBenefit', 'U') IS NOT NULL
    DROP TABLE ebs.EmployeeBenefit;
GO

IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'ebs')
    DROP SCHEMA ebs;
GO

CREATE SCHEMA ebs;
GO

CREATE TABLE ebs.EmployeeBenefit (
    BenefitID INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeID INT NOT NULL,
    ProductID INT NOT NULL,
    AssignedDate DATETIME NOT NULL DEFAULT GETDATE(),
    RedeemedDate DATETIME NULL,
    DiscountPercent DECIMAL(5,2) NOT NULL,
    Redeemed BIT NOT NULL DEFAULT 0
);
GO

CREATE OR ALTER FUNCTION ebs.GetUnpopularProducts()
RETURNS TABLE
AS
RETURN
(
    SELECT p.ProductID, p.Name, ISNULL(s.Quantity, 0) AS StockLevel
    FROM Production.Product p
    LEFT JOIN 
    (
        SELECT ProductID, SUM(OrderQty) AS TotalOrdered
        FROM Sales.SalesOrderDetail
        GROUP BY ProductID
    ) so ON p.ProductID = so.ProductID
    LEFT JOIN 
    (
        SELECT ProductID, SUM(Quantity) AS Quantity
        FROM Production.ProductInventory
        GROUP BY ProductID
    ) s ON p.ProductID = s.ProductID
    WHERE ISNULL(so.TotalOrdered, 0) < 50 OR ISNULL(s.Quantity, 0) > 100
);
GO

CREATE OR ALTER PROCEDURE ebs.AssignEmployeeBenefit
    @EmployeeID INT,
    @ProductID INT,
    @DiscountPercent DECIMAL(5,2)
AS
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM HumanResources.Employee WHERE BusinessEntityID = @EmployeeID)
        BEGIN
            THROW 50001, 'Employee does not exist.', 1;
        END

        IF NOT EXISTS (SELECT 1 FROM Production.Product WHERE ProductID = @ProductID)
        BEGIN
            THROW 50002, 'Product does not exist.', 1;
        END

        IF @DiscountPercent <= 0 OR @DiscountPercent > 100
        BEGIN
            THROW 50003, 'Discount percent must be between 0 and 100.', 1;
        END

        INSERT INTO ebs.EmployeeBenefit (EmployeeID, ProductID, DiscountPercent)
        VALUES (@EmployeeID, @ProductID, @DiscountPercent);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE ebs.RedeemBenefit
    @BenefitID INT,
    @EmployeeID INT
AS
BEGIN
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 
            FROM ebs.EmployeeBenefit 
            WHERE BenefitID = @BenefitID AND EmployeeID = @EmployeeID AND Redeemed = 0
        )
        BEGIN
            THROW 50004, 'Benefit not found or already redeemed.', 1;
        END

        UPDATE ebs.EmployeeBenefit
        SET Redeemed = 1,
            RedeemedDate = GETDATE()
        WHERE BenefitID = @BenefitID AND EmployeeID = @EmployeeID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO
