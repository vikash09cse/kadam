using System.Text.RegularExpressions;

namespace Core.Features.StudentsWeb;

public sealed partial class StudentsWebValidator
{
    [GeneratedRegex(@"^[\p{L}\s]+$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^\d{10}$")]
    private static partial Regex MobilePattern();

    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex PinPattern();

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex DocumentNumberPattern();

    public IReadOnlyList<string> ValidateStudent(StudentsWebEditDTO model)
    {
        var errors = new List<string>();
        var today = DateTime.Today;

        if (model.EnrollmentDate.Date > today)
            errors.Add("Enrollment date cannot be in the future.");
        if (model.EnrollmentDate.Date < today.AddYears(-2))
            errors.Add("Enrollment date cannot be more than two years in the past.");

        ValidateName(model.FirstName, "First name", errors);
        ValidateName(model.LastName, "Last name", errors);

        if (model.GenderId <= 0) errors.Add("Gender is required.");
        if (model.DateOfBirth.Date > today) errors.Add("Date of birth cannot be in the future.");
        if (model.InstitutionId <= 0) errors.Add("Institution is required.");
        if (model.GradeId <= 0) errors.Add("Grade is required.");
        if (string.IsNullOrWhiteSpace(model.Section)) errors.Add("Section is required.");

        var age = CalculateAge(model.DateOfBirth, model.EnrollmentDate);
        if (model.IsKadamPlusStudent)
        {
            if (age.Years < 5 || (age.Years == 5 && age.Months < 5) ||
                age.Years > 14 || (age.Years == 14 && age.Months >= 5))
                errors.Add("Kadam Plus students must be between 5 years 5 months and 14 years 4 months old at enrollment.");
        }
        else if (age.Years < 6 || (age.Years == 6 && age.Months < 5) ||
                 age.Years > 14 || (age.Years == 14 && age.Months >= 5))
        {
            errors.Add("Kadam students must be between 6 years 5 months and 14 years 4 months old at enrollment.");
        }

        if (model.DoYouHaveAadhaarCard &&
            (string.IsNullOrWhiteSpace(model.AadhaarCardNumber) ||
             model.AadhaarCardNumber.Length != 12 || !model.AadhaarCardNumber.All(char.IsDigit)))
            errors.Add("Aadhaar number must contain exactly 12 digits.");

        if (model.IsKadamPlusStudent &&
            (string.IsNullOrWhiteSpace(model.StudentRegistratioNumber) ||
             model.StudentRegistratioNumber.Trim().Length > 50))
            errors.Add("Student registration number is required and cannot exceed 50 characters.");

        if (!model.IsKadamPlusStudent)
        {
            if (model.ChildStatudBeforeKadamSTC <= 0)
                errors.Add("Child status before Kadam STC is required.");
            if (model.HowLongPlaningToStayThisArea <= 0)
                errors.Add("Expected stay duration is required.");
            if (model.ChildStatudBeforeKadamSTC == 2 && model.Class is < 1 or > 8)
                errors.Add("Last class must be between 1 and 8.");
            if (model.ChildStatudBeforeKadamSTC == 3)
            {
                if (model.ReasonId <= 0) errors.Add("Dropout reason is required.");
                if (string.IsNullOrWhiteSpace(model.DropoutClass)) errors.Add("Dropout class is required.");
                if (!model.DropoutYear.HasValue ||
                    model.DropoutYear < model.DateOfBirth.Year ||
                    model.DropoutYear > model.EnrollmentDate.Year)
                    errors.Add("Dropout year must be between the birth year and enrollment year.");
            }
        }

        ValidateFamily(model, errors);
        return errors;
    }

    public IReadOnlyList<string> ValidateHealth(StudentsWebHealthDTO model)
    {
        var errors = new List<string>();
        if (model.StudentId <= 0) errors.Add("Student is required.");
        if (model.PhysicallyChallenged)
        {
            if (!model.PhysicallyChallengedType.HasValue || model.PhysicallyChallengedType <= 0)
                errors.Add("Physical challenge type is required.");
            if (!model.PercentagePhysicallyChallenged.HasValue ||
                model.PercentagePhysicallyChallenged <= 0 ||
                model.PercentagePhysicallyChallenged > 100)
                errors.Add("Physical challenge percentage must be between 1 and 100.");
            if (string.IsNullOrWhiteSpace(model.DisabilityCertificatePath))
                errors.Add("Disability certificate is required.");
        }
        return errors;
    }

