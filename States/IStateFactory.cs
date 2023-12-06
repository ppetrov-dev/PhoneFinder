using PhoneFinder.Domain;

namespace PhoneFinder.States;

internal interface IStateFactory
{
    IState GetOrCreateAccountSelectedState(Account account);

    IState CreateFoundGoalPhoneNumber(Account account, PhoneNumber phoneNumber);

    IState CreateNoBalanceState(Account account);

    IState CreateSelectNextAccountState(Account? currentAccount = null);
}
