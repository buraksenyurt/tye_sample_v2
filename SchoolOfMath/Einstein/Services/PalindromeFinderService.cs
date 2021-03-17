using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SchoolOfRock;
using System;
using System.Threading.Tasks;

namespace Einstein
{
    public class PalindromeFinderService
        : PalindromeFinder.PalindromeFinderBase
    {
        private readonly ILogger<PalindromeFinderService> _logger;
        private readonly IDistributedCache _cache;
        private readonly PalindromeReply True = new() { IsPalindrome = true };
        private readonly PalindromeReply False = new() { IsPalindrome = false };
        public PalindromeFinderService(ILogger<PalindromeFinderService> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache; //Dağıtık cache servisi olarak Redis konumlanacak. Startup'ta onu ekledik çünkü.
        }

        public override async Task<PalindromeReply> IsItPalindrome(PalindromeRequest request, ServerCallContext context)
        {
            long r, sum = 0, t;
            var number = request.Number;

            var inCache = await _cache.GetStringAsync(request.Number.ToString()); // bu sayı Redis Cache'te var mı?
            if (inCache=="YES")
            {
                _logger.LogInformation($"{request.Number} palindrom bir sayıdır ve şu an Redis'ten getiriyorum. Hesap etmeye gerek yok");
                return True;
            }

            for (t = number; number != 0; number /= 10)
            {
                r = number % 10;
                sum = sum * 10 + r;
            }
            if (t == sum)
            {
                _logger.LogInformation($"{request.Number} palindrom bir sayı ama Redis cache'e atılmamış. Şimdi ekleyeceğim.");
                // Sayı adını Key olarak kullanıp Cache'e atıyoruz ve ona value olarak YES değerini atıyoruz.
                await _cache.SetStringAsync(request.Number.ToString(), "YES", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                });
                // Palindrome sayı ise onu Redis Cache'e atıyoruz.
                return True;
            }
            else
                return False;
        }
    }
}