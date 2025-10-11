namespace ESGSustainabilityAPI.ViewModels
{
    /// <summary>
    /// ViewModel para paginação de resultados
    /// </summary>
    /// <typeparam name="T">Tipo dos dados paginados</typeparam>
    public class PagedResultViewModel<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    /// <summary>
    /// ViewModel para parâmetros de paginação
    /// </summary>
    public class PaginationParametersViewModel
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? SearchTerm { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }

    /// <summary>
    /// ViewModel para resposta padrão da API
    /// </summary>
    /// <typeparam name="T">Tipo dos dados de resposta</typeparam>
    public class ApiResponseViewModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// ViewModel para resposta de erro da API
    /// </summary>
    public class ErrorResponseViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
    }
}

