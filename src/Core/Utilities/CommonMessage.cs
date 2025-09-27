namespace Core.Utilities;
public static class MessageSuccess
{
    public const string Saved = "Record has been saved successfully.";
    public const string Updated = "Record has been updated successfully.";
    public const string Deleted = "Record has been deleted successfully.";
    public const string EmailSent = "E-mail has been sent successfully.";
    public const string PasswordRecovery = "Password has been sent on your registered email address.";
    public const string LoggedIn = "Logged in successfully.";
    public const string PasswordChanged = "Password has been updated successfully.";
    public const string ConfirmText = "Are you sure you want to delete this record?";
    public const string ImportSuccess = "Record has been imported successfully.";
    public const string FileUploaded = "File has been uploaded successfully.";
    public const string NoRecordFound = "No record found.";
    public const string RecordFound = "Record found.";
    public const string Closed = "Record has been closed successfully.";
    public const string Found = "Record has been found successfully.";


}
public static class MessageError
{
    public const string DuplicateEmail = "Email already exists, please try using different email.";
    public const string DuplicateDivisionName = "Division name already exists, please try using different name.";
    public const string DuplicateDivisionCode = "Division code already exists, please try using different code.";
    public const string DuplicateStateName = "State name already exists, please try using different name.";
    public const string DuplicateStateCode = "State code already exists, please try using different code.";
    public const string DuplicateDistrictName = "District name already exists, please try using different name.";
    public const string DuplicateDistrictCode = "District code already exists, please try using different code.";
    public const string DuplicateBlockName = "Block name already exists, please try using different name.";
    public const string DuplicateVillageName = "Village name already exists, please try using different name.";
    public const string DuplicateInstitutionName = "Institution name already exists, please try using different name.";
    public const string DuplicateInstitutionCode = "Institution code already exists, please try using different code.";
    public const string DuplicateProgramName = "Program name already exists, please try using different name.";
    public const string DuplicateRoleName = "Role name already exists, please try using different name.";
    public const string DuplicateMenuName = "Menu name already exists, please try using different name.";
    public const string DuplicateSubjectName = "Subject name already exists, please try using different name.";
    public const string DuplicateStepName = "Step name already exists, please try using different name.";
    public const string DuplicateThemeName = "Theme name already exists, please try using different name.";
    public const string DuplicateStudentRegistrationNumber = "Registration number already exists, please try using different number.";
    public const string DuplicateStudent = "Student registration already exists.";
    public const string DuplicateAadhaarNumber = "Aadhaar number already exists, please try using different number.";

    public const string NoDataFound = "Sorry! no data found, please try again.";
    public const string CodeIssue = "Server error: Contact administrator.";
    public const string EmailNotFound = "Email does not exists in our system, please try using different email.";
    public const string OldPasswordNotCorrect = "Please enter correct old password.";
    public const string LoginError = "Incorrect email or password, please try using correct email and password.";
    public const string DuplicateRecord = "Duplicate record found : ";
    public const string DuplicateEmailTemplateTitle = "Template title already exists. Please try using different template title.";
    public const string EmailNotSent = "But Some problem occured while sending email : ";
    public const string TokenExpired = "Token has been expired.";
    public const string VerificationFormInvalid = "Invalid form.";
    public const string InvalidCredential = "The email id or password is incorrect. Please try again.";
    public static string ErrorSavingUser(string errorMessage) => $"Error saving user: {errorMessage}";
    public static string InvalidData => "Invalid data";
    public static string InvalidStudentId => "Invalid student ID";
    public static string StudentIdRequired => "Student ID is required";
    public static string SubjectIdRequired => "Subject ID is required";

    public const string FailedToSaveProfilePicture = "Failed to save profile picture";
}