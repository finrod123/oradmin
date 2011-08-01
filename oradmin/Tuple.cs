using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 first, T2 second)
        {
            return new Tuple<T1, T2>(first, second);
        }
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 first, T2 second, T3 third)
        {
            return new Tuple<T1, T2, T3>(first, second, third);
        }
    }

    public class Tuple<T1, T2> : IEquatable<Tuple<T1, T2>>
    {
        #region Members
        T1 first;
        T2 second;
        #endregion

        #region Constructor
        public Tuple(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
        #endregion

        #region Properties
        public T1 First
        {
            get { return this.first; }
        }
        public T2 Second
        {
            get { return this.second; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Tuple<T1, T2>))
                return Equals(obj as Tuple<T1, T2>);

            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return first.GetHashCode() ^ second.GetHashCode();
        }

        #region IEquatable<Tuple<T1,T2>> Members
        public bool Equals(Tuple<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(this.first, other.First) &&
                   EqualityComparer<T2>.Default.Equals(this.second, other.Second);
        }
        #endregion
    }

    public class Tuple<T1, T2, T3> : IEquatable<Tuple<T1, T2, T3>>
    {
        #region Members
        T1 first;
        T2 second;
        T3 third;
        #endregion

        #region Constructor
        public Tuple(T1 first, T2 second, T3 third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }
        #endregion

        #region Properties
        public T1 First
        {
            get { return this.first; }
        }
        public T2 Second
        {
            get { return this.second; }
        }
        public T3 Third
        {
            get { return this.third; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Tuple<T1, T2, T3>))
                return Equals(obj as Tuple<T1, T2, T3>);

            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return first.GetHashCode() ^ second.GetHashCode() ^ third.GetHashCode();
        }

        #region IEquatable<Tuple<T1,T2, T3>> Members
        public bool Equals(Tuple<T1, T2, T3> other)
        {
            return EqualityComparer<T1>.Default.Equals(this.first, other.First) &&
                   EqualityComparer<T2>.Default.Equals(this.second, other.Second) &&
                   EqualityComparer<T3>.Default.Equals(this.third, other.Third);
        }
        #endregion
    }
}
