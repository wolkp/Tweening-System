using System;

public interface IRemoteTweenService
{
    void ListenToRemoteTween(string remoteObjectID, Action<RemoteTweenData> onServerUpdate);
}