using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Security.Claims;
using System.Linq;
using Autofac;
using System.Web;
using System.Collections.Generic;
using System;

namespace MultiBotPoc
{
    [BotAuthentication(CredentialProviderType = typeof(MultiCredentialProvider))]
    public class MessagesController : ApiController
    {

        static MessagesController()
        {
            // Update the container to use the right MicorosftAppCredentials based on
            // Identity set by BotAuthentication
            var builder = new ContainerBuilder();

            builder.Register(c => Class1.GetCredentialsFromClaims(((ClaimsIdentity)HttpContext.Current.User.Identity)))
                .AsSelf()
                .InstancePerLifetimeScope();
            builder.Update(Conversation.Container);
        }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient client = new ConnectorClient(new Uri(activity.ServiceUrl));
                var reply = activity.CreateReply( "TEST");
                await client.Conversations.ReplyToActivityAsync(reply);
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }

    public static class Class1
    {
        public const string AppPasswordClaim = "appPassword";

        public static string GetAppIdFromClaims(this ClaimsIdentity identity)
        {
            if (identity == null)
                return null;

            Claim botClaim = identity.Claims.FirstOrDefault(c => c.Type == "appid" || c.Type == "azp");
            if (botClaim != null)
                return botClaim.Value;

            botClaim = identity.Claims.FirstOrDefault(c => c.Issuer == "https://api.botframework.com" && c.Type == "aud");
            if (botClaim != null)
                return botClaim.Value;

            return null;
        }

        public static string GetAppPasswordFromClaims(this ClaimsIdentity identity)
        {
            return identity?.Claims.FirstOrDefault(c => c.Type == AppPasswordClaim)?.Value;
        }

        public static MicrosoftAppCredentials GetCredentialsFromClaims(this ClaimsIdentity claimsIdentity)
        {
            var appId = claimsIdentity.GetAppIdFromClaims();
            var password = claimsIdentity.GetAppPasswordFromClaims();
            return new MicrosoftAppCredentials(appId, password);
        }
    }

    public class MultiCredentialProvider : ICredentialProvider
    {
        public Dictionary<string, string> Credentials = new Dictionary<string, string>
        {
            { "4a8d4b4b-275a-4c55-bf81-b503b807bf72", "wndxfSTO6406~:ydMSXD8)=" },
            { "e1199c57-0bdc-430a-ab15-dcab79d63593", "tbYGDRL79!=uaabpFL904!$" }
        };

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(this.Credentials.ContainsKey(appId));
        }

        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult(this.Credentials.ContainsKey(appId) ? this.Credentials[appId] : null);
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(!this.Credentials.Any());
        }
    }
}