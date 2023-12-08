namespace PhoneFinder.Services;

internal interface ISoundService
{
    void PlayGoalPhoneNumberFound();
    void PlayError();
    void PlayWarning();
}