    public IReadOnlyList<string> ValidateDocument(StudentsWebDocumentDTO model)
    {
        var errors = new List<string>();
        if (model.StudentId <= 0) errors.Add("Student is required.");
        if (model.DocumentTypeId <= 0) errors.Add("Document type is required.");
        if (string.IsNullOrWhiteSpace(model.DocumentNumber) ||
            model.DocumentNumber.Length is < 5 or > 25 ||
            !DocumentNumberPattern().IsMatch(model.DocumentNumber))
            errors.Add("Document number must contain 5 to 25 alphanumeric characters.");
        if (string.IsNullOrWhiteSpace(model.DocumentPath))
            errors.Add("A document file is required.");
        return errors;
    }

    public static int EffectiveAge(DateTime dateOfBirth, DateTime enrollmentDate)
    {
        var age = CalculateAge(dateOfBirth, enrollmentDate);
        return age.Months >= 5 ? age.Years + 1 : age.Years;
    }

    private static (int Years, int Months) CalculateAge(DateTime birth, DateTime at)
    {
        var years = at.Year - birth.Year;
        var months = at.Month - birth.Month;
        if (at.Day < birth.Day) months--;
        if (months < 0)
        {
            years--;
            months += 12;
        }
        return (years, months);
    }

    private static void ValidateName(string value, string field, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            errors.Add($"{field} is required.");
        else if (value.Length > 24 || !NamePattern().IsMatch(value))
            errors.Add($"{field} may contain letters and spaces only, up to 24 characters.");
    }

    private static void ValidateFamily(StudentsWebEditDTO model, ICollection<string> errors)
    {
        ValidateName(model.FatherName, "Father's name", errors);
        ValidateName(model.MotherName, "Mother's name", errors);
        if (!IsValidParentAge(model.FatherAge))
            errors.Add("Father's age must be 0 if unknown, or more than 20 and less than 99.");
        if (!IsValidParentAge(model.MotherAge))
            errors.Add("Mother's age must be 0 if unknown, or more than 20 and less than 99.");
        if (string.IsNullOrWhiteSpace(model.PrimaryContactNumber) || !MobilePattern().IsMatch(model.PrimaryContactNumber))
            errors.Add("Primary contact number must contain exactly 10 digits.");
        if (!string.IsNullOrWhiteSpace(model.AlternateContactNumber) &&
            !MobilePattern().IsMatch(model.AlternateContactNumber))
            errors.Add("Alternate contact number must contain exactly 10 digits.");
        if (string.IsNullOrWhiteSpace(model.HouseAddress) || model.HouseAddress.Length is < 10 or > 200)
            errors.Add("House address must be between 10 and 200 characters.");
        if (string.IsNullOrWhiteSpace(model.PinCode) || !PinPattern().IsMatch(model.PinCode))
            errors.Add("PIN code must contain exactly 6 digits.");
        if (model.PeopleInHouseId is null or < 2) errors.Add("People in house must be selected.");
        if (model.FatherOccupationId is null or <= 0) errors.Add("Father's occupation is required.");
        if (model.FatherEducationId is null or <= 0) errors.Add("Father's education is required.");
        if (model.MotherOccupationId is null or <= 0) errors.Add("Mother's occupation is required.");
        if (model.MotherEducationId is null or <= 0) errors.Add("Mother's education is required.");
        if (model.CasteId is null or <= 0) errors.Add("Caste is required.");
        if (model.ReligionId is null or <= 0) errors.Add("Religion is required.");
        if (string.IsNullOrWhiteSpace(model.ParentMonthlyIncome)) errors.Add("Monthly income is required.");
        if (string.IsNullOrWhiteSpace(model.ParentMontlyExpenditure)) errors.Add("Monthly expenditure is required.");
    }

    private static bool IsValidParentAge(int? age) =>
        age is 0 or (> 20 and < 99);
}
