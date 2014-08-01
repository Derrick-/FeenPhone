
namespace Alienseed.BaseNetworkServer.Accounting
{
    public interface IAccountRepository
    {
        IUserClient Login(string username, string password);
    }

    public interface IUserClient : IUser
    {
        IClient Client { get; }
        bool SetClient(IClient client);
    }

    public interface IClient
    {
        IUserClient User { get; }

        bool Connected { get; }

        //void OnGameTick(IGameInternal game);
        //void OnTableStatusChange(IGameInternal game, string newstatus);
        //void OnSit(IGameInternal game, ITableInternal table, ISeatInternal seat, IUser user);
        //void OnLeave(IGameInternal game, ITableInternal table, ISeatInternal seat, IUser user);
        //void OnBet(IGameInternal game, ITableInternal table, ISeatInternal seat, int bet);
        //void OnFold(IGameInternal game, ITableInternal table, ISeatInternal seat);

        //void OnReset(IGame game);

        //void OnBetPrompt(IGameInternal game, ITableInternal table, ISeatInternal seat, BetRequest prompt);
        //void OnDealtCard(IGameInternal game, ISeatInternal seat, ICard card);
        //void OnGameSequenceAdvance(IGameInternal game);
        //void OnCheck(ISeatInternal seat);
        //void OnShowCards(IGameInternal game, ISeatInternal seat, ICardSet cards);
        //void OnWinners(IGameInternal game, IEnumerable<ISeatInternal> seats);
        //void OnPotRefundedTo(IGameInternal game, IBettingSeat seat, int amount);
        //void OnPotPaidTo(IGameInternal game, IBettingSeat seat, int amount);
        //void OnHandChanged(IGameInternal game, ISeatInternal seat);
    }


}
