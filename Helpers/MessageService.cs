using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EXOApp.Helpers
{
    internal class MessageService : MessageInterface
    {
        public async Task<bool> ShowAsyncAcceptCancel(string title, string message, string accept = "OK", string cancel = "Cancel")
        {
            return await App.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        public async Task ShowAsyncOK(string title, string message, string accept = "OK")
        {
            await App.Current.MainPage.DisplayAlert(title, message, accept);
        }
    }
}
