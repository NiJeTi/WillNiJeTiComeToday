namespace TelegramBot.Commands
{
    internal interface ICommand
    {
        public string Response { get; }

        public void Prepare();

        public void Handle(object query);
    }
}