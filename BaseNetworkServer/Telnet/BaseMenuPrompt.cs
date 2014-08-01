using System;
using System.Collections.Generic;
using System.Linq;

namespace Alienseed.BaseNetworkServer.Network.Telnet.Prompts
{
    public abstract class BaseMenuPrompt : BaseTextPrompt
    {
        public abstract string MenuName { get; }

        protected delegate BaseTextPrompt MenuResponseHandler(BaseTextPrompt sender, BaseTelNetState client, string args = null);

        protected struct MenuOption
        {
            public MenuOption(bool admin) : this()
            {
                AdminOnly = admin;
            }

            public string Command;
            public string Description;
            public MenuResponseHandler Result;
            public bool AdminOnly;
        }

        protected readonly bool AdminOnly;
        MenuOption[] _options;
        protected IEnumerable<MenuOption> Options { get { return AdminOnly ? _options : _options.Where(m => !m.AdminOnly); } }

        protected BaseMenuPrompt(MenuOption[] options, bool foradmin = false)
        {
            AdminOnly = foradmin;
            _options = options;
        }

        protected override string[] QuestionText { get { return Menu; } }

        private string[] menu = null;
        string[] Menu { get { return menu ?? (menu = BuildMenu()); } }

        string[] BuildMenu()
        {
            string[] menu = new string[Options.Count() + 2];

            int marginsize = Math.Max(10, MenuName.Length) + 2;

            string margin = new string(' ', marginsize);

            menu[0] = string.Format("{0,-11} Command  \tDescription", string.Format("{0}:", MenuName));
            menu[1] = string.Format("{0}----------\t-------------------", margin);

            int i = 2;
            foreach(var option in Options)
                menu[i++] = string.Format("{0}{1,-10:10}\t{2}", margin, option.Command.ToUpper(), option.Description);

            return menu;
        }


        public override BaseTextPrompt OnResponse(BaseTelNetState client, string text, bool cancel)
        {
            MenuOption choice = Options.Where(m => m.Command.Equals(text, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (choice.Result != null)
                return choice.Result(this, client, text);
            else
                return this;
        }
            
    }
}
