namespace InternHub.Application.DTO.Pagination
{
    public class BaseParametersDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy {get; set; } = "Name";
        public string? SortDirection { get; set; } = "asc";
    }
}