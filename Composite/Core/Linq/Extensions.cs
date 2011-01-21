using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Composite.Core.Linq
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public static class DictionaryExtensions
    {
        public static int GetContentHashCode(this IDictionary dictionary)
        {
            int hash = 0;

            foreach (DictionaryEntry entry in dictionary)
            {
                hash = hash ^ entry.Key.GetHashCode() ^ entry.Value.GetHashCode();
            }

            return hash;
        }
    }


    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns evaluated collection. It allows avoiding of multiple calculations for the same enumerator.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="enumerable">Enumerable object to be evaluated.</param>
        /// <returns>Evaluated collection.</returns>
        public static IEnumerable<T> Evaluate<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is T[] || enumerable is List<T>)
            {
                return enumerable;
            }
            return new List<T>(enumerable);
        }



        public static IEnumerable<T> EvaluateOrNull<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return null;

            if (enumerable is T[] || enumerable is List<T>)
            {
                return enumerable;
            }
            return new List<T>(enumerable);
        }    

        /// <summary>
        /// Extends standard IQueryable<typeparamref name="T"/>.Single method, allows specifying exception text.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="exceptionOnEmpty">Exception format for not a single row found</param>
        /// <param name="exceptionOnMultipleResults">Exception format for multiple rows found</param>
        /// <param name="formatArgs"></param>
        /// <returns></returns>
        public static T SingleOrException<T>(this IQueryable<T> query, string exceptionOnEmpty, string exceptionOnMultipleResults, params object[] formatArgs) 
        {
            var result = query.ToList();

            if (result.Count == 0) throw new InvalidOperationException(string.Format(exceptionOnEmpty, formatArgs));
            
            if (result.Count == 1) return result[0];

            throw new InvalidOperationException(string.Format(exceptionOnMultipleResults, formatArgs));
        }

        /// <summary>
        /// Extends standard IQueryable<typeparamref name="T"/>.Single method, allows specifying exception text.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="exceptionOnEmpty">Exception format for not a single row found</param>
        /// <param name="exceptionOnMultipleResults">Exception format for multiple rows found</param>
        /// <param name="formatOptions">Format arguments</param>
        /// <returns></returns>
        public static T SingleOrDefaultOrException<T>(this IQueryable<T> query, string exceptionOnMultipleResults, params object[] formatArgs) 
        {
            var result = query.ToList();

            if (result.Count == 0) return default(T);
            
            if (result.Count == 1) return result[0];

            throw new InvalidOperationException(string.Format(exceptionOnMultipleResults, formatArgs));
        }

        /// <summary>
        /// Extends standard IQueryable<typeparamref name="T"/>.First() method, allows specifying exception text.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="exceptionOnEmpty">Exception format for not a single row found</param>
        /// <param name="formatOptions">Format arguments</param>
        /// <returns></returns>
        public static T FirstOrException<T>(this IQueryable<T> query, string exceptionOnEmpty, params object[] formatArgs) where T: class
        {
            var result = query.FirstOrDefault();

            if (result == null) throw new InvalidOperationException(string.Format(exceptionOnEmpty, formatArgs));

            return result;
        }
    }



    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public static class ListExtensions
    {
        public static List<U> ToList<T, U>(this IEnumerable<T> source, Func<T, U> convertor)
        {
            List<U> result = new List<U>();

            foreach (T item in source)
            {
                result.Add(convertor(item));
            }

            return result;
        }

        public static List<object> ToListOfObjects(this IEnumerable enumerable)
        {
            var result = new List<object>();

            foreach (object o in enumerable)
            {
                result.Add(o);
            }

            return result;
        }


        public static IEnumerable<object> ToEnumerableOfObjects(this IEnumerable enumerable)
        {
            foreach (object o in enumerable)
            {
                yield return o;
            }
        }
    }
}
