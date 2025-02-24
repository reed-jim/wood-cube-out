#if UNITY_EDITOR 

using System;
using SearchEngine.Memento;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    [Serializable]
    public class RSW : IValidatable
    {
        public ResultsSWFactoryTypes type;
        public RswSimpleFacade.Memento simple;
        public RSWComplex0Facade.Memento complex0;
        public RSWComplex1Facade.Memento complex1;
        public RSWComplex2Facade.Memento complex2;
        public RSWComplex3Facade.Memento complex3;

        public bool Validate()
        {                                                                                         
            return
                type == ResultsSWFactoryTypes.Simple && simple != null && simple.Validate()
                || type == ResultsSWFactoryTypes.Complex0 && complex0!=null && complex0.Validate()
                || type == ResultsSWFactoryTypes.Complex1 && complex1!=null && complex1.Validate()
                || type == ResultsSWFactoryTypes.Complex2 && complex2!=null && complex2.Validate()
                || type == ResultsSWFactoryTypes.Complex3 && complex3!=null && complex3.Validate();
        }
    } 
}

#endif