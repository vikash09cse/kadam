namespace Core.DTOs
{
    public class DivisionListDTO
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string DivisionCode { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
