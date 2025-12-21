CREATE PROCEDURE [dbo].[usp_SearchRequestResponseByWord]
    @SearchWord NVARCHAR(500),
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [RequestId],
        [Request],
        [Response],
        [Tags],
        [Exception],
        [IsSuccess],
        [CreatedDate],
        [ProcessingTimeMs],
        COUNT(*) OVER() as TotalCount
    FROM [dbo].[RequestResponse]
    WHERE [Tags] LIKE '%' + @SearchWord + '%'
    ORDER BY [CreatedDate] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
