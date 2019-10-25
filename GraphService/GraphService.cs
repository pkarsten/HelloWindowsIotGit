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
using System.IO;
using System.Collections.ObjectModel;

namespace MSGraph
{
    public class GraphService
    {
        //Hello Graph
        private static readonly string graphEndpoint = "https://graph.microsoft.com/";
        private static readonly string graphVersion = "beta/me";//"v1.0/me"; //beta
        

        private readonly string accessToken = string.Empty;
        private HttpClient httpClient = null;
        private readonly JsonSerializerSettings jsonSettings =
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        // Hot examples:
        //https://csharp.hotexamples.com/examples/-/Microsoft.Graph/-/php-microsoft.graph-class-examples.html

        //This sample app implements Azure functions designed to be invoked via Microsoft Flow to provision a Microsoft Team when a new flight is added to a master list in SharePoint.The sample uses Microsoft Graph to do the following provisioning tasks:
        //https://github.com/microsoftgraph/contoso-airlines-azure-functions-sample/tree/master/create-flight-team
        private static readonly string teamsEndpoint = Environment.GetEnvironmentVariable("TeamsEndpoint");
        private static readonly string sharePointEndpoint = Environment.GetEnvironmentVariable("SharePointEndPoint");

        //TODO: Remove my ID Here 
        // Below are the clientId (Application Id) of your app registration and the tenant information. 
        // You have to replace:
        // - the content of ClientID with the Application Id for your app registration
        // - Te content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for any Work or School accounts, or Microsoft personal account, use common
        //   - for Microsoft Personal account, use consumers

        private static string ClientId = "cba8344a-8cbb-41be-a414-4da940902ad7";//"0b8b0665-bc13-4fdc-bd72-e0227b9fc011";
        private static string Tenant = "common";
        public static PublicClientApplication PublicClientApp { get; } = new PublicClientApplication(ClientId, $"https://login.microsoftonline.com/{Tenant}");
        public static string TokenForUser = null;
        public static DateTimeOffset Expiration;
        public static string[] Scopes = { "user.read", "Files.Read", "Calendars.Read","Tasks.Read" };

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
            //e.g. https://graph.microsoft.com/beta/me/drive/root:/Bilder/WindowsIoTApp   path="/Bilder/WindowsIotApp"
            var response = await MakeGraphCall(HttpMethod.Get, $"/drive/root:{path}");
            var sf = JsonConvert.DeserializeObject<ItemInfoResponse>(await response.Content.ReadAsStringAsync());
            return sf;
        }

        public async Task<IList<ItemInfoResponse>> PopulateChildren(ItemInfoResponse info)
        {
            try
            {
                List<ItemInfoResponse> myList = new List<ItemInfoResponse>();
                string nextLink =null;
                var response = await MakeGraphCall(HttpMethod.Get, $"/drive/items/{info.Id}/children?select=id,image,name"); //get only the id's add "?select=id" at end  
                var l = JsonConvert.DeserializeObject<ParseChildrenResponse>(await response.Content.ReadAsStringAsync());
                nextLink = l.NextLink;
                myList.AddRange(l.Value);

                if (nextLink != null)
                {
                    do
                    {
                        HttpResponseMessage rm = await MakeGraphCall(HttpMethod.Get, "", null, 0, nextLink); //get only the id's add "?select=id" at end  
                        ParseChildrenResponse cr = JsonConvert.DeserializeObject<ParseChildrenResponse>(await rm.Content.ReadAsStringAsync());
                        if (cr != null)
                        {
                            myList.AddRange(cr.Value);
                            nextLink = cr.NextLink;
                        }
                    }
                    while (nextLink != null);
                }

                return myList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in PopulateChildren" + ex.Message);
                return null;
            }
        }

        public async Task<string> GetCalendarViewTest()
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

        public async Task<IList<CalendarEventItem>> GetCalendarEvents(int nextXDays)
        {
            //https://graph.microsoft.com/v1.0/me/calendarView?startdatetime=2019-02-12&enddatetime=2019-02-19&$select=subject,Start,End
            try
            {
                DateTime startDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                DateTime endDT = startDT.AddDays(nextXDays);
                string strStarDT = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", startDT);
                string strEndDT = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", endDT);
                System.Diagnostics.Debug.WriteLine("Local Time Start: " + strStarDT + " End " + strEndDT);

                var utcstartDT = startDT.ToUniversalTime();
                var utcendDT = endDT.ToUniversalTime();
                string strutcStart = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", utcstartDT);
                string strUtcEnd = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", utcendDT);
                System.Diagnostics.Debug.WriteLine("UTC Start Time: " + strutcStart + " End " + strUtcEnd);

                List<CalendarEventItem> myList = new List<CalendarEventItem>();
                string nextLink = null;
                var response = await MakeGraphCall(HttpMethod.Get, $"/calendarView?startdatetime={strutcStart}.000Z&enddatetime={strUtcEnd}.000Z&select=subject,start,end,isallday");
                var calendarevents = JsonConvert.DeserializeObject<ParseCalendarEventResponse>(await response.Content.ReadAsStringAsync());
                nextLink = calendarevents.NextLink;
                myList.AddRange(calendarevents.Value);
                System.Diagnostics.Debug.WriteLine("NEXT LINK : " + nextLink);
                if (nextLink != null)
                {
                    do
                    {
                        HttpResponseMessage rm = await MakeGraphCall(HttpMethod.Get, "", null, 0, nextLink);
                        ParseCalendarEventResponse cr = JsonConvert.DeserializeObject<ParseCalendarEventResponse>(await rm.Content.ReadAsStringAsync());
                        if (cr != null)
                        {
                            myList.AddRange(cr.Value);
                            nextLink = cr.NextLink;
                        }
                    }
                    while (nextLink != null);
                }

                return myList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetCalendarEvents" + ex.Message);
                return null;
            }
        }

        //public async Task<IList<CalendarEventItem>> GetTodayCalendarEvents()
        //{
        //    //https://graph.microsoft.com/v1.0/me/calendarView?startdatetime=2019-02-12&enddatetime=2019-02-19&$select=subject,Start,End
        //    // https://graph.microsoft.com/v1.0/me/calendarview?startdatetime=2019-04-08T06:00:00.014Z&enddatetime=2019-04-08T23:30:00.014Z&select=subject,start,end,isallday
        //    try
        //    {

        //        DateTime startDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0,0,0);
        //        DateTime endDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(1).Day, 0, 0, 0);
        //        string strStarDT = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", startDT);
        //        string strEndDT= String.Format("{0:yyyy-MM-ddTHH:mm:ss}", endDT);
        //        System.Diagnostics.Debug.WriteLine("Local Time Start: " + strStarDT + " End " + strEndDT);

        //        var utcstartDT = startDT;//.ToUniversalTime();
        //        var utcendDT = endDT.ToUniversalTime();
        //        string strutcStart = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", utcstartDT);
        //        string strUtcEnd = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", utcendDT);
        //        System.Diagnostics.Debug.WriteLine("UTC Start Time: " + strutcStart+ " End " + strUtcEnd);

                
        //        var response = await MakeGraphCall(HttpMethod.Get, $"/calendarView?startdatetime={strutcStart}&enddatetime={strUtcEnd}&select=subject,start,end,isallday");
        //        var calendarevents = JsonConvert.DeserializeObject<ParseCalendarEventResponse>(await response.Content.ReadAsStringAsync());
        //        return calendarevents.Value;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Error in GetCalendarEvents" + ex.Message);
        //        return null;
        //    }
        //}

        public async Task<DriveItem> GetOneDriveItemAsync(string itemId)
        {
            //https://docs.microsoft.com/en-us/graph/api/driveitem-get?view=graph-rest-1.0
            // /me/drive/items/{item-id}
            var response = await MakeGraphCall(HttpMethod.Get, $"/drive/items/{itemId}");
            return JsonConvert.DeserializeObject<DriveItem>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ItemInfoResponse> GetItem(string itemId)
        {
            //https://docs.microsoft.com/en-us/graph/api/driveitem-get?view=graph-rest-1.0
            // /me/drive/items/{item-id}
            //get Item graph Uri : https://api.onedrive.com/v1.0/drive/root:/Bilder/Karneval/IMG_4463.JPG
            var response = await MakeGraphCall(HttpMethod.Get, $"/drive/items/{itemId}");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ItemInfoResponse>(json);
        }

        public async Task<Stream> RefreshAndDownloadContent(ItemInfoResponse model,bool refreshFirst)
        {
            //var response = await MakeGraphCall(HttpMethod.Get, $"/drive/items/{model.DownloadUrl}");
            //var stream = await response.Content.ReadAsStreamAsync();
            //return stream;
            System.Diagnostics.Debug.WriteLine($"RefreshAndDownloadContent: {model.DownloadUrl}");
            var response = await httpClient.GetAsync(model.DownloadUrl);
            var stream = await response.Content.ReadAsStreamAsync();
            return stream;
        }

        #region Outlook Tasks ToDo

        public async Task<IList<TaskFolder>> GeTaskFolders()
        {
            // Doku https://docs.microsoft.com/en-us/graph/api/outlooktaskfolder-list-tasks?view=graph-rest-beta&tabs=http
            //Task Folders: https://graph.microsoft.com/beta/me/outlook/taskFolders
            try
            {
                var response = await MakeGraphCall(HttpMethod.Get, $"/outlook/taskFolders");
                var taskfolders = JsonConvert.DeserializeObject<ParseTaskFolderResponse>(await response.Content.ReadAsStringAsync());
                return taskfolders.Value;


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR while Get Task Folders " + ex.Message);
                return null;
            }
        }

        public async Task<IList<TaskResponse>> GetTasksFromToDoTaskList(string taskfolderId)
        {
            //Task Folders: https://graph.microsoft.com/beta/me/outlook/taskFolders
            //List Tasks in Task (ToDO) List: https://graph.microsoft.com/beta/me/outlook/taskFolders/AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoALgAAA9AbFx3CcYdHmhKEe93jcbkBAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAAA=/tasks
            //Filtered status = nonStarted
            // https://graph.microsoft.com/beta/me/outlook/taskFolders/AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoALgAAA9AbFx3CcYdHmhKEe93jcbkBAEzk4EU4PLJIn8ZZnZVUnYgAAplWC_4AAAA=/tasks?$filter=status eq 'notStarted'
            //Get TasksCOntent from One Task e.g. (Einkaufen) https://graph.microsoft.com/beta/me/outlook/tasks('AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoARgAAA9AbFx3CcYdHmhKEe93jcbkHAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAABM5OBFODyySJ-GWZ2VVJ2IAAGzZf5vAAAA')

            try
            {
                var response = await MakeGraphCall(HttpMethod.Get, $"/outlook/taskFolders/{taskfolderId}/tasks?$filter=status eq 'notStarted'");
                var tasks = JsonConvert.DeserializeObject<ParseTaskResponse>(await response.Content.ReadAsStringAsync());
                return tasks.Value;


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR while Get Tasklist in Folder " + ex.Message);
                return null;
            }
        }

        public async Task<TaskResponse> GetPurchaseTask()
        {
            //Get Task (Einkaufen) https://graph.microsoft.com/beta/me/outlook/tasks('AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoARgAAA9AbFx3CcYdHmhKEe93jcbkHAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAABM5OBFODyySJ-GWZ2VVJ2IAAGzZf5vAAAA')

            try
            {
                var response = await MakeGraphCall(HttpMethod.Get, $"/outlook/tasks('AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoARgAAA9AbFx3CcYdHmhKEe93jcbkHAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAABM5OBFODyySJ-GWZ2VVJ2IAAGzZf5vAAAA')");
                //var json = await response.Content.ReadAsStringAsync();
                var purchtask = JsonConvert.DeserializeObject<TaskResponse>(await response.Content.ReadAsStringAsync());
                return purchtask;


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR while Get Task " + ex.Message);
                return null;
            }
        }
        #endregion



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


        private async Task<HttpResponseMessage> MakeGraphCall(HttpMethod method, string uri, object body = null, int retries = 0, string nextdatalink = "")
        {
            string useversion = graphVersion;
            // Initialize retry delay to 3 secs
            int retryDelay = 3;

            string payload = string.Empty;

            if (body != null && (method != HttpMethod.Get || method != HttpMethod.Delete))
            {
                // Serialize the body
                payload = JsonConvert.SerializeObject(body, jsonSettings);
            }

            if(nextdatalink =="")
                System.Diagnostics.Debug.WriteLine("Graph Request URL :" + graphEndpoint + useversion + uri + "");
            else
                System.Diagnostics.Debug.WriteLine("Graph Request URL :" + nextdatalink + "");

            //if (logger != null)
            //{
            //    logger.Info($"MakeGraphCall Request: {method} {uri}");
            //    logger.Info($"MakeGraphCall Payload: {payload}");
            //}

            do
            {
                // Create the request
                HttpRequestMessage request;
                if (nextdatalink == "")
                    request = new HttpRequestMessage(method, $"{graphEndpoint}{useversion}{uri}");
                else
                    request = new HttpRequestMessage(method, $"{nextdatalink}");

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
