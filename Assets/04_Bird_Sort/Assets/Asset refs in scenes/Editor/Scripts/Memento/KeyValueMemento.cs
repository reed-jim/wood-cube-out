#if UNITY_EDITOR

namespace SearchEngine.Memento
{
    public class KeyValueMemento<T1, T2>
    {
        public T1 Key;
        public T2 Value;

        public KeyValueMemento(){}

        public KeyValueMemento(T1 key, T2 value)
        {
            Key = key;
            Value = value;
        }

        public void SetKeyValue(T1 key, T2 value)
        {
            Key = key;
            Value = value;
        }
    }
}

#endif