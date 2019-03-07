using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MSGraph.Response;
using MSGraph.Request;
using Microsoft.Identity.Client;
using System.Linq;

namespace MSGraph
{
    public class GraphService
    {
        //Hello Graph
        private static readonly string graphEndpoint = "https://graph.microsoft.com/";
        private static readonly string graphVersion = "v1.0/me"; //beta
        

        private readonly string accessToken = string.Empty;
        private HttpClient httpClient = null;
        private readonly JsonSerializerSettings jsonSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        //This sample app implements Azure functions designed to be invoked via Microsoft Flow to provision a Microsoft Team when a new flight is added to a master list in SharePoint.The sample uses Microsoft Graph to do the following provisioning tasks:
        //https://github.com/microsoftgraph/contoso-airlines-azure-functions-sample/tree/master/create-flight-team
        private static readonly string teamsEndpoint = Environment.GetEnvironmentVariable("TeamsEndpoint");
        private static readonly string sharePointEndpoint = Environment.GetEnvironmentVariable("SharePointEndPoint");

        // Below are the clientId (Application Id) of your app registration and the tenant information. 
        // You have to replace:
        // - the content of ClientID with the Application Id for your app registration
        // - Te content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for any Work or School accounts, or Microsoft personal account, use common
        //   - for Microsoft Personal account, use consumers

        private static string ClientId = "0b8b0665-bc13-4fdc-bd72-e0227b9fc011";
        private static string Tenant = "common";
        public static PublicClientApplication PublicClientApp { get; } = new PublicClientApplication(ClientId, $"https://login.microsoftonline.com/{Tenant}");
        public static string TokenForUser = null;
        public static DateTimeOffset Expiration;
        public static string[] Scopes = { "user.read", "Files.Read", "Calendars.Read" };

        //private TraceWriter logger = null;


        public GraphService(string accessToken)
        {
            this.accessToken = accessToken;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

        }
        public static async Task<bool> SignOut()
        {
            IEnumerable<IAccount> accounts = await GraphService.PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();
            try
            {
                await GraphService.PublicClientApp.RemoveAsync(firstAccount);
                return true;
            }
            catch (MsalException ex)
            {

                System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
            }
        }

        /// <summary>
        /// Get Auth Result for User
        /// </summary>
        /// <returns>AuthenticationResult</returns>
        public static async Task<AuthenticationResult> GetAuthResult()
        {
            IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();
            return  await PublicClientApp.AcquireTokenSilentAsync(Scopes, firstAccount);
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        public static async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUserAsync()
        {
            IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();

            AuthenticationResult authResult;
            try
            {
                authResult = await PublicClientApp.AcquireTokenSilentAsync(Scopes, firstAccount);
                TokenForUser = authResult.AccessToken;
            }

            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await PublicClientApp.AcquireTokenAsync(Scopes);
                }
                catch (MsalException msalex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                }
            }

            catch (Exception ex)
            {
                if (TokenForUser == null || Expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    authResult = await PublicClientApp.AcquireTokenAsync(Scopes);

                    TokenForUser = authResult.AccessToken;
                    Expiration = authResult.ExpiresOn;
                }
                System.Diagnostics.Debug.WriteLine($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
            }

            return TokenForUser;
        }

        public async Task<ItemInfoResponse> GetAppRoot()
        {
            return await GetSpecialFolder(SpecialFolder.AppRoot);
        }

        // /drive/special/{special}
        public async Task<ItemInfoResponse> GetSpecialFolder(SpecialFolder kind)
        {
            if (kind == SpecialFolder.None)
            {
                throw new ArgumentException("Please use a value other than None", nameof(kind));
            }

            //Special folder private const string RequestSpecialFolder = "/drive/special/{0}";
            var response = await MakeGraphCall(HttpMethod.Get, $"/drive/special/{kind}");
            var sf= JsonConvert.DeserializeObject<ItemInfoResponse>(await response.Content.ReadAsStringAsync());
            return sf;

        }

        public async Task<ItemInfoResponse> GetPhotosAndImagesFromFolder(string path)
        {
            //https://docs.microsoft.com/en-us/graph/api/resources/onedrive?view=graph-rest-1.0
            ///me/drive/root:/path/to/folder
            //https://graph.microsoft.com/v1.0/me/drive/root:/Bilder/Karneval2019
            var response = await MakeGraphCall(HttpMethod.Get, $"/drive/root:{path}");
            var sf = JsonConvert.DeserializeObject<ItemInfoResponse>(await response.Content.ReadAsStringAsync());
            return sf;
        }

        public async Task<IList<ItemInfoResponse>> PopulateChildren(ItemInfoResponse info)
        {
            var response = await MakeGraphCall(HttpMethod.Get, $"/drive/items/{info.Id}/children");
            var l = JsonConvert.DeserializeObject<ParseChildrenResponse>(await response.Content.ReadAsStringAsync());
            return l.Value;
        }

