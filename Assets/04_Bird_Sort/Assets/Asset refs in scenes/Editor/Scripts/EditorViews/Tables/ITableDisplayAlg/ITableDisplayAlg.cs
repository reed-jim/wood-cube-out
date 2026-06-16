#if UNITY_EDITOR

namespace SearchEngine.EditorViews.Tables
{
    public interface ITableDisplayAlg
    {
        void ShowPreTitle();
        void ShowTitle();
        void ShowRow(int rowNum);
        void AfterTable();
    }
}

#endif