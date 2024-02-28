using TodoListAPI.Entities;

namespace TodoListAPI.Contracts
{
    public class TodosPageResponse
    {
        public List<Todo> Todos { get; set; } = new();
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
