using Discord.WebSocket;
using MediatR;

namespace Lyrica.Services.Core.Messages 
{ 
    public class UserVoiceStateNotification : INotification 
    { 
        public SocketUser User { get; } 
 
        public SocketVoiceState? Old { get; } 
 
        public SocketVoiceState New { get; } 
 
        public UserVoiceStateNotification(SocketUser user, SocketVoiceState? old, SocketVoiceState @new) 
        { 
            User = user; 
            Old = old; 
            New = @new; 
        } 
    } 
} 