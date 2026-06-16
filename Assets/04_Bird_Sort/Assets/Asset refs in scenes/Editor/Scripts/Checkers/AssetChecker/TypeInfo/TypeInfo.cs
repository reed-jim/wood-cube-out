#if UNITY_EDITOR
using System.Reflection;

namespace SearchEngine.Checkers
{
    /// <summary>
    /// TypeInfo can store and determine type`s fields and properties
    /// </summary>
    public class TypeInfo
    {
        private FieldInfo[] fields;
        private FieldInfo[] fieldArrays;
        private PropertyInfo[] props;
        private PropertyInfo[] propArrays;
        
        public FieldInfo[] Fields
        {
            get { return fields; }
            set
            {
                if (value != null)
                    fields = value;
                else
                    fields = new FieldInfo[0];
            }
        }

        public FieldInfo[] FieldArrays
        {
            get { return fieldArrays; }
            set
            {
                if (value != null)
                    fieldArrays = value;
                else
                    fieldArrays = new FieldInfo[0];
            }
        }

        public PropertyInfo[] Props
        {
            get { return props; }
            set
            {
                if (value != null)
                    props = value;
                else
                    props = new PropertyInfo[0];
            }
        }

        public PropertyInfo[] PropArrays
        {
            get { return propArrays; }
            set
            {
                if (value != null)
                    propArrays = value;
                else
                    propArrays = new PropertyInfo[0];
            }
        }

        public bool IsEmpty
        {
            get { return Fields.Length + FieldArrays.Length + Props.Length + PropArrays.Length == 0; }

        }
           
        public TypeInfo(FieldInfo[] fields, FieldInfo[] fieldArrays, PropertyInfo[] properties, PropertyInfo[] propArrays)
        {
            Fields = fields;
            FieldArrays = fieldArrays;
            Props = properties;
            PropArrays = propArrays;
        }

        public static TypeInfo Empty
        {
            get
            {
                return new TypeInfo(new FieldInfo[0], new FieldInfo[0], new PropertyInfo[0], new PropertyInfo[0]);
            }
        }
    }
}
#endif

