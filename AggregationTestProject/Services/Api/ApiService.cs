using log4net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Web;
using Unity;

namespace AggregationTestProject.Services.Api
{
    public class ApiService
    {
        private readonly IUnityContainer _unityContainer;
        private readonly ILog _log;
        private readonly SemaphoreSlim _semaphore;

        private bool _isDebug = true;
        private int _delayAfterRequest = 1000;

        public ApiService(ILog log, IUnityContainer unityContainer)
        {
            _log = log;
            _unityContainer = unityContainer;

            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<HttpContent> MakeRequestAsync(
            string url,
            HttpMethod method = null,
            HttpContent content = null,
            Dictionary<string, string> parameters = null)
        {
            await _semaphore.WaitAsync();

            try
            {
                var client = _unityContainer.Resolve<HttpClient>();

                url = BuildQueryString(url, parameters);

                try
                {
                    var requestMessage = new HttpRequestMessage(method ?? HttpMethod.Get, url)
                    {
                        Content = content
                    };

                    var httpResponse = await client.SendAsync(requestMessage);
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    if (ExtractError(responseContent))
                    {
                        throw new InvalidOperationException("Api error");
                    }

                    return httpResponse.Content;
                }
                catch (HttpRequestException ex)
                {
                    _log.Debug("Api service: http request error", ex);

                    throw;
                }
                catch (TaskCanceledException ex)
                {
                    _log.Debug("Api service: timeout error", ex);

                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    _log.Debug("Api service: error", ex);

                    throw;
                }
                catch (Exception ex)
                {
                    _log.Debug("Api service: unknown error", ex);

                    throw;
                }
            }
            finally
            {
                if (_isDebug)
                {
                    await Task.Delay(_delayAfterRequest);
                }

                _semaphore.Release();
            }
        }

        private string BuildQueryString(string url, Dictionary<string, string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                var queryString = string.Join("&", parameters.Select(p => $"{HttpUtility.UrlEncode(p.Key)}={HttpUtility.UrlEncode(p.Value)}"));
                if (url.Contains("?"))
                {
                    return $"{url}&{queryString}";
                }
                else
                {
                    return $"{url}?{queryString}";
                }
            }
            else
            {
                return url;
            }
        }

        static bool ExtractError(string jsonString)
        {
            try
            {
                var jsonObject = JObject.Parse(jsonString)["error"];

                if (jsonObject is JObject errorObject)
                {
                    return true;
                }
                else
                {
                    if (jsonObject is null)
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
