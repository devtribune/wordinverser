CREATE TABLE [dbo].[RequestResponse]
(
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RequestId] UNIQUEIDENTIFIER NOT NULL UNIQUE DEFAULT NEWID(),
    [Request] NVARCHAR(MAX) NOT NULL,
    [Response] NVARCHAR(MAX) NOT NULL,
    [Tags] NVARCHAR(MAX) NOT NULL, -- JSON array of words for faster search
    [Exception] NVARCHAR(MAX) NULL,
    [IsSuccess] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [ProcessingTimeMs] BIGINT NULL
);
GO

CREATE INDEX [IX_RequestResponse_CreatedDate] ON [dbo].[RequestResponse] ([CreatedDate] DESC);
GO

CREATE INDEX [IX_RequestResponse_IsSuccess] ON [dbo].[RequestResponse] ([IsSuccess]);
GO
