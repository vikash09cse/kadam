namespace Core.Utilities;

public static class DBConstant
{
    public static class SP
    {
        public const string usp_Users = "usp_Users";
        public const string usp_UpsertUser = "usp_UpsertUser";
        public const string usp_UserLoginValidate = "usp_UserLoginValidate";

    }
    public static class Param
    {
        public const string QueryType = "QueryType";
        public const string Id = "Id";
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string Email = "Email";
        public const string Password = "Password";
        public const string UserName = "UserName";
        public const string PasswordHash = "PasswordHash";
        public const string PasswordSalt = "PasswordSalt";
        public const string Phone = "Phone";
        public const string AlternatePhone = "AlternatePhone";
        public const string Gender = "Gender";
        public const string Grade = "Grade";
        public const string Section = "Section";
        public const string GradeSection = "GradeSection";
        public const string DivisionId = "DivisionId";
        public const string RoleId = "RoleId";
        public const string ReporteeRoleId = "ReporteeRoleId";
        public const string UserStatus = "UserStatus";
        public const string ActivityType = "ActivityType";
        public const string IsDeleted = "IsDeleted";
        public const string DateCreated = "DateCreated";
        public const string CreatedBy = "CreatedBy";
        public const string ModifyDate = "ModifyDate";
        public const string ModifyBy = "ModifyBy";
    }
    public static class BaselineType
    {
        public const string BaselinePreAssessment = "baselinepreAssessment";
        public const string EndlinePreAssessment = "endlinepreAssessment";
    }
}