        public async Task<string> GetCalendarView()
        {
            //https://graph.microsoft.com/v1.0/me/calendarView?startdatetime=2019-02-12&enddatetime=2019-02-19&$select=subject,Start,End
            try
            {
                var response = await MakeGraphCall(HttpMethod.Get, $"/calendarView?startdatetime=2019-02-12&enddatetime=2019-02-19&$select=subject,Start,End");
                //var ce = JsonConvert.DeserializeObject<CalendarEvent>(await response.Content.ReadAsStringAsync())
                var calendarevents = await response.Content.ReadAsStringAsync();
                return calendarevents;


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<DriveItem> GetOneDriveItemAsync(string itemId)
        {
            //https://docs.microsoft.com/en-us/graph/api/driveitem-get?view=graph-rest-1.0
            // /me/drive/items/{item-id}
            var response = await MakeGraphCall(HttpMethod.Get, $"/drive/items/{itemId}");
            return JsonConvert.DeserializeObject<DriveItem>(await response.Content.ReadAsStringAsync());
        }

        //public async Task<DriveItem> GetTeamOneDriveFolderAsync(string teamId, string folderName)
        //{
        //    // Retry this call twice if it fails
        //    // There seems to be a delay between creating a Team and the drives being
        //    // fully created/enabled
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/groups/{teamId}/drive/root:/{folderName}", retries: 4);
        //    return JsonConvert.DeserializeObject<DriveItem>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task CopySharePointFileAsync(string siteId, string itemId, ItemReference target)
        //{
        //    var copyPayload = new DriveItem
        //    {
        //        ParentReference = target
        //    };

        //    var response = await MakeGraphCall(HttpMethod.Post,
        //        $"/sites/{siteId}/drive/items/{itemId}/copy",
        //        copyPayload);
        //}

        //public async Task<List<string>> GetUserIds(string[] pilots, string[] flightAttendants)
        //{
        //    var userIds = new List<string>();

        //    // Look up each user to get their Id property
        //    foreach (var pilot in pilots)
        //    {
        //        var user = await GetUserByUpn(pilot);
        //        userIds.Add($"{graphEndpoint}users/{user.Id}");
        //    }

        //    foreach (var flightAttendant in flightAttendants)
        //    {
        //        var user = await GetUserByUpn(flightAttendant);
        //        userIds.Add($"{graphEndpoint}users/{user.Id}");
        //    }

        //    return userIds;
        //}

        //public async Task<User> GetMe()
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/me");
        //    return JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<User> GetUserByUpn(string upn)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/users/{upn}");
        //    return JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<Group> CreateGroupAsync(Group group)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, "/groups", group);
        //    return JsonConvert.DeserializeObject<Group>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task AddOpenExtensionToGroupAsync(string groupId, ProvisioningExtension extension)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/groups/{groupId}/extensions", extension);
        //}

        //public async Task CreateTeamAsync(string groupId, Team team)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Put, $"/groups/{groupId}/team", team);
        //}

        //public async Task<Invitation> CreateGuestInvitationAsync(Invitation invite)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, "/invitations", invite);
        //    return JsonConvert.DeserializeObject<Invitation>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task AddMemberAsync(string teamId, string userId, bool isOwner = false)
        //{
        //    var addUserPayload = new AddUserToGroup() { UserPath = $"{graphEndpoint}beta/users/{userId}" };
        //    await MakeGraphCall(HttpMethod.Post, $"/groups/{teamId}/members/$ref", addUserPayload);

        //    // Step 3 -- Add the ID to the owners of group if requested
        //    if (isOwner)
        //    {
        //        await MakeGraphCall(HttpMethod.Post, $"/groups/{teamId}/owners/$ref", addUserPayload);
        //    }
        //}

        //public async Task<GraphCollection<Channel>> GetTeamChannelsAsync(string teamId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/teams/{teamId}/channels");
        //    return JsonConvert.DeserializeObject<GraphCollection<Channel>>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task CreateChatThreadAsync(string teamId, string channelId, ChatThread thread)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/teams/{teamId}/channels/{channelId}/chatThreads", thread);
        //}

        //public async Task<Channel> CreateTeamChannelAsync(string teamId, Channel channel)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/teams/{teamId}/channels", channel);
        //    return JsonConvert.DeserializeObject<Channel>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task AddAppToTeam(string teamId, TeamsApp app)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/teams/{teamId}/apps", app);
        //}

        //public async Task<Site> GetSharePointSiteAsync(string sitePath)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/sites/{sitePath}");
        //    return JsonConvert.DeserializeObject<Site>(await response.Content.ReadAsStringAsync());
        //}



        //public async Task<Plan> CreatePlanAsync(Plan plan)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/planner/plans", plan);
        //    return JsonConvert.DeserializeObject<Plan>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<Bucket> CreateBucketAsync(Bucket bucket)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/planner/buckets", bucket);
        //    return JsonConvert.DeserializeObject<Bucket>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<PlannerTask> CreatePlannerTaskAsync(PlannerTask task)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/planner/tasks", task);
        //    return JsonConvert.DeserializeObject<PlannerTask>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<Site> GetTeamSiteAsync(string teamId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/groups/{teamId}/sites/root");
        //    return JsonConvert.DeserializeObject<Site>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<SharePointList> CreateSharePointListAsync(string siteId, SharePointList list)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/sites/{siteId}/lists", list);
        //    return JsonConvert.DeserializeObject<SharePointList>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<GraphCollection<Group>> FindGroupsBySharePointItemIdAsync(int itemId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/groups?$filter={Group.SchemaExtensionName}/sharePointItemId  eq {itemId}");
        //    return JsonConvert.DeserializeObject<GraphCollection<Group>>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task ArchiveTeamAsync(string teamId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/teams/{teamId}/archive");
        //}

        //public async Task<GraphCollection<User>> GetGroupMembersAsync(string groupId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/groups/{groupId}/members");
        //    return JsonConvert.DeserializeObject<GraphCollection<User>>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task SendNotification(Notification notification)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, "/me/notifications", notification);
        //}

        //public async Task AddTeamChannelTab(string teamId, string channelId, TeamsChannelTab tab)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/teams/{teamId}/channels/{channelId}/tabs", tab,
        //        version: string.IsNullOrEmpty(teamsEndpoint) ? "beta" : teamsEndpoint);
        //}

        //public async Task<GraphCollection<SharePointList>> GetSiteListsAsync(string siteId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/sites/{siteId}/lists");
        //    return JsonConvert.DeserializeObject<GraphCollection<SharePointList>>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task<SharePointPage> CreateSharePointPageAsync(string siteId, SharePointPage page)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/sites/{siteId}/pages", page,
        //        version: string.IsNullOrEmpty(sharePointEndpoint) ? "beta" : sharePointEndpoint);
        //    return JsonConvert.DeserializeObject<SharePointPage>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task PublishSharePointPageAsync(string siteId, string pageId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Post, $"/sites/{siteId}/pages/{pageId}/publish");
        //}

        //public async Task<GraphCollection<Group>> GetAllGroupsAsync(string filter = null)
        //{
        //    string query = string.IsNullOrEmpty(filter) ? string.Empty : $"?$filter={filter}";
        //    var response = await MakeGraphCall(HttpMethod.Get, $"/groups{query}");
        //    return JsonConvert.DeserializeObject<GraphCollection<Group>>(await response.Content.ReadAsStringAsync());
        //}

        //public async Task DeleteGroupAsync(string groupId)
        //{
        //    var response = await MakeGraphCall(HttpMethod.Delete, $"/groups/{groupId}");
        //}

        private async Task<HttpResponseMessage> MakeGraphCall(HttpMethod method, string uri, object body = null, int retries = 0, string version = "")
        {
            version = graphVersion;
            // Initialize retry delay to 3 secs
            int retryDelay = 3;

            string payload = string.Empty;

            if (body != null && (method != HttpMethod.Get || method != HttpMethod.Delete))
            {
                // Serialize the body
                payload = JsonConvert.SerializeObject(body, jsonSettings);
            }

            System.Diagnostics.Debug.WriteLine("Graph Request URL :" + graphEndpoint + version + uri + "");

            //if (logger != null)
            //{
            //    logger.Info($"MakeGraphCall Request: {method} {uri}");
            //    logger.Info($"MakeGraphCall Payload: {payload}");
            //}

            do
            {
                // Create the request
                var request = new HttpRequestMessage(method, $"{graphEndpoint}{version}{uri}");


                if (!string.IsNullOrEmpty(payload))
                {
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                }

                // Send the request
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    //if (logger != null)
                    //    logger.Info($"MakeGraphCall Error: {response.StatusCode}");
                    if (retries > 0)
                    {
                        //if (logger != null)
                        //    logger.Info($"MakeGraphCall Retrying after {retryDelay} seconds...({retries} retries remaining)");
                        //TODO? Thread.Sleep(retryDelay * 1000);
                        // Double the retry delay for subsequent retries
                        retryDelay += retryDelay;
                    }
                    else
                    {
                        // No more retries, throw error
                        var error = await response.Content.ReadAsStringAsync();
                        throw new Exception(error);
                    }
                }
                else
                {
                    return response;
                }
            }
            while (retries-- > 0);

            return null;
        }
    }
}
