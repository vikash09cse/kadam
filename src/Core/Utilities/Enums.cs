using System.ComponentModel;

namespace Core.Utilities
{
    public class Enums
    {
        public enum Gender
        {
            [Description("Male")]
            Male = 1,
            [Description("Female")]
            Female = 2,
            [Description("Other")]
            Other = 3

        }
        public enum Status
        {
            [Description("Active")]
            Active = 1,
            [Description("Inactive")]
            Inactive = 2,
            [Description("Closed")]
            Closed = 3
        }
        public enum InstitutionType
        {
            [Description("Primary School")]
            PrimarySchool = 1,
            [Description("Middle School")]
            MiddleSchool = 2,
            [Description("High School")]
            HighSchool = 3,
            [Description("Pre School")]
            PreSchool = 4,
            [Description("DIET")]
            DIET = 5
        }
        public enum InstitutionBuilding
        {
            [Description("Public")]
            Public = 1,
            [Description("Private")]
            Private = 2,
            [Description("Govt. School")]
            GovtSchool = 3
        }

        public enum ChildStatusBeforKadamType
        {
            [Description("Never Enrolled in School")]
            NeverEnrolledInSchool = 1,
            [Description("Enrolled but not Attending")]
            EnrolledButNotAttending = 2,
            [Description("Dropped-Out")]
            DroppedOut = 3
        }
        public enum HowLongStayInThisAreaType
        {
            [Description("Less than 3 Months")]
            LessThan3Months = 1,
            [Description("3-6 months")]
            ThreeToSixMonths = 2,
            [Description("6-12 months")]
            SixToTwelveMonths = 3,
            [Description("1-3 years")]
            OneToThreeYears = 4,
            [Description("More than 4 years")]
            MoreThanFourYears = 5
        }
        public enum StudentReasonType
        {
            [Description("Migration of the family")]
            MigrationOfTheFamily = 1,
            [Description("Financial Constraints")]
            FinancialConstraints = 2,
            [Description("Obligation to do household chores")]
            ObligationToDoHouseholdChores = 3,
            [Description("Child not comfortable in school environment")]
            ChildNotComfortableInSchoolEnvironment = 4,
            [Description("Health related issues")]
            HealthRelatedIssues = 5,
            [Description("Others")]
            Others = 6
        }
        public enum OccupationType
        {
            [Description("Self-employed")]
            SelfEmployed = 1,
            [Description("Daily Wages Labourer")]
            DailyWagesLabourer = 2,
            [Description("Govt. Service")]
            GovtService = 3,
            [Description("Non Govt. Service")]
            NonGovtService = 4,
            [Description("Business")]
            Business = 5,
            [Description("Unemployed")]
            Unemployed = 6,
            [Description("Other")]
            Other = 7
        }
        public enum EducationType
        {
            [Description("Below 7th")]
            Below7th = 1,
            [Description("7th")]
            Seventh = 2,
            [Description("8th")]
            Eighth = 3,
            [Description("9th")]
            Ninth = 4,
            [Description("10th")]
            Tenth = 5,
            [Description("HSC")]
            HSC = 6,
            [Description("Graduation")]
            Graduation = 7,
            [Description("No Formal Education")]
            NoFormalEducation = 8
        }
        public enum PeopleLivingCountType
        {
            [Description("1")]
            One = 1,
            [Description("2")]
            Two = 2,
            [Description("3")]
            Three = 3,
            [Description("4")]
            Four = 4,
            [Description("5")]
            Five = 5,
            [Description("6")]
            Six = 6,
            [Description("7")]
            Seven = 7,
            [Description("8")]
            Eight = 8,
            [Description("9")]
            Nine = 9,
            [Description("10")]
            Ten = 10,
            [Description("11")]
            Eleven = 11,
            [Description("12")]
            Twelve = 12,
            [Description("13")]
            Thirteen = 13,
            [Description("14")]
            Fourteen = 14,
            [Description("15")]
            Fifteen = 15,
            [Description(">15")]
            Sixteen = 16
        }
        public enum CasteType
        {
            [Description("SC")]
            SC = 1,
            [Description("ST")]
            ST = 2,
            [Description("NT")]
            NT = 3,
            [Description("OBC")]
            OBC = 4,
            [Description("General")]
            General = 5
        }
        public enum ReligionType
        {
            [Description("Hindu")]
            Hindu = 1,
            [Description("Muslim")]
            Muslim = 2,
            [Description("Christian")]
            Christian = 3,
            [Description("Buddhism")]
            Buddhism = 4,
            [Description("Jain")]
            Jain = 5,
            [Description("Sikh")]
            Sikh = 6,
            [Description("Other Religion")]
            OtherReligion = 7
        }
        public enum MonthlyIncomeType
        {
            [Description("< 5000")]
            LessThan5000 = 1,
            [Description("5001-10000")]
            From5001To10000 = 2,
            [Description("10001-20000")]
            From10001To20000 = 3,
            [Description("> 20000")]
            MoreThan20000 = 4
        }
        public enum PhysicallyChallengedType
        {
            [Description("Blindness")]
            Blindness = 1,
            [Description("Deafness or Hearing Loss")]
            DeafnessOrHearingLoss = 2,
            [Description("Impairment")]
            Impairment = 3,
            [Description("Learning Disability")]
            LearningDisability = 4,
            [Description("Leprocy- Cured")]
            LeprocyCured = 5,
            [Description("Loco-Motor Disability")]
            LocoMotorDisability = 6,
            [Description("Low Vision")]
            LowVision = 7,
            [Description("Memory Loss")]
            MemoryLoss = 8,
            [Description("Mental Illness")]
            MentalIllness = 9,
            [Description("Mental Retardation")]
            MentalRetardation = 10,
            [Description("Physical Disability")]
            PhysicalDisability = 11,
            [Description("Speech & Language Disorders")]
            SpeechAndLanguageDisorders = 12
        }
        public enum DocumentType
        {
            [Description("Aadhar Card")]
            AadharCard = 1,
            [Description("Parent Aadhar Card")]
            ParentAadharCard = 2,
            [Description("Birth Certificate")]
            BirthCertificate = 3,
            [Description("Caste Certificate")]
            CasteCertificate = 4,
            [Description("RG Health Card")]
            RGHealthCard = 5,
            [Description("Others")]
            Others = 6
        }
        public enum PhysicalChallengedTypes
        {
            [Description("Blindness")]
            Blindness = 1,
            [Description("Deafness or Hearing Loss")]
            DeafnessOrHearingLoss = 2,
            [Description("Impairment")]
            Impairment = 3,
            [Description("Learning Disability")]
            LearningDisability = 4,
            [Description("Leprocy-Cured")]
            LeprocyCured = 5,
            [Description("Loco-motor Disability")]
            LocoMotorDisability = 6,
            [Description("Low Vision")]
            LowVision = 7,
            [Description("Memory Loss")]
            MemoryLoss = 8,
            [Description("Mental Illness")]
            MentalIllness = 9,
            [Description("Mental Retardation")]
            MentalRetardation = 10,
            [Description("Physical Disability")]
            PhysicalDisability = 11,
            [Description("Speech & Language Disorders")]
            SpeechAndLanguageDisorders = 12
        }

    }
}
