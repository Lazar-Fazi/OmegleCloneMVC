using System;
using Microsoft.AspNetCore.SignalR;

namespace OmegleCloneMVC.Hubs
{
    public class ChatHub : Hub
    {
        private static int OnlineCount = 0;

        private static Dictionary<string, string> UserInterests = new();



        private static List<string> WaitingUsers = new();
        private static Dictionary<string, string> UserGenders = new();
        private static Dictionary<string, string> Pairs = new();

        public override async Task OnConnectedAsync()
        {
            var gender = Context.GetHttpContext()?.Request.Query["gender"].ToString() ?? "Nepoznat";
            var interest = Context.GetHttpContext()?.Request.Query["interest"].ToString() ?? "";

            UserGenders[Context.ConnectionId] = gender;
            UserInterests[Context.ConnectionId] = interest;


            string partner = null;
            string yourGender = null;
            string partnerGender = null;

            lock (WaitingUsers)
            {
                if (WaitingUsers.Count > 0)
                {
                    partner = WaitingUsers[0];
                    WaitingUsers.RemoveAt(0);

                    Pairs[Context.ConnectionId] = partner;
                    Pairs[partner] = Context.ConnectionId;

                    yourGender = UserGenders[Context.ConnectionId];
                    partnerGender = UserGenders[partner];
                }
                else
                {
                    WaitingUsers.Add(Context.ConnectionId);
                }
            }

            if (partner != null)
            {
                await Clients.Client(partner).SendAsync("PartnerGender", yourGender);
                await Clients.Client(Context.ConnectionId).SendAsync("PartnerGender", partnerGender);

                var yourInterest = UserInterests[Context.ConnectionId];
                var partnerInterest = UserInterests[partner];

                if (!string.IsNullOrEmpty(yourInterest) &&
                    !string.IsNullOrEmpty(partnerInterest) &&
                    yourInterest.Equals(partnerInterest, StringComparison.OrdinalIgnoreCase))
                {
                    await Clients.Client(partner).SendAsync("CommonInterest", yourInterest);
                    await Clients.Client(Context.ConnectionId).SendAsync("CommonInterest", yourInterest);
                }


                await Clients.Client(partner).SendAsync("PartnerFound");
                await Clients.Client(Context.ConnectionId).SendAsync("PartnerFound");
            }

            // ✅ Brojanje online korisnika
            Interlocked.Increment(ref OnlineCount);
            await Clients.All.SendAsync("UpdateOnlineUsers", OnlineCount);

            await base.OnConnectedAsync();
        }




        public async Task SendMessage(string message)
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("ReceiveMessage", message);
            }
        }
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("PartnerDisconnected");
                Pairs.Remove(partner);
                Pairs.Remove(Context.ConnectionId);
            }
            else
            {
                WaitingUsers.Remove(Context.ConnectionId);
            }

            Interlocked.Decrement(ref OnlineCount);
            await Clients.All.SendAsync("UpdateOnlineUsers", OnlineCount);

            UserGenders.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);

            UserInterests.Remove(Context.ConnectionId);

        }



        public async Task SendOffer(object offer)
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("ReceiveOffer", offer);
            }
        }

        public async Task SendAnswer(object answer)
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("ReceiveAnswer", answer);
            }
        }

        public async Task SendIceCandidate(object candidate)
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("ReceiveIceCandidate", candidate);
            }
        }

        public async Task SendTyping()
        {
            if (Pairs.TryGetValue(Context.ConnectionId, out var partner))
            {
                await Clients.Client(partner).SendAsync("ReceiveTyping");
            }
        }



    }

}
