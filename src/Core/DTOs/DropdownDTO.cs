namespace Core.DTOs
{
    public class DropdownDTO
    {
        public int Value { get; set; }
        public string Text { get; set; } = string.Empty;
        /// <summary>Used by roles dropdown; ignored for other dropdown sources.</summary>
        public bool AllowMultipleDivision { get; set; }
    }
}
