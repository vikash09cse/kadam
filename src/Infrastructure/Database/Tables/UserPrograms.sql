Create Table UserPrograms
(
    Id Int Identity(1,1) Primary Key,
    UserId Int Not Null,
    ProgramId Int Not Null,
)