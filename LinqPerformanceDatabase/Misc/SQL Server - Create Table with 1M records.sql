-- drop database if it already exists
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'LinqPerformanceDemo')
BEGIN
    DROP DATABASE LinqPerformanceDemo;
END

-- create a new database instance
CREATE DATABASE LinqPerformanceDemo;

use [LinqPerformanceDemo]

-- drop table if it already exists
IF OBJECT_ID('dbo.LargeTable', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.LargeTable;
END


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

IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'NonClusteredIndex-SecurityType_Projectref' AND object_id = OBJECT_ID('dbo.LargeTable'))
BEGIN
    DROP INDEX [NonClusteredIndex-SecurityType_Projectref] ON [dbo].[LargeTable];
END

/****** Object:  Index [NonClusteredIndex-SecurityType_Projectref]    Script Date: 23/05/2024 12:04:05 ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-SecurityType_Projectref] ON [dbo].[LargeTable]
(
	[SecurityType] ASC,
	[ProjectRef] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


DECLARE @Counter INT = 0;

WHILE @Counter < 1000000
BEGIN
    INSERT INTO dbo.LargeTable ([CodeName], [ProjectRef], [ExternalRef], [StartDate], IsMale, SecurityType, Height, Address)
    VALUES (
        LEFT(NEWID(), 8),  -- Random string
        LEFT(NEWID(), 8),  -- Random string
        ABS(CHECKSUM(NEWID()) % 1000000),  -- Random integer
        --RAND() * 1000,  -- Random decimal
        DATEADD(DAY, ABS(CHECKSUM(NEWID()) % 365), GETDATE()),  -- Random date within the past year
        CAST(ABS(CHECKSUM(NEWID()) % 2) AS BIT),  -- Random bit
        CHAR(65 + ABS(CHECKSUM(NEWID()) % 26)),  -- Random char (A-Z)
        RAND() * 1000,  -- Random float
        LEFT(NEWID(), 8)  -- Random string
        --ABS(CHECKSUM(NEWID()) % 256)  -- Random tinyint
    );

    SET @Counter = @Counter + 1;
END;
