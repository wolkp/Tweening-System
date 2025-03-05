using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RemoteTweenSync
{
    private readonly Queue<RemoteTweenData> _updates = new();
    private readonly List<ITween> _activeTweens = new();
    private readonly ITransform _target;
    private readonly IRemoteTweenService _remoteTweenService;
    private readonly float _smoothingFactor;

    private TaskCompletionSource<bool> _taskCompletionSource;
    private float _lastServerUpdateTime;
    private bool _isUpdating = false;
    private int _tweensInProgress = 0;

    public RemoteTweenSync(ITransform target, IRemoteTweenService remoteTweenService, float smoothingFactor = 0.1f)
    {
        this._target = target ?? throw new ArgumentNullException(nameof(target));
        this._remoteTweenService = remoteTweenService ?? throw new ArgumentNullException(nameof(remoteTweenService));
        this._smoothingFactor = smoothingFactor;
    }

    public void Listen(string remoteObjectID)
    {
        _remoteTweenService.ListenToRemoteTween(remoteObjectID, OnServerUpdate);
    }

    public async Task UpdateAsync()
    {
        if (_isUpdating)
            return;

        _isUpdating = true;

        _taskCompletionSource = new TaskCompletionSource<bool>();

        while (_updates.Count > 0)
        {
            RemoteTweenData prevData = _updates.Dequeue();
            RemoteTweenData nextData = _updates.Count > 0 ? _updates.Peek() : prevData;

            if (prevData.Position == nextData.Position)
                continue;

            RemoteMoveTween tween = new RemoteMoveTween(
                _target,
                prevData.Position,
                nextData.Position,
                nextData.Time - prevData.Time,
                EasingFunctions.Linear,
                OnTweenComplete,
                _smoothingFactor
            );

            _activeTweens.Add(tween);
            _tweensInProgress++;

            await Task.Yield();
        }

        await _taskCompletionSource.Task;

        _isUpdating = false;
    }

    private void OnTweenComplete(ITween tween)
    {
        _tweensInProgress--;

        if (_tweensInProgress == 0)
        {
            _taskCompletionSource.SetResult(true);
        }
    }

    public void OnServerUpdate(RemoteTweenData data)
    {
        if (UpdateIsOutdated(data))
            return;

        _lastServerUpdateTime = data.Time;
        _updates.Enqueue(data);
    }

    private bool UpdateIsOutdated(RemoteTweenData data)
    {
        return data.Time < _lastServerUpdateTime;
    }
}