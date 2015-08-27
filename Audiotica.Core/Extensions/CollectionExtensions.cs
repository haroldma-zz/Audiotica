namespace Audiotica.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static void Fill<T>(this T[] array, T value)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }
    }
}