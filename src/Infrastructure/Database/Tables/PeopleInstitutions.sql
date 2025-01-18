Create Table PeopleInstitutions
(
    Id Int Identity(1,1) Primary Key,
    UserId Int Not Null,
    DivisionId Int Not Null,
    StateId Int Not Null,
    DistrictId Int Not Null,
    BlockId Int Not Null,
    VillageId Int Not Null,
    InstitutionTypeId Int Not Null,
    InstitutionIds VarChar(2000) Null
)