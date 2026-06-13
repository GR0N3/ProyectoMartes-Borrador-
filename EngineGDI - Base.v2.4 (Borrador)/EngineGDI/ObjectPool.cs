using System;
using System.Collections.Generic;

namespace EngineGDI
{
    public class ObjectPool<T> where T : class
    {
        private readonly List<T> pool;
        private readonly Func<T> factoryMethod;
        private readonly Func<T, bool> checkIfActive;
        private readonly Action<T> deactivateMethod;

        public IReadOnlyList<T> Items => pool;

        public ObjectPool(int initialCapacity, Func<T> factoryMethod, Func<T, bool> checkIfActive, Action<T> deactivateMethod)
        {
            this.factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));
            this.checkIfActive = checkIfActive ?? throw new ArgumentNullException(nameof(checkIfActive));
            this.deactivateMethod = deactivateMethod ?? throw new ArgumentNullException(nameof(deactivateMethod));
            this.pool = new List<T>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++)
            {
                T item = factoryMethod();
                deactivateMethod(item);
                pool.Add(item);
            }
        }

        public T Get()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!checkIfActive(pool[i]))
                    return pool[i];
            }

            T newItem = factoryMethod();
            deactivateMethod(newItem);
            pool.Add(newItem);
            return newItem;
        }
    }
}
