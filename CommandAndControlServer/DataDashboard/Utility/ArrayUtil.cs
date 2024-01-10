namespace DataDashboard.Utility
{
    public static class ArrayUtil
    {
        /// <summary>
        /// Removes trailing nulls from a byte array
        /// </summary>
        /// <returns>New byte[] that does not contain 0x00 at the end </returns>
        public static byte[] RemoveTrailingNulls(byte[] array)
        {
            int i = array.Length - 1;
            while (array[i] == 0x00)
            {
                i--;
            }
            byte[] newArray = new byte[i + 1];
            Array.Copy(array, newArray, i + 1);
            return newArray;
        }
    }
}
