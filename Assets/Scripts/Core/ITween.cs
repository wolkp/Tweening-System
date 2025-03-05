public interface ITween
{
    bool IsComplete { get; }
    ITween NextTween { get; set; }
    void Start();
    void Update(float deltaTime);
    void Pause();
    void Resume();
    void Cancel();
    void JumpToTime(float time);
    void Chain(params ITween[] nextTweens);
}