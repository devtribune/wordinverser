CREATE PROCEDURE [dbo].[usp_GetWordCacheBatch]
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Word],
        [InversedWord],
        [CreatedDate],
        [UpdatedDate]
    FROM [dbo].[WordCache]
    ORDER BY [Id]
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
