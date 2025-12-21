CREATE PROCEDURE [dbo].[usp_UpsertWordCache]
    @Word NVARCHAR(500),
    @InversedWord NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [dbo].[WordCache] WHERE [Word] = @Word)
    BEGIN
        UPDATE [dbo].[WordCache]
        SET [InversedWord] = @InversedWord,
            [UpdatedDate] = GETUTCDATE()
        WHERE [Word] = @Word;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[WordCache] ([Word], [InversedWord])
        VALUES (@Word, @InversedWord);
    END
END
