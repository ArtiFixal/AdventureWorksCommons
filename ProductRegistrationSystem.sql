USE AdventureWorks;
GO

IF OBJECT_ID('prs.trg_InsertProductValidation', 'TR') IS NOT NULL
    DROP TRIGGER prs.trg_InsertProductValidation;
GO

IF OBJECT_ID('prs.NewProductStaging', 'U') IS NOT NULL
    DROP TABLE prs.NewProductStaging;
GO

IF OBJECT_ID('prs.DataValidation', 'P') IS NOT NULL
    DROP PROCEDURE prs.DataValidation;
GO

IF OBJECT_ID('prs.ValidateText', 'P') IS NOT NULL
    DROP PROCEDURE prs.ValidateText;
GO

IF OBJECT_ID('prs.ValidatePositiveNumber', 'P') IS NOT NULL
    DROP PROCEDURE prs.ValidatePositiveNumber;
GO

IF EXISTS (SELECT * FROM sys.types WHERE is_user_defined = 1 AND name = 'ProductNameType')
    DROP TYPE prs.ProductNameType;
GO

IF EXISTS (SELECT * FROM sys.types WHERE is_user_defined = 1 AND name = 'ProductNumberType')
    DROP TYPE prs.ProductNumberType;
GO

IF EXISTS (SELECT * FROM sys.types WHERE is_user_defined = 1 AND name = 'ProductPriceType')
    DROP TYPE prs.ProductPriceType;
GO

IF EXISTS (SELECT * FROM sys.types WHERE is_user_defined = 1 AND name = 'ProductStockPoint')
    DROP TYPE prs.ProductStockPoint;
GO

IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'prs')
    DROP SCHEMA prs;
GO

CREATE SCHEMA prs;
GO

CREATE TYPE prs.ProductNameType FROM NVARCHAR(60) NOT NULL;
CREATE TYPE prs.ProductNumberType FROM NVARCHAR(25) NOT NULL;
CREATE TYPE prs.ProductPriceType FROM MONEY NOT NULL;
CREATE TYPE prs.ProductStockPoint FROM SMALLINT NOT NULL;
GO

CREATE TABLE prs.NewProductStaging (
    Name                     prs.ProductNameType,
    ProductNumber            prs.ProductNumberType,
    MakeFlag                 BIT NOT NULL DEFAULT 0,
    FinishedGoodsFlag        BIT NOT NULL DEFAULT 0,
    Color                    NVARCHAR(15) NULL,
    SafetyStockLevel         prs.ProductStockPoint,
    ReorderPoint             prs.ProductStockPoint,
    StandardCost             prs.ProductPriceType,
    ListPrice                prs.ProductPriceType,
    Size                     NVARCHAR(5) NULL,
    SizeUnitMeasureCode      NCHAR(3) NULL,
    WeightUnitMeasureCode    NCHAR(3) NULL,
    Weight                   DECIMAL(8,2) NULL,
    DaysToManufacture        INT NOT NULL,
    ProductLine              NCHAR(2) NULL,
    Class                    NCHAR(2) NULL,
    Style                    NCHAR(2) NULL,
    ProductSubcategoryID     INT NULL,
    ProductModelID           INT NULL,
    SellStartDate            DATETIME NOT NULL,
    SellEndDate              DATETIME NULL,
    DiscontinuedDate         DATETIME NULL
);
GO

CREATE PROCEDURE prs.ValidateText
    @Text NVARCHAR(255),
    @FieldName NVARCHAR(30)
AS
BEGIN
    IF @Text IS NULL OR @Text = ''
    BEGIN
        DECLARE @ErrorMsg NVARCHAR(255) = @FieldName + ' cannot be empty';
        THROW 50001, @ErrorMsg, 1;
    END
END
GO

CREATE PROCEDURE prs.ValidatePositiveNumber
    @Number INT,
    @FieldName NVARCHAR(30)
AS
BEGIN
    DECLARE @ErrorMsg NVARCHAR(127) = @FieldName + ' cannot be ';
    IF @Number IS NULL
    BEGIN
        SET @ErrorMsg += 'null';
        THROW 50004, @ErrorMsg, 1;
    END
    IF @Number < 0
    BEGIN
        SET @ErrorMsg += 'negative';
        THROW 50004, @ErrorMsg, 1;
    END
END
GO

CREATE PROCEDURE prs.DataValidation
    @Name prs.ProductNameType,
    @ProductNumber prs.ProductNumberType,
    @StandardCost prs.ProductPriceType,
    @ListPrice prs.ProductPriceType,
    @SafetyStockLevel prs.ProductStockPoint,
    @ReorderPoint prs.ProductStockPoint,
    @DaysToManufacture INT,
    @SellStartDate DATETIME
