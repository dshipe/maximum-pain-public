using Microsoft.Extensions.Options;

namespace UnitTestProject1
{
    public class Options<T> : IOptions<T> where T : class, new()
    {
        public T Value { get; set; }

        public Options(T value)
        {
            Value = value;
        }
    }
}
