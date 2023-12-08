using System.Media;

#pragma warning disable CA1416

namespace PhoneFinder.Services;

internal class SoundService : ISoundService
{
    private readonly SoundPlayer _messagePlayer;
    private readonly SoundPlayer _errorPlayer;
    private readonly SoundPlayer _warningPlayer;

    public SoundService()
    {
        _messagePlayer = new SoundPlayer("Resources/message.wav");
        _errorPlayer = new SoundPlayer("Resources/error.wav");
        _warningPlayer = new SoundPlayer("Resources/warning.wav");
    }

    public void PlayGoalPhoneNumberFound()
    {
        _messagePlayer.Play();
    }

    public void PlayError()
    {
        _errorPlayer.Play();
    }

    public void PlayWarning()
    {
        _warningPlayer.Play();
    }
}
