namespace Interchée.Entities
{
    public class AbsenceConfiguration
    {
        public int MaxAbsenceDaysPerMonth { get; set; } = 5;
        public int MaxAbsenceDaysPerInternship { get; set; } = 20;
        public bool RequireDocumentationForExtendedLeave { get; set; } = true;
        public int DocumentationRequiredAfterDays { get; set; } = 3;
    }
}
