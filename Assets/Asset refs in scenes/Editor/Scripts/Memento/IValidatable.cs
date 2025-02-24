#if UNITY_EDITOR

namespace SearchEngine.Memento
{
    interface IValidatable
    {
        bool Validate();
    }
}

#endif