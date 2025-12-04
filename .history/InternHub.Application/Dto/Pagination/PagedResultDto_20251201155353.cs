using System.Collections.Generic;
namespace InternHub.Application.DTO.Pagination
{
    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); 
        public IList<T> Items { get; set; }
    }
}