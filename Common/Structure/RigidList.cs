using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OtherworldMod.Common.Structure
{
    public class RigidList<T>
    {
        public RigidList(int cap)
        {
            list = new List<(bool slotActive, T element)>(cap);
        }

        private List<(bool slotActive, T element)> list;
        private int count = 0;
        public int Count
        {
            get => count;
        }

        public T this[int index]
        {
            get
            {
                return list[index].element;
            }
            set
            {
                if (index > list.Capacity)
                    list.EnsureCapacity(index);
                list[index] = (true, value);
            }
        }
        public bool TryGet(int index, out T element)
        {
            bool Fail(out T element)
            {
                element = default(T);
                return false;
            }

            if (Count == 0)
                return Fail(out element);
            if (index < 0)
                return Fail(out element);
            if (index >= Count)
                return Fail(out element);
            if (index >= list.Capacity)
                return Fail(out element);
            if (!list[index].slotActive)
                return Fail(out element);

            element = list[index].element;
            return true;
        }

        public void Add(T element)
        {
            bool success = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].slotActive)
                {
                    this[i] = element;
                    success = true;
                    break;
                }
            }
            if (!success)
            {
                count++;
                list.Add((true,element));
            }
        }
        public void Remove(T element)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].slotActive)
                    continue;
                if (list[i].element.Equals(element))
                {
                    list[i] = (false, list[i].element);
                    break;
                }
            }

            for (int i = list.Count-1; i >= 0; i--)
            {
                if (list[i].slotActive)
                    break;
                else
                {
                    list.RemoveAt(i);
                    count--;
                }
            }
        }
        public void RemoveAt(int index)
        {
            if (index < 0)
                return;
            if (index >= Count)
                return;
            if (!list[index].slotActive)
                return;

            list[index] = (false, list[index].element);

            for (int i = list.Count-1; i >= 0; i--)
            {
                if (list[i].slotActive)
                    break;
                else
                {
                    list.RemoveAt(i);
                    count--;
                }
            }
        }

        public int IndexOf(T element)
        {
            return list.IndexOf((true, element));
        }
    }
}