AS
BEGIN
    EXEC prs.ValidateText @Text = @Name, @FieldName = 'Name';
    EXEC prs.ValidateText @Text = @ProductNumber, @FieldName = 'ProductNumber';
    EXEC prs.ValidatePositiveNumber @Number = @DaysToManufacture, @FieldName = 'DaysToManufacture';

    IF @StandardCost <= 0
        THROW 50002, 'StandardCost must be greater than 0', 1;

    IF @ListPrice < @StandardCost
        THROW 50003, 'ListPrice cannot be lower than StandardCost', 1;

    IF @SafetyStockLevel < 0
        THROW 50004, 'SafetyStockLevel cannot be negative', 1;

    IF @ReorderPoint < 0
        THROW 50005, 'ReorderPoint cannot be negative', 1;

    IF @SellStartDate IS NULL
        THROW 50006, 'SellStartDate is required', 1;
END
GO

CREATE TRIGGER prs.trg_InsertProductValidation
ON prs.NewProductStaging
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Name                    prs.ProductNameType,
        @ProductNumber           prs.ProductNumberType,
        @MakeFlag                BIT,
        @FinishedGoodsFlag       BIT,
        @Color                   NVARCHAR(15),
        @SafetyStockLevel        prs.ProductStockPoint,
        @ReorderPoint            prs.ProductStockPoint,
        @StandardCost            prs.ProductPriceType,
        @ListPrice               prs.ProductPriceType,
        @Size                    NVARCHAR(5),
        @SizeUnitMeasureCode     NCHAR(3),
        @WeightUnitMeasureCode   NCHAR(3),
        @Weight                  DECIMAL(8,2),
        @DaysToManufacture       INT,
        @ProductLine             NCHAR(2),
        @Class                   NCHAR(2),
        @Style                   NCHAR(2),
        @ProductSubcategoryID    INT,
        @ProductModelID          INT,
        @SellStartDate           DATETIME,
        @SellEndDate             DATETIME,
        @DiscontinuedDate        DATETIME;

    SELECT 
        @Name = Name,
        @ProductNumber = ProductNumber,
        @MakeFlag = MakeFlag,
        @FinishedGoodsFlag = FinishedGoodsFlag,
        @Color = Color,
        @SafetyStockLevel = SafetyStockLevel,
        @ReorderPoint = ReorderPoint,
        @StandardCost = StandardCost,
        @ListPrice = ListPrice,
        @Size = Size,
        @SizeUnitMeasureCode = SizeUnitMeasureCode,
        @WeightUnitMeasureCode = WeightUnitMeasureCode,
        @Weight = Weight,
        @DaysToManufacture = DaysToManufacture,
        @ProductLine = ProductLine,
        @Class = Class,
        @Style = Style,
        @ProductSubcategoryID = ProductSubcategoryID,
        @ProductModelID = ProductModelID,
        @SellStartDate = SellStartDate,
        @SellEndDate = SellEndDate,
        @DiscontinuedDate = DiscontinuedDate
    FROM inserted;

    EXEC prs.DataValidation 
        @Name = @Name,
        @ProductNumber = @ProductNumber,
        @StandardCost = @StandardCost,
        @ListPrice = @ListPrice,
        @SafetyStockLevel = @SafetyStockLevel,
        @ReorderPoint = @ReorderPoint,
        @DaysToManufacture = @DaysToManufacture,
        @SellStartDate = @SellStartDate;

    INSERT INTO Production.Product (
        Name,
        ProductNumber,
        MakeFlag,
        FinishedGoodsFlag,
        Color,
        SafetyStockLevel,
        ReorderPoint,
        StandardCost,
        ListPrice,
        Size,
        SizeUnitMeasureCode,
        WeightUnitMeasureCode,
        Weight,
        DaysToManufacture,
        ProductLine,
        Class,
        Style,
        ProductSubcategoryID,
        ProductModelID,
        SellStartDate,
        SellEndDate,
        DiscontinuedDate,
        rowguid,
        ModifiedDate
    )
    VALUES (
        @Name,
        @ProductNumber,
        @MakeFlag,
        @FinishedGoodsFlag,
        @Color,
        @SafetyStockLevel,
        @ReorderPoint,
        @StandardCost,
        @ListPrice,
        @Size,
        @SizeUnitMeasureCode,
        @WeightUnitMeasureCode,
        @Weight,
        @DaysToManufacture,
        @ProductLine,
        @Class,
        @Style,
        @ProductSubcategoryID,
        @ProductModelID,
        @SellStartDate,
        @SellEndDate,
        @DiscontinuedDate,
        NEWID(),
        GETDATE()
    );
END
GO
