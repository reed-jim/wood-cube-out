using System;

public interface ISaferioUIAnimation
{
    public void Show();
    public void Hide();
    public void Hide(Action onCompletedAction);
}