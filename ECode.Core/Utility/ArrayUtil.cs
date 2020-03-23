using System;

namespace ECode.Utility
{
    public static class ArrayUtil
    {
        /// <summary>
        /// Returns hash code for the array which is generated based on the elements.
        /// </summary>
        /// <remarks>
        /// Hash code returned by this method is guaranteed to be the same for
        /// arrays with equal elements.
        /// </remarks>
        /// <param name="array">
        /// Array to calculate hash code for.
        /// </param>
        /// <returns>
        /// A hash code for the specified array.
        /// </returns>
        public static int GetHashCode(Array array)
        {
            if (array == null || array.Length == 0)
            { return 0; }

            int hashCode = 0;
            for (int i = 0; i < array.Length; i++)
            {
                object elem = array.GetValue(i);
                if (elem == null)
                { continue; }

                if (elem is Array)
                {
                    hashCode += 17 * GetHashCode(elem as Array);
                }
                else
                {
                    hashCode += 13 * elem.GetHashCode();
                }
            }

            return hashCode;
        }

        /// <summary>
        /// Tests equality of two single-dimensional arrays by checking each element
        /// for equality.
        /// </summary>
        /// <param name="arrayA">The first array to be checked.</param>
        /// <param name="arrayB">The second array to be checked.</param>
        /// <returns>True if arrays are the same, false otherwise.</returns>
        public static bool AreEqual(Array arrayA, Array arrayB)
        {
            if (arrayA == null && arrayB == null)
            { return true; }

            if (arrayA != null && arrayB != null)
            {
                if (arrayA.Length != arrayB.Length)
                { return false; }

                for (int i = 0; i < arrayA.Length; i++)
                {
                    object elemA = arrayA.GetValue(i);
                    object elemB = arrayB.GetValue(i);

                    if (elemA is Array && elemB is Array)
                    {
                        if (!AreEqual(elemA as Array, elemB as Array))
                        {
                            return false;
                        }
                    }
                    else if (!Equals(elemA, elemB))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Concatenates 2 arrays of compatible element types
        /// </summary>
        /// <remarks>
        /// If either of the arguments is null, the other array is returned as the result. 
        /// The array element types may differ as long as they are assignable. 
        /// The result array will be of the "smaller" element type.
        /// </remarks>
        public static Array Concat(Array first, Array second)
        {
            if (first == null)
            { return second; }

            if (second == null)
            { return first; }

            Type resultElementType;
            Type firstElementType = first.GetType().GetElementType();
            Type secondElementType = second.GetType().GetElementType();
            if (firstElementType.IsAssignableFrom(secondElementType))
            {
                resultElementType = firstElementType;
            }
            else if (secondElementType.IsAssignableFrom(firstElementType))
            {
                resultElementType = secondElementType;
            }
            else
            {
                throw new ArgumentException($"Array element types '{firstElementType}' and '{secondElementType}' are not compatible");
            }

            Array result = Array.CreateInstance(resultElementType, first.Length + second.Length);
            Array.Copy(first, result, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);

            return result;
        }
    }
}