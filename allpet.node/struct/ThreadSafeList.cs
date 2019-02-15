using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Module.Struct
{
    class ThreadSafeQueueWithKey<T>
    {
        System.Collections.Concurrent.ConcurrentQueue<T> queue = new System.Collections.Concurrent.ConcurrentQueue<T>();
        System.Collections.Concurrent.ConcurrentDictionary<T, int> bag = new System.Collections.Concurrent.ConcurrentDictionary<T, int>();
        List<T> list = new List<T>();


        public int Count
        {
            get
            {

                return queue.Count;

            }
        }


        public bool Enqueue(T item)
        {
            if (bag.ContainsKey(item))
                return false;
            queue.Enqueue(item);
            bag[item] = 1;
            return true;
        }
        public T Dequeue()
        {
            if(queue.TryDequeue(out T result))
            {
                bag.TryRemove(result, out int _);
                return result;
            }
            return default(T);
        }
        public void Clear()
        {
            queue.Clear();
            bag.Clear();
        }
        public bool Contains(T item)
        {
            return bag.ContainsKey(item);
        }

        public T Getqueue(string key)
        {
            foreach(var item in queue)
            {
                if(item.ToString()== key)
                {
                    return item;
                }
            }
            return default(T);
        }

    }
}
