using MicroserviceCommon.Application;

namespace Offer
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return BaseProgram<Startup>.Main(args);
        }
    }
}
