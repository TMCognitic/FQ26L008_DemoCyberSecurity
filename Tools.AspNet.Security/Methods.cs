using Microsoft.AspNetCore.SignalR;

namespace Tools.AspNet.Security
{
    [Flags]
    public enum Methods
    {
        Get = 1,
        Post = 2,
        Put = 4,
        Patch = 8,
        Delete = 16
    }
}
