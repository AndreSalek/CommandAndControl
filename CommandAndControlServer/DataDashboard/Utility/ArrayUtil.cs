namespace DataDashboard.Utility
{
    public static class ArrayUtil
    {
        public static byte[] RemoveTrailingNulls(byte[] array)
        {
            int i = array.Length - 1;
            while (array[i] == 0)
            {
                i--;
            }
            byte[] newArray = new byte[i + 1];
            Array.Copy(array, newArray, i + 1);
            return newArray;
        }
    }
}
