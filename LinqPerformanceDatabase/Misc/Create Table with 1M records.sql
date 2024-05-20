USE [LinqPerformanceDemo];

IF OBJECT_ID('dbo.LargeTable', 'U') IS NOT NULL
    DROP TABLE [dbo].[LargeTable];
GO

CREATE TABLE [dbo].[LargeTable](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CodeName] [varchar](50) NULL,
	[ProjectRef] [varchar](50) NULL,
	[ExternalRef] [int] NULL,
	[StartDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[SecurityType] [char](1) NULL,
	[Height] [float] NULL,
	[Address] [nvarchar](100) NULL
);
GO

DECLARE @Counter INT = 0;

WHILE @Counter < 1000000
BEGIN
    INSERT INTO dbo.LargeTable ([CodeName], [ProjectRef], [ExternalRef], [StartDate], [IsActive], SecurityType, Height, Address)
    VALUES (
        LEFT(NEWID(), 8),  -- Random string
        LEFT(NEWID(), 8),  -- Random string
        ABS(CHECKSUM(NEWID()) % 1000000),  -- Random integer
        DATEADD(DAY, ABS(CHECKSUM(NEWID()) % 365), GETDATE()),  -- Random date within the past year
        CAST(ABS(CHECKSUM(NEWID()) % 2) AS BIT),  -- Random bit
        CHAR(65 + ABS(CHECKSUM(NEWID()) % 26)),  -- Random char (A-Z)
        RAND() * 1000,  -- Random float
        LEFT(NEWID(), 8)  -- Random string
    );

    SET @Counter = @Counter + 1;
END;