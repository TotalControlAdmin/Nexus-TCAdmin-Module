using System.Collections.Generic;

namespace TCAdminModule.Objects
{
    public class ActionCommandAttribute
    {
        public ActionCommandAttribute(string name, string description, string emojiName, List<string> permissions,
            bool deleteMenu = false)
        {
            Name = name;
            Description = description;
            Permissions = permissions;
            EmojiName = emojiName;
            DeleteMenu = deleteMenu;
        }

        public string Name { get; }

        public string Description { get; }

        public string EmojiName { get; }

        public List<string> Permissions { get; }

        public bool DeleteMenu { get; }
    }
}