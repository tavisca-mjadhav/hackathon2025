namespace OrderAPI.Common
{
    public static class CommonUtils
    {
        public static string GenerateRandomAlphanumeric()
        {
            int length = 5;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
