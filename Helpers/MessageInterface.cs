using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EXOApp.Helpers
{
    public interface MessageInterface
    {
        Task<bool> ShowAsyncAcceptCancel(string title, string message, string accept, string cancel);
        Task ShowAsyncOK(string title, string message, string accept);
    }
}
