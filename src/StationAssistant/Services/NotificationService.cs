using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Services
{  
    public enum TypeNotification
    {
            Info,
            Success,
            Warning,
            Error, 
            SignalR
    }

    public class NotificationService
    {
        public string Message { get; private set; }

        public string Sender {get; private set; }

        public TypeNotification MessageType { get; private set; }

        public event Action OnChange;

        public void SetMessage(TypeNotification typeNotification, string message)
        {
            Message = message;
            MessageType = typeNotification;
            NotifyStateChanged();
        }

        public void SetMessage(string sender, string message)
        {
            Sender = sender;
            Message = message;
            MessageType = TypeNotification.SignalR;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
