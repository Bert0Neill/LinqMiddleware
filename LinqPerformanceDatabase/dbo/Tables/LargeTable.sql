CREATE TABLE [dbo].[LargeTable] (
    [ID]           INT            IDENTITY (1, 1) NOT NULL,
    [CodeName]     VARCHAR (50)   NULL,
    [ProjectRef]   VARCHAR (50)   NULL,
    [ExternalRef]  INT            NULL,
    [StartDate]    DATETIME       NULL,
    [IsMale]       BIT            NULL,
    [SecurityType] CHAR (1)       NULL,
    [Height]       FLOAT (53)     NULL,
    [Address]      NVARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

