using Grpc.Core;
using Microsoft.Extensions.Logging;
using SchoolOfRock;
using System.Threading.Tasks;

namespace Einstein
{
    public class PalindromeFinderService
        : PalindromeFinder.PalindromeFinderBase
    {
        private readonly ILogger<PalindromeFinderService> _logger;
        public PalindromeFinderService(ILogger<PalindromeFinderService> logger)
        {
            _logger = logger;
        }

        public override async Task<PalindromeReply> IsItPalindrome(PalindromeRequest request, ServerCallContext context)
        {
            long r, sum = 0, t;
            var num = request.Number;
            for (t = num; num != 0; num /= 10)
            {
                r = num % 10;
                sum = sum * 10 + r;
            }
            if (t == sum)
                return new PalindromeReply { IsPalindrome = true };
            else
                return new PalindromeReply { IsPalindrome = false };
        }
    }
}