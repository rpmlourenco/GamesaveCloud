using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;

namespace GamesaveCloudManager
{
    public class IGDBHelper
    {
        private readonly string? clientId;
        private readonly string? clientSecret;
        private string? accessToken;
        private static readonly HttpClient client = new();
        private static readonly string endpointGames = "https://api.igdb.com/v4/games";
        private static readonly string endpointToken = "https://id.twitch.tv/oauth2/token";
        private static string? cacheFilePath;

        public IGDBHelper()
        {
            var pathCurrent = Path.GetDirectoryName(Environment.ProcessPath);
            if (pathCurrent != null)
            {
                string pathCredential = Path.Combine(pathCurrent, "credential");
                Directory.CreateDirectory(pathCredential);
                cacheFilePath = Path.Combine(pathCredential, "igdb.cache");
            }

            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("igdb_secrets.json"));

            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                Stream? stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var sr = new StreamReader(stream);
                    string content = sr.ReadToEnd();
                    JObject json = JObject.Parse(content);

                    if (json != null)
                    {
                        var clientIdJ = json["client_id"];
                        var clientSecretJ = json["client_secret"];

                        if (clientIdJ != null && clientSecretJ != null)
                        {
                            clientId = clientIdJ.Value<string>();
                            clientSecret = clientSecretJ.Value<string>();
                        }
                        else
                        {
                            throw (new Exception("Data not found when parsing igbd_secrets.jsonCache"));
                        }
                    }
                    else
                    {
                        throw (new Exception("Not possible to parse igbd_secrets.jsonCache"));
                    }
                }
                else
                {
                    throw (new Exception("Resource not found when loading igdb_secrets.jsonCache"));
                }
            }
            else
            {
                throw (new Exception("Assembly not found when loading igdb_secrets.jsonCache"));
            }
        }

        private static string? ReadToken(string jsonCache)
        {
            JObject json = JObject.Parse(jsonCache);
            if (json != null && json["access_token"] != null)
            {
                var jToken = json["access_token"];
                if (jToken != null)
                {
                    return jToken.Value<string>();
                }
            }
            return null;
        }

        private async Task<bool> ReadCacheAsync()
        {
            var cache = File.Exists(cacheFilePath) ? File.ReadAllBytes(cacheFilePath) : null;
            if (cache != null)
            {

                var newToken = ReadToken(System.Text.Encoding.UTF8.GetString(cache));
                if (newToken != null)
                {
                    accessToken = newToken;
                    return true;
                }

            }

            var newToken2 = await GetNewTokenAsync();
            if (newToken2 != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void WriteCache(string jsonCache)
        {
            if (cacheFilePath != null)
            {
                File.WriteAllBytes(cacheFilePath, System.Text.Encoding.UTF8.GetBytes(jsonCache));
            }
        }

        private async Task<string?> GetNewTokenAsync()
        {
            if (clientId != null && clientSecret != null)
            {
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type","client_credentials")
                };
                client.DefaultRequestHeaders.Clear();
                using HttpResponseMessage response = await client.PostAsync(endpointToken, new FormUrlEncodedContent(values));
                using HttpContent content = response.Content;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var jsonResponse = await content.ReadAsStringAsync();
                    if (jsonResponse != null)
                    {
                        var newToken = ReadToken(jsonResponse);
                        if (newToken != null)
                        {
                            accessToken = newToken;
                            WriteCache(jsonResponse);
                            return newToken;

                        }
                    }
                }
            }
            return null;
        }

        private async Task<string?> SendRequestAsync(string endpoint, HttpContent requestContent)
        {
            var gotToken = true;
            if (accessToken == null)
            {
                gotToken = await ReadCacheAsync();
            }

            if (gotToken)
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Client-ID", clientId);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                using HttpResponseMessage response = await client.PostAsync(endpoint, requestContent);
                using HttpContent content = response.Content;

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var newToken = await GetNewTokenAsync();
                    if (newToken != null)
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Client-ID", clientId);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                        using HttpResponseMessage response2 = await client.PostAsync(endpoint, requestContent);
                        using HttpContent content2 = response2.Content;
                        if (response2.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return await content2.ReadAsStringAsync();
                        }
                    }
                }
                else
                {
                    return await content.ReadAsStringAsync();
                }

            }
            return null;
        }

        public async Task<string?> GetGameAsync(long gameId)
        {
            var requestContent = new StringContent($"fields name, url; where id = {gameId};", Encoding.UTF8, "text/plain");
            var jsonResult = await SendRequestAsync(endpointGames, requestContent);

            if (jsonResult != null)
            {
                JArray jArray = JArray.Parse(jsonResult);

                if (jArray != null && jArray.Count > 0)
                {
                    JObject json = (JObject)jArray.First();

                    var jUrlToken = json["url"];
                    if (jUrlToken != null)
                    {
                        if (jUrlToken.Value<String>() != null)
                        {
                            return jUrlToken.Value<String>();
                        }

                    }

                }
            }
            return null;
        }

        public async Task<long> GetIdAsync(string name)
        {
            var requestContent = new StringContent($"fields id; where name = \"{name}\";", Encoding.UTF8, "text/plain");
            var jsonResult = await SendRequestAsync(endpointGames, requestContent);

            if (jsonResult != null)
            {
                JArray jArray = JArray.Parse(jsonResult);

                if (jArray != null && jArray.Count > 0)
                {
                    JObject json = (JObject)jArray.First();

                    var jUrlToken = json["id"];
                    if (jUrlToken != null)
                    {
                        return jUrlToken.Value<long>();
                    }

                }
                else
                {
                    requestContent = new StringContent($"fields id; search \"{name}\";", Encoding.UTF8, "text/plain");
                    jsonResult = await SendRequestAsync(endpointGames, requestContent);

                    if (jsonResult != null)
                    {
                        jArray = JArray.Parse(jsonResult);

                        if (jArray != null && jArray.Count > 0)
                        {
                            JObject json = (JObject)jArray.First();

                            var jUrlToken = json["id"];
                            if (jUrlToken != null)
                            {
                                return jUrlToken.Value<long>();
                            }
                        }
                    }
                }
            }
            return default;
        }

    }
}
