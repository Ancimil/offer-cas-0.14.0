namespace Offer.Infrastructure
{
    public static class Extenstions
    {
        public static string GetApplicationNumber(this long ApplicationId)
        {
            var result = "0000000000" + ApplicationId;
            return result.Substring(result.Length - 10);
        }
    }
}
