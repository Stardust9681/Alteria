namespace OtherworldMod.Common.Interface
{
    public interface IClonable<T> where T : IClonable<T>
    {
        public T Clone();
    }
}
