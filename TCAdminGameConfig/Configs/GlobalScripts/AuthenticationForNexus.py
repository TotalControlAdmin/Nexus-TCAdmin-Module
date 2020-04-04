import clr;
import System;
from System import Guid;
myguid = Guid.NewGuid();

Script.WriteToConsole("[WARNING] This token is one-use. Once used to login via Discord the token will be disposed of.");
Script.WriteToConsole("Token: " + myguid.ToString());

ThisUser.CustomFields["__Nexus:DiscordToken"] = myguid.ToString();
ThisUser.CustomFields["__Nexus:DiscordUserId"] = None
ThisUser.Save();