using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ModelsLibrary;

namespace StationAssistant.Services
{
    public class TrainsHub : Hub
    {
        public async Task SendArrived(string user, TrainModel train)
        {
            await Clients.All.SendAsync("TrainArrived", user, train);
        }
    }
    
}