#if UNITY_EDITOR

namespace SearchEngine.Memento
{
    public interface IOriginator<T>
    {
        T GetMemento();
        void SetMemento(T memento);
    }
}

#endif