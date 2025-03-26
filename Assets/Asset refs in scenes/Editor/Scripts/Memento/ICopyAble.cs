#if UNITY_EDITOR

namespace SearchEngine.Memento
{
    public interface ICopyAble<T> : ICopyAble
    {
        new T Copy();
    }

    public interface ICopyAble
    {
        object Copy();
    }
}

#endif