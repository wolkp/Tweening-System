using Moq;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class TweenTests
{
    private Mock<ITransform> _mockTransform;
    private Mock<IColorable> _mockColorable;
    private Mock<IRemoteTweenService> _mockRemoteTweenService;

    private MoveTween _moveTween;
    private ScaleTween _scaleTween;
    private ColorTween _colorTween;

    [SetUp]
    public void SetUp()
    {
        _mockTransform = new Mock<ITransform>();
        _mockTransform.SetupProperty(t => t.Position, Vector3.zero);
        _mockTransform.SetupProperty(t => t.Scale, Vector3.one);

        _mockColorable = new Mock<IColorable>();
        _mockColorable.SetupProperty(r => r.Color, Color.white);

        _moveTween = new MoveTween(_mockTransform.Object, Vector3.zero, new Vector3(10, 0, 0), 2f, EasingFunctions.Linear, null);
        _scaleTween = new ScaleTween(_mockTransform.Object, Vector3.one, new Vector3(2, 2, 2), 2f, EasingFunctions.Linear, null);
        _colorTween = new ColorTween(_mockColorable.Object, Color.white, Color.red, 2f, EasingFunctions.Linear, null);

        _mockRemoteTweenService = new Mock<IRemoteTweenService>();
    }

    [TearDown]
    public void TearDown()
    {
        TweenManager.Clear();
    }

    [Test]
    public void MoveTween_ProgressesOverTime()
    {
        _moveTween.Update(1f); // 50% progress (1s out of 2s)

        Vector3 expectedPosition = new Vector3(5, 0, 0);
        Assert.AreEqual(expectedPosition, _mockTransform.Object.Position);
    }

    [Test]
    public void ScaleTween_ProgressesOverTime()
    {
        _scaleTween.Update(1f); // 50% progress (1s out of 2s)

        Vector3 expectedScale = new Vector3(1.5f, 1.5f, 1.5f);
        Assert.AreEqual(expectedScale, _mockTransform.Object.Scale);
    }

    [Test]
    public void ColorTween_ProgressesOverTime()
    {
        _colorTween.Update(1f); // 50% progress (1s out of 2s)

        Color expectedColor = Color.Lerp(Color.white, Color.red, 0.5f); // Should be halfway between white and red
        Assert.AreEqual(expectedColor, _mockColorable.Object.Color);
    }

    [Test]
    public void Tween_CompletesProperly()
    {
        bool wasCompleted = false;
        _moveTween = new MoveTween(_mockTransform.Object, Vector3.zero, new Vector3(10, 0, 0), 2f, EasingFunctions.Linear, (_) => wasCompleted = true);

        _moveTween.Update(2f); // Fully complete
        Assert.IsTrue(wasCompleted);
        Assert.AreEqual(new Vector3(10, 0, 0), _mockTransform.Object.Position);
    }

    [Test]
    public void Tween_PausesAndResumesCorrectly()
    {
        _moveTween.Update(1f); // 50% progress
        _moveTween.Pause();
        _moveTween.Update(1f); // This update should do nothing

        Assert.AreEqual(new Vector3(5, 0, 0), _mockTransform.Object.Position);

        _moveTween.Resume();
        _moveTween.Update(1f); // Resume from 50%

        Assert.AreEqual(new Vector3(10, 0, 0), _mockTransform.Object.Position);
    }

    [Test]
    public void Tween_CancelsProperly()
    {
        _moveTween.Update(1f); // 50% progress
        _moveTween.Cancel();
        _moveTween.Update(1f); // Should not move anymore

        Assert.AreEqual(new Vector3(10, 0, 0), _mockTransform.Object.Position); // Instantly reaches end
    }

    [Test]
    public void Tween_JumpsToTimeCorrectly()
    {
        _moveTween.JumpToTime(1f); // Jump to 50%
        Assert.AreEqual(new Vector3(5, 0, 0), _mockTransform.Object.Position);

        _moveTween.JumpToTime(2f); // Jump to end
        Assert.AreEqual(new Vector3(10, 0, 0), _mockTransform.Object.Position);
    }

    [Test]
    public void EasingFunction_AppliesCorrectly()
    {
        _moveTween = new MoveTween(_mockTransform.Object, Vector3.zero, new Vector3(10, 0, 0), 2f, EasingFunctions.EaseInQuad, null);
        _moveTween.Update(1f); // 50% progress in time

        Vector3 expectedEasedPosition = new Vector3(2.5f, 0, 0); // Quadratic ease-in
        Assert.AreEqual(expectedEasedPosition, _mockTransform.Object.Position);
    }

    [Test]
    public void Tween_ChainsCorrectly()
    {
        bool firstTweenCompleted = false;
        bool secondTweenCompleted = false;

        var firstTween = new MoveTween(_mockTransform.Object, Vector3.zero, new Vector3(10, 0, 0), 1f, EasingFunctions.Linear, (_) => firstTweenCompleted = true);
        var secondTween = new ScaleTween(_mockTransform.Object, Vector3.one, new Vector3(2, 2, 2), 1f, EasingFunctions.Linear, (_) => secondTweenCompleted = true);

        firstTween.Chain(secondTween);
        firstTween.Start();

        // Update first tween to completion
        firstTween.Update(1f);
        Assert.IsTrue(firstTweenCompleted);
        Assert.AreEqual(new Vector3(10, 0, 0), _mockTransform.Object.Position);

        // Update second tween to completion
        secondTween.Update(1f);
        Assert.IsTrue(secondTweenCompleted);
        Assert.AreEqual(new Vector3(2, 2, 2), _mockTransform.Object.Scale);
    }


    [UnityTest]
    public IEnumerator RemoteTweenSync_HandlesOutOfOrderUpdates()
    {
        // Arrange
        var remoteTweenSync = new RemoteTweenSync(_mockTransform.Object, _mockRemoteTweenService.Object, 0.1f);
        var startPosition = new Vector3(0, 0, 0);
        var endPosition = new Vector3(10, 0, 0);

        // Simulate out-of-order updates
        var lateUpdate = new RemoteTweenData { Position = new Vector3(5, 0, 0), Time = 1f };   // Out-of-order update (will be processed second)
        var earlyUpdate = new RemoteTweenData { Position = new Vector3(10, 0, 0), Time = 2f };  // First in terms of time

        // Call Listen method to simulate remote updates being received
        remoteTweenSync.OnServerUpdate(lateUpdate);
        remoteTweenSync.OnServerUpdate(earlyUpdate);

        // Now update asynchronously
        var updateAsyncTask = remoteTweenSync.UpdateAsync();
        yield return new WaitUntil(() => updateAsyncTask.IsCompleted);

        // Assert: The final position should be the one from the second update (after the Lerp interpolation)
        // Depending on the smoothing factor, you might want to check a range around the expected value.
        Assert.AreEqual(new Vector3(10, 0, 0), _mockTransform.Object.Position);
    }

    [UnityTest]
    public IEnumerator RemoteTweenSync_HandlesDelayedServerUpdates()
    {
        // Arrange
        var remoteTweenSync = new RemoteTweenSync(_mockTransform.Object, _mockRemoteTweenService.Object, 0.1f);

        // Simulate initial update
        var firstUpdate = new RemoteTweenData { Position = new Vector3(0, 0, 0), Time = 0f };
        remoteTweenSync.OnServerUpdate(firstUpdate);

        // Simulate a few frames of delay before next update
        for(var i = 0; i < 30; i++)
        {
            yield return null;
        }

        // Now simulate a second update with a delay
        var secondUpdate = new RemoteTweenData { Position = new Vector3(5, 0, 0), Time = 1f };
        remoteTweenSync.OnServerUpdate(secondUpdate);

        // Wait for the asynchronous update to complete
        var updateAsyncTask = remoteTweenSync.UpdateAsync();
        yield return new WaitUntil(() => updateAsyncTask.IsCompleted);

        // Assert: The object should have moved smoothly towards the second update
        Assert.AreEqual(new Vector3(5, 0, 0), _mockTransform.Object.Position);
    }
}