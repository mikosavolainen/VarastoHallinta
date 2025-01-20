using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VarastoHallinta
{
    public static class publix
    {
        public static string Domain { get; set; } = Domain;

        public static async Task<string> CheckApiStatusAsync()
        {
            string apiUrl = "https://oh3acvarasto.oh3cyt.com";
            string fallbackUrl = "http://192.168.1.105:2058";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Domain = apiUrl;
                    }
                    else
                    {
                        Domain = fallbackUrl;
                    }
                }
                catch
                {
                    Domain = fallbackUrl;
                }

                
                if (!Uri.IsWellFormedUriString(Domain, UriKind.Absolute))
                {
                    throw new UriFormatException($"Domain {Domain} is not a valid URI.");
                }

                return Domain;
            }
        }
    }
}
