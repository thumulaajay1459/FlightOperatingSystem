namespace Common
{
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationMetadata Pagination { get; set; } = new();
    }

    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    public static class PaginationHelper
    {
        public static (int page, int pageSize) ValidateParams(int page, int pageSize, int maxPageSize = 100)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > maxPageSize) pageSize = 10;
            return (page, pageSize);
        }

        public static PaginatedResult<T> CreateResult<T>(List<T> data, int page, int pageSize, int totalCount)
        {
            return new PaginatedResult<T>
            {
                Data = data,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };
        }
    }
}


  

  