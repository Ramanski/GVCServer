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
            Error
    }

    public class NotificationService
    {
        public string Message { get; private set; }

        public TypeNotification MessageType { get; private set; }

        public event Action OnChange;

        public void SetMessage(TypeNotification typeNotification, string message)
        {
            Message = message;
            MessageType = typeNotification;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
