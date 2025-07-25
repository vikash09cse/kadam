Create or Alter Procedure dbo.usp_DashboardCount
@CreatedBy INT = 0
AS
	BEGIN
		 Declare @ActiveCount INT;
		 Declare @CompletedCount INT;
		 Declare @InactiveCount INT;

		 SET @ActiveCount = (Select Count(1) From Students Where (@CreatedBy = 0 OR CreatedBy = @CreatedBy) AND IsDeleted = 0 AND CurrentStatus=1)
		 SET @CompletedCount = (Select Count(1) From Students Where (@CreatedBy = 0 OR CreatedBy = @CreatedBy) AND IsDeleted = 0 AND CurrentStatus=3)
		 SET @InactiveCount = (Select Count(1) From Students Where (@CreatedBy = 0 OR CreatedBy = @CreatedBy) AND IsDeleted = 0 AND CurrentStatus=2)

		 SELECT @ActiveCount AS ActiveCount, @CompletedCount AS CompletedCount, @InactiveCount AS InactiveCount
	END
