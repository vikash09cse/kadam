CREATE TABLE dbo.Divisions
(
    Id					Int Not Null Identity(1,1),
    DivisionName		Varchar(55) Not Null,
	DivisionCode		Varchar(25) Not Null,
	StateId				Int Null,
	DivisionStatus		TinyInt Not Null Default(0),
	CloseDate			DateTime Null,
	IsDeleted			bit NOT NULL,
	DateCreated			datetime NULL,
	CreatedBy			int NULL,
	ModifyDate			Datetime NULL,
	ModifyBy			int NULL,
	DeletedDate			Datetime NULL,
	DeletedBy			int NULL
);