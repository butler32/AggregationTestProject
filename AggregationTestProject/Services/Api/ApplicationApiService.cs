using AggregationTestProject.DTOs;
using AggregationTestProject.Services.Utilities;
using AggregationTestProject.Utilities.Interfaces;
using AngleSharp.Html.Parser;
using log4net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Windows.Xps;

namespace AggregationTestProject.Services.Api
{
    public class ApplicationApiService : IConnactableDevice
    {
        private readonly ILog _log;
        private readonly ApiService _apiService;
        private readonly InternetDeviceHelper _internetDeviceHelper;

        public event EventHandler<bool> ConnectionStateChanged;
        public string Ip { get; private set; }
        public string OperatorIp { get; private set; }
        public TimeSpan PingInterval { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }

        public ApplicationApiService(
            ILog log,
            ApiService apiService,
            InternetDeviceHelper internetDeviceHelper,
            string ip,
            string operatorIp,
            TimeSpan pingInterval,
            string email,
            string password)
        {
            _log = log;
            _apiService = apiService;
            _internetDeviceHelper = internetDeviceHelper;
            Ip = ip;
            OperatorIp = operatorIp;
            PingInterval = pingInterval;
            Email = email;
            Password = password;
        }

        public async Task Login()
        {
            try
            {
                var uri = "login";

                var getLoginResponse = await _apiService.MakeRequestAsync(uri);
                var stringResponse = await getLoginResponse.ReadAsStringAsync();

                var csrfToken = AppHtmlParser.GetCsrfTokenValue(stringResponse);

                var rememberMeValue = "on";

                var content = new MultipartFormDataContent
                {
                    { new StringContent(Email, Encoding.UTF8, MediaTypeNames.Text.Plain), "email" },
                    { new StringContent(Password, Encoding.UTF8, MediaTypeNames.Text.Plain), "password" },
                    { new StringContent(csrfToken!, Encoding.UTF8, MediaTypeNames.Text.Plain), "_csrf_token" },
                    { new StringContent(rememberMeValue, Encoding.UTF8, MediaTypeNames.Text.Plain), "_remember_me" },
                };

                await _apiService.MakeRequestAsync(uri, HttpMethod.Post, content);
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: login error", ex);

                throw;
            }
        }

        public async Task<MissionDto> GetCurrentMissionAsync()
        {
            try
            {
                var productUrl = "client/api/get/task/";
                var parameters = new Dictionary<string, string>
                {
                    { "operatorIp", OperatorIp }
                };

                var apiResponse = await _apiService.MakeRequestAsync(productUrl, parameters:parameters);
                var jsonResponse = await apiResponse.ReadAsStringAsync();
                var mission = JsonConvert.DeserializeObject<MissionResponseDto>(jsonResponse);

                return mission!.Mission;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: get current mission error", ex);

                throw;
            }
        }

        public async Task<InitializedBox> InitializeBoxAsync()
        {
            try
            {
                var initializeBoxUri = "client/api/initialize/box";

                var parameters = new Dictionary<string, string>
                {
                    {"operatorIp", OperatorIp }
                };

                var apiRepsonse = await _apiService.MakeRequestAsync(initializeBoxUri, parameters: parameters);
                var jsonResponse = await apiRepsonse.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<InitializedBoxResult>(jsonResponse);

                return result.Box;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: initialize box error", ex);

                throw;
            }
        }

        public async Task<BoxWorkplace> ShowBoxWorkplaceAsync(bool useOperator1 = true)
        {
            try
            {
                var uri = "user/show/box-workplace/";

                var parameters = new Dictionary<string, string>
                {
                    { "operatorIp", OperatorIp }
                };

                var apiResponse = await _apiService.MakeRequestAsync(uri, method: HttpMethod.Get, parameters: parameters);
                var stringResponse = await apiResponse.ReadAsStringAsync();

                var result = AppHtmlParser.GetBoxWorkplace(stringResponse);

                return result;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: get box workplace error", ex);

                throw;
            }
        }

        public async Task<PutItemInBoxResult> PutItemInBoxAsync(string itemCode, bool useOperator1 = true)
        {
            string url = $"client/api/put/item/in/box/";

            var parameters = new Dictionary<string, string>
            {
                { "operatorIp", OperatorIp }
            };

            var token = "_!mySecretToken!_";

            try
            {
                var content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("token", token),
                    new KeyValuePair<string, string>("code", itemCode)
                ]);

                var apiResponse = await _apiService.MakeRequestAsync(url, method: HttpMethod.Post, content: content, parameters: parameters);
                var responseStr = await apiResponse.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PutItemInBoxResult>(responseStr);

                return result;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: put item in box error", ex);

                throw;
            }
        }

        public async Task<BindLabelCodeWithBox> BindCodeFromLabelWithFilledBox(int boxId, string boxCode, bool useOperator1 = false)
        {
            try
            {
                var uri = $"client/api/bind/code/from/label/with/filled/box/";
                var token = "_!mySecretToken!_";
                var parameters = new Dictionary<string, string>
                {
                    { "operatorIp", OperatorIp }
                };

                var content = new MultipartFormDataContent
                {
                    { new StringContent(token, Encoding.UTF8, MediaTypeNames.Text.Plain), "token" },
                    { new StringContent(OperatorIp, Encoding.UTF8, MediaTypeNames.Text.Plain), "operatorIp" },
                    { new StringContent(boxCode, Encoding.UTF8, MediaTypeNames.Text.Plain), "boxCode" },
                    { new StringContent(boxId.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain), "boxId" }
                };

                var apiResponse = await _apiService.MakeRequestAsync(uri, method: HttpMethod.Post, content: content, parameters: parameters);
                var responseStr = await apiResponse.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BindLabelCodeWithBoxResult>(responseStr);

                return result.Box;
            }
            catch (Exception ex)
            {
                throw new Exception($"Привязка кода к гр. упаковке: {ex.Message}", ex);
            }
        }

        public async Task<PalletWorkplace> ShowPalletWorkplaceAsync(bool useOperator1 = true)
        {
            var uri = "user/show/pallet-workplace/";

            var parameters = new Dictionary<string, string>
            {
                { "operatorIp", OperatorIp }
            };

            try
            {
                var apiResponse = await _apiService.MakeRequestAsync(uri, method: HttpMethod.Get, parameters: parameters);
                var stringResponse = await apiResponse.ReadAsStringAsync();

                var result = AppHtmlParser.GetPalletWorkplace(stringResponse);

                return result;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: get pallet workplace error", ex);

                throw;
            }
        }

        public async Task<PutBoxOnPalletResult> PutBoxOnPalletAsync(string boxCode, bool useOperator1 = true)
        {
            string url = $"client/api/put/box/on/pallet/";

            var parameters = new Dictionary<string, string>
            {
                { "operatorIp", OperatorIp }
            };

            try
            {
                var content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("code", boxCode)
                ]);

                var apiResponse = await _apiService.MakeRequestAsync(url, method: HttpMethod.Post, content: content, parameters: parameters);
                var responseStr = await apiResponse.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PutBoxOnPalletResult>(responseStr);

                return result;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: put box on pallet error", ex);

                throw;
            }
        }

        public async Task<InitializedPallet> InitializePalletAsync(int operatorNumber = 1)
        {
            string url = $"client/api/initialize/pallet/";

            var parameters = new Dictionary<string, string>
            {
                { "operatorIp", OperatorIp }
            };

            try
            {
                var content = new MultipartFormDataContent
                {
                    { new StringContent(OperatorIp, Encoding.UTF8, "application/json"), "operatorIp" },
                };

                var apiResponse = await _apiService.MakeRequestAsync(url, method: HttpMethod.Post, content: content, parameters: parameters);

                var result = JsonConvert.DeserializeObject<InitializedPalletResult>(await apiResponse.ReadAsStringAsync());

                return result.Pallet;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: initialize pallet error", ex);

                throw new Exception($"Обработка запроса инициализации тр. упаковки: {ex.Message}", ex);
            }
        }

        public async Task<TaskShiftResponseDto> SetShiftAsync(int shift)
        {
            try
            {
                var uri = "client/api/set/task/shift/";
                var parameters = new Dictionary<string, string>
                {
                    { "operatorIp", OperatorIp }
                };

                var token = "_!mySecretToken!_";
                var content = new MultipartFormDataContent
                {
                    { new StringContent(token, Encoding.UTF8, MediaTypeNames.Text.Plain), "token" },
                    { new StringContent(shift.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain), "taskShift" },
                };

                var apiResponse = await _apiService.MakeRequestAsync(uri, method: HttpMethod.Post, parameters: parameters, content: content);
                var stringResponse = await apiResponse.ReadAsStringAsync();
                var taskShift = JsonConvert.DeserializeObject<TaskShiftResponseDto>(stringResponse);

                return taskShift;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: set shift error", ex);

                throw;
            }
        }

        public async Task<PalletInfoDto> GetPalletInfoByBoxCode(string code)
        {
            try
            {
                var uri = "client/api/get/pallet/info/by/box/code/";
                var parameters = new Dictionary<string, string>
                {
                    { "operatorIp", OperatorIp }
                };

                var token = "_!mySecretToken!_";

                var jsonCode = JsonConvert.SerializeObject(new List<string>{ code });

                var content = new MultipartFormDataContent
                {
                    { new StringContent(token, Encoding.UTF8, MediaTypeNames.Text.Plain), "token" },
                    { new StringContent(jsonCode, Encoding.UTF8, MediaTypeNames.Text.Plain), "code" },
                };

                var apiResponse = await _apiService.MakeRequestAsync(uri, method: HttpMethod.Post, parameters: parameters, content: content);
                var stringResponse = await apiResponse.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PalletInfoResultDto>(stringResponse);

                return result.Pallet;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: get pallet error");

                throw;
            }
        }

        public async Task<PalletInfoDto> ManualClosePalletAsync(int id)
        {
            try
            {
                var uri = $"client/api/close/not/full/pallet/{id}/";
                var parameters = new Dictionary<string, string>
                {
                    { "operatorIp", OperatorIp }
                };

                var apiResponse = await _apiService.MakeRequestAsync(uri, method: HttpMethod.Get, parameters: parameters);
                var stringResponse = await apiResponse.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PalletInfoResultDto>(stringResponse);

                return result.Pallet;
            }
            catch (Exception ex)
            {
                _log.Debug("App api service: close unfilled pallet error");

                throw;
            }
        }
    }
}
