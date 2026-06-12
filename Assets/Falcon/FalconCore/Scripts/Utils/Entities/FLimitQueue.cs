using Falcon.FalconCore.Scripts.Entities;

namespace Falcon.FalconCore.Scripts.Utils.Entities
{
    public class FLimitQueue<T> : FQueue<T>
    {
        private readonly int maxSize;

        public FLimitQueue(int maxSize)
        {
            this.maxSize = maxSize;
        }

        public new void Enqueue(T item)
        {
            if (Count > maxSize)
            {
                T oldestVal;
                TryDequeue(out oldestVal);
            }


            base.Enqueue(item);
        }
    }
}