using System;
using System.Collections.Generic;
using Beazley.Consumer.Net45.ServiceInterfaces;
using Beazley.ServiceModels.Net45.DNO;
using Beazley.ServiceProxy.Net45;

namespace Beazley.Consumer.Net45
{
    public class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting the consumer");
            
            var realServicePrxy = new RestServiceProxy<IRealService>("http://beazley-rulebookendava-2-2-config-server.rulebookservices.com");

            var input = new PremiumRequest
            {
                NumberOfEmployees = 5,
                Revenue = 100,
                SchemeKeys = new SchemeKeys
                {
                    EffectiveDateTime = System.DateTime.Now,
                    Keys = new List<SchemeKey>(),// { new SchemeKey { Name = "test", Value = "testValue" } },
                    ProductName = "DOProd"
                }
            };

            var result = realServicePrxy.PostAsync(x => x.GetPremiumQuote(input)).Result;

            Console.WriteLine($"premium is:  { result.GrossPremium}");
            Console.ReadLine();
        }
    }
}
