namespace Integration.Setup;

public static class QueryBuilder
{
    public static List<KeyValuePair<string, string?>> Create() => [];

    extension(List<KeyValuePair<string, string?>> query)
    {
        public void Add(string key, string? value)
        {
            if (value is null)
                return;

            query.Add(new KeyValuePair<string, string?>(key, value));
        }

        public void AddRange<T>(string key, IEnumerable<T>? values)
        {
            if (values is null)
                return;

            query.AddRange(
                values.Select(v => new KeyValuePair<string, string?>(key, v?.ToString()))
            );
        }
    }
}
