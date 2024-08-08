namespace AzureRest.Contracts
{
    public class ListResponse<T>
    {
        public CountValue Count { get; set; }

        public T[] Value { get; set; }
    }
}
