namespace Plus.Communication.Rcon.Commands.Hotel
{
    class ReloadServerSettingsCommand : IRconCommand
    {
        public string Description => "This command is used to reload the server settings.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            PlusEnvironment.GetSettingsManager().Init();
            return true;
        }
    }
}