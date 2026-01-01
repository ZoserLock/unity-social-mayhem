using System.Collections;
using System.Collections.Generic;

namespace Zen.Core
{
    // TODO: This look weird, I think it should manage the list of items instead of relying on the user to manage it.
    // Implementation of a shuffle bag. A shuffle bag is a collection with a random element selection. 
    // When an element is selected, it is removed from future selections until all elements have been selected.
    public class ShuffleBag<T>
    {
        private readonly System.Random _random;
        private readonly List<T> _objectBag = new List<T>();
        private int _index = 0;

        public ShuffleBag(int seed = -1)
        {
            if (seed == -1)
            {
                _random = new System.Random();
            }
            else
            {
                _random = new System.Random(seed);
            }
        }

        public void Shuffle()
        {
            _index = _objectBag.Count - 1;
        }

        public void clear()
        {
            _objectBag.Clear();
            _index = -1;
        }

        public void Add(T obj, int quantity)
        {
            if (quantity > 0)
            {
                for (int a = 0; a < quantity; ++a)
                {
                    _objectBag.Add(obj);
                }
            }
        }

        public T Next()
        {
            if (_objectBag.Count <= 0)
            {
                return default(T);
            }

            if (_index < 0)
            {
                Shuffle();
            }

            int selected = _random.Next(_index + 1);

            T temp = _objectBag[selected];
            _objectBag[selected] = _objectBag[_index];
            _objectBag[_index] = temp;

            _index--;

            return temp;
        }
    }
}