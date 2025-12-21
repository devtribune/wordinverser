CREATE PROCEDURE [dbo].[usp_GetAllRequestResponses]
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
    ORDER BY [CreatedDate] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
