﻿namespace Core.Utilities;
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
   
    
}
public static class MessageError
{
    public const string DuplicateEmail = "Email already exists, please try using different email.";
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
}