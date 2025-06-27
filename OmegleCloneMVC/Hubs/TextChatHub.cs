using Microsoft.AspNetCore.SignalR;

namespace OmegleCloneMVC.Hubs
{
    public class TextChatHub : Hub
    {
        private static Dictionary<string, string> UserInterests = new();
        private static List<string> Waiting = new();
        private static Dictionary<string, string> Pairs = new();

        public override async Task OnConnectedAsync()
        {
            var interest = Context.GetHttpContext()?.Request.Query["interest"].ToString()?.ToLower() ?? "";

            UserInterests[Context.ConnectionId] = interest;

            string partner = null;

            lock (Waiting)
            {
                var match = Waiting.FirstOrDefault(w =>
                    UserInterests.ContainsKey(w) &&
                    UserInterests[w] == interest &&
                    w != Context.ConnectionId);

                if (match != null)
                {
                    partner = match;
                    Waiting.Remove(match);
                }
                else if (Waiting.Count > 0)
                {
                    partner = Waiting[0]; // fallback
                    Waiting.RemoveAt(0);
                }
                else
                {
                    Waiting.Add(Context.ConnectionId);
                }

                if (partner != null)
                {
                    Pairs[Context.ConnectionId] = partner;
                    Pairs[partner] = Context.ConnectionId;
                }
            }

            if (partner != null)
            {
                await Clients.Client(partner).SendAsync("ReceiveMessage", "✅ Partner povezan.");
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "✅ Partner povezan.");
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string msg)
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("ReceiveMessage", msg);
            }
        }

        public async Task SendTyping()
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("ReceiveTyping");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("PartnerDisconnected");
                Pairs.Remove(partner);
                Pairs.Remove(Context.ConnectionId);
            }
            else
            {
                Waiting.Remove(Context.ConnectionId);
            }

            UserInterests.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
