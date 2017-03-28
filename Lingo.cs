using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace LingoBot
{
    class Lingo
    {
        DiscordClient discord;
        CommandService commands;

        Random rand = new Random();
        Dictionary<ulong, List<string>[]> languageRoles = new Dictionary<ulong, List<string>[]>();
        LinkedList<string> memes = new LinkedList<string>();
        bool initFlag = false; 
        string permissionError = "You don't have the permission to use that.";


        public Lingo()
        {
            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '~';
            });

            commands = discord.GetService<CommandService>();

            RegisterMemeCommand();
            RegisterAddMemeCommand();
            RegisterRemoveMemeCommand();
            RegisterInitCommand();
            RegisterKatakanaCommand();
            RegisterHiraganaCommand();
            RegisterAddLanguageCommand();
            RegisterLanguageCommand();
            RegisterRemoveLanguageCommand();
            RegisterHelpCommand();
                
            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("Mjk1NjY3NzcwODAzMzU1NjYw.C7rh0g.ESwSupsPBf13U5Fj9h4XDsUFMCk", TokenType.Bot);
            });
        }
        
        // Commands

        private void RegisterMemeCommand()
        {
            commands.CreateCommand("meme")
                .Do(async (e) =>
                {
                    if (initFlag)
                    {
                        if (memes.Count == 0)
                        {
                            await e.Channel.SendMessage("I dont have any memes to display, please use `!addmeme <meme_link>` to add a meme!");
                        }
                        else
                        {
                            int indexOfMeme = rand.Next(memes.Count);
                            string meme = memes.ElementAt(indexOfMeme);
                            await e.Channel.SendMessage(meme);
                        }
                    }
                    else
                        await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
                });
        }

        private void RegisterAddMemeCommand()
        {
            commands.CreateCommand("addmeme").Parameter("link")
                .Do(async (e) =>
                {
                    if(initFlag)
                    {
                        bool flag = false;
                        for (int i = 0; i < e.User.Roles.Count(); i++)
                        {
                            if (e.User.Roles.ElementAt(i).ToString() == "Semi-Moderator")
                            {
                                flag = true;
                            }
                        }
                        if (flag)
                            await AddMeme(e, e.GetArg("link"));
                        else
                            PrintPremissionError(e);
                        UpdateMemesList();
                    }
                    else
                        await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
                });
        }
         
        private void RegisterHiraganaCommand()
        {
            commands.CreateCommand("hiragana").Do(async (e) =>
            {
                await e.Channel.SendMessage("http://prntscr.com/ep0d0w");
            });
        }

        private void RegisterKatakanaCommand()
        {
            commands.CreateCommand("katakana").Do(async (e) =>
            {
                await e.Channel.SendMessage("http://prntscr.com/ep0etn");
            });
        }

        private void RegisterRemoveMemeCommand()
        {
            commands.CreateCommand("removememe").Parameter("link")
                .Do(async (e) =>
                {
                    if (initFlag)
                    {
                        bool flag = false;
                        for (int i = 0; i < e.User.Roles.Count(); i++)
                        {
                            if (e.User.Roles.ElementAt(i).ToString() == "Semi-Moderator")
                            {
                                flag = true;
                            }
                        }
                        if (flag)
                            if (memes.Contains(e.GetArg("link")))
                            {
                                string link = e.GetArg("link");
                                memes.Remove(link);
                                await e.Channel.SendMessage("Meme successfully removed from the meme list.");
                            }
                            else
                                await e.Channel.SendMessage("This meme is not in the meme list.");
                        else
                            PrintPremissionError(e);
                        UpdateMemesList();
                    }
                    else
                        await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
                });
        }

        private void RegisterInitCommand()
        {
            commands.CreateCommand("init")
                .Do(async (e) =>
                {
                    bool flag = false;
                    for (int i = 0; i < e.User.Roles.Count(); i++)
                    {
                        if (e.User.Roles.ElementAt(i).ToString() == "Semi-Moderator")
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                        if (initFlag == false)
                        {
                            initFlag = true;
                            await e.Channel.SendMessage("Starting init process.");
                            string line;
                            string dirpath = Directory.GetCurrentDirectory();
                            dirpath = RemoveFromEnd(dirpath, "\\bin\\Debug");
                            dirpath = Path.Combine(dirpath, "Entities\\memeLinks.txt");
                            System.IO.StreamReader file1 = new System.IO.StreamReader(dirpath);
                            line = file1.ReadLine();
                            while (line != "@")
                            { 
                                await AddMeme(e, line);
                                line = file1.ReadLine();
                            }
                            file1.Close();

                            dirpath = Directory.GetCurrentDirectory();
                            dirpath = RemoveFromEnd(dirpath, "\\bin\\Debug");
                            dirpath = Path.Combine(dirpath, "Entities\\languagesMap.txt");
                            System.IO.StreamReader file = new System.IO.StreamReader(dirpath);
                            line = file.ReadLine();
                            while (line != "@")
                            {
                                await AddLang(e, line);
                                line = file.ReadLine();
                            }
                            file.Close();


                            await e.Channel.SendMessage("Finishing initiallizing");
                            initFlag = true;
                        }
                        else
                            await e.Channel.SendMessage("You can only init once, fool.");
                    else
                        PrintPremissionError(e);
                });
        }

        private void RegisterHelpCommand()
        {
            commands.CreateCommand("help").Parameter("function", ParameterType.Optional)
                            .Do(async (e) =>
                            {
                            string myMessage = "";
                                if(e.GetArg("function") != "")
                                    await e.Channel.SendMessage("Presenting help information for the command `~" +e.GetArg("function") + "`:");
                                else
                                {
                                    await e.Channel.SendMessage("Available commands: meme, languages, addlanguage, removelanguage, hiragana, katakana\nFor more commands contact mods or @Collector");
                                    return;
                                }
                                if (e.GetArg("function") == "languages")
                                    myMessage = "Usage: Check someone's fluent, conversational and learning languages.\nExample: `~languages @Collector`";
                                else if (e.GetArg("function") == "addlanguage")
                                    myMessage = "Usage: Add to your language list a fluent, conversational or learning language.\nExample: `~addlanguages conversational EN`";
                                else if (e.GetArg("function") == "removelanguage")
                                    myMessage = "Usage: Remove a fluent, conversational or learning language from your languages list.\nExample: `~removelanguages fluent JA`";
                                else if (e.GetArg("function") == "hiragana" || e.GetArg("function") == "katakana")
                                    myMessage = "Usage: Print a " + e.GetArg("function") + " chart.";
                                else if (e.GetArg("function") == "meme")
                                    myMessage = "Usage: Shows a dank meme. Pretty straight forward. To add memes contant mods.";
                                await e.Channel.SendMessage(myMessage);

                            });
        }

        private void RegisterAddLanguageCommand()
        {
            commands.CreateCommand("addlanguage").Parameter("type").Parameter("Language")
                .Do(async (e) =>
                {
                    if (initFlag)
                    {
                        if (!languageRoles.ContainsKey(e.User.Id))
                        {
                            List<string>[] langlist = new List<string>[3];
                            langlist[0] = new List<string>();
                            langlist[1] = new List<string>();
                            langlist[2] = new List<string>();
                            languageRoles.Add(e.User.Id, langlist);
                        }
                        int type = -1;
                        if (e.GetArg("type") == "fluent")
                            type = 0;
                        else if (e.GetArg("type") == "conversational")
                            type = 1;
                        else if (e.GetArg("type") == "learning")
                            type = 2;
                        if (type != -1)
                        {
                            if (!languageRoles[e.User.Id][type].Contains(e.GetArg("Language")))
                            {
                                languageRoles[e.User.Id][type].Add(e.GetArg("Language"));
                            }
                            await e.Channel.SendMessage("Language successfully added.");
                        }
                        else
                            await e.Channel.SendMessage("Sorry, you entered an invalid language state, please try fluent, conversational or learning");
                        UpdateLanguagesDictionary();
                    }
                    else
                        await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
                });
        }

        private void RegisterLanguageCommand()
        {
            commands.CreateCommand("languages").Parameter("id")
                .Do(async (e) =>
                {
                    if (initFlag)
                    {
                        if (languageRoles.ContainsKey(e.Message.MentionedUsers.ElementAt(0).Id))
                        {
                            string fluentOut = "Fluent: ";
                            for (int i = 0; i < languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(0).Count; i++)
                            {
                                fluentOut += languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(0).ElementAt(i) + (i < languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(0).Count - 1 ? ", " : "");
                            }
                            if (languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(0).Count == 0)
                                fluentOut = "@";
                            string conversationalOut = "Conversational: ";
                            for (int i = 0; i < languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(1).Count; i++)
                            {
                                conversationalOut += languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(1).ElementAt(i) + (i < languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(1).Count - 1 ? ", " : "");
                            }
                            if (languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(1).Count == 0)
                                conversationalOut = "@";
                            string learningOut = "Learning: ";
                            for (int i = 0; i < languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(2).Count; i++)
                            {
                                learningOut += languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(2).ElementAt(i) + (i < languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(2).Count - 1 ? ", " : "");
                            }
                            if (languageRoles[e.Message.MentionedUsers.ElementAt(0).Id].ElementAt(2).Count == 0)
                                learningOut = "@";
                            if (fluentOut != "@")
                                await e.Channel.SendMessage(fluentOut);
                            else
                                await e.Channel.SendMessage("This user has no fluent languages");
                            if (conversationalOut != "@")
                                await e.Channel.SendMessage(conversationalOut);
                            else
                                await e.Channel.SendMessage("This user has no conversational languages");
                            if (learningOut != "@")
                                await e.Channel.SendMessage(learningOut);
                            else
                                await e.Channel.SendMessage("This user is not learning any languages");
                        }
                        else
                            await e.Channel.SendMessage("The user did not set his languages!");
                    }
                    else
                        await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
                });
        }

        private void RegisterRemoveLanguageCommand()
        {
            commands.CreateCommand("removelanguage").Parameter("type").Parameter("Language")
            .Do(async (e) =>
             {
                 if(initFlag)
                 {
                     if (!languageRoles.ContainsKey(e.User.Id))
                     {
                         await e.Channel.SendMessage("You don't have any assigned languages yet! Use `~addlanguage` to assign languages to yourself. ");
                     }
                     else
                     {
                     int type = -1;
                     if (e.GetArg("type") == "fluent")
                         type = 0;
                     else if (e.GetArg("type") == "conversational")
                         type = 1;
                     else if (e.GetArg("type") == "learning")
                         type = 2;
                     if (type != -1)
                     {
                         if (!languageRoles[e.User.Id][type].Contains(e.GetArg("Language")))
                         {
                            await e.Channel.SendMessage("You didn't have that language in the first place.");
                         }
                         else
                         {
                            await e.Channel.SendMessage("Succefully removed language.");
                            languageRoles[e.User.Id][type].Remove(e.GetArg("Language"));
                         }
                       
                     }
                     else
                         await e.Channel.SendMessage("Sorry, you entered an invalid language state, please try fluent, conversational or learning");
                     }
                     UpdateLanguagesDictionary();
                 }
                 else
                     await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
             });
        }

        private void RegisterCookieCommand()
        {
            commands.CreateCommand("cookie").Parameter("user")
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("");
                });

        }

        // Update database (text files) functions
       
        private void UpdateMemesList()
        {
            if (initFlag)
            {
                string line;
                string[] appString = new string[memes.Count + 1];
                string dirpath = Directory.GetCurrentDirectory();
                dirpath = RemoveFromEnd(dirpath, "\\bin\\Debug");
                dirpath = Path.Combine(dirpath, "Entities\\memeLinks.txt");

                int i = 0;
                for (i = 0; i < memes.Count; i++)
                {
                    line = memes.ElementAt(i);
                    appString[i] = line + '\n';
                }
                appString[memes.Count] = "@";

                System.IO.StreamWriter filewrite = new StreamWriter(dirpath);
                for (i = 0; i < appString.Length; i++)
                {
                    filewrite.Write(appString[i]);
                }
                filewrite.Close();
            }
        }

        private void UpdateLanguagesDictionary()
        {
            string dirpath = Directory.GetCurrentDirectory();
            dirpath = RemoveFromEnd(dirpath, "\\bin\\Debug");
            dirpath = Path.Combine(dirpath, "Entities\\languagesMap.txt");
            int i = 0;
            string[] list = new string[languageRoles.Count];
            for (i = 0; i < languageRoles.Count; i++)
            {
                ulong ID = languageRoles.Keys.ElementAt(i);
                List<string>[] typeArray = languageRoles[ID];
                List<string> fluentList = typeArray[0];
                List<string> conversationalList = typeArray[1];
                List<string> learningList = typeArray[2];
                string buff = "";
                string format = ID + "&";
                for (int j = 0; j < fluentList.Count; j++)
                    buff += fluentList.ElementAt(j) + (j == fluentList.Count - 1 ? "" : ",");
                format += buff + "&";
                buff = "";
                for (int j = 0; j < conversationalList.Count; j++)
                    buff += conversationalList.ElementAt(j) + (j == conversationalList.Count - 1 ? "" : ",");
                format += buff + "&";
                buff = "";
                for (int j = 0; j < learningList.Count; j++)
                    buff += learningList.ElementAt(j) + (j < learningList.Count - 1 ? "," : "");
                format += buff;
                list[i] = format;

            }
            System.IO.StreamWriter filewrite = new StreamWriter(dirpath);
            for (i = 0; i < list.Length; i++)
            {
                filewrite.Write(list[i] + '\n');
            }
            filewrite.Write("@");
            filewrite.Close();
        }

        // Functions for the commands that read from text

        private async Task AddLang(CommandEventArgs e, string line)
        {
            if (initFlag)
            {
                string[] arr = line.Split('&');
                ulong ID = Convert.ToUInt64(arr[0]);
                string[] fluentList = arr[1].Split(',');
                string[] conversationalList = arr[2].Split(',');
                string[] learningList = arr[3].Split(',');
                string empty = "";
                List<string>[] langArr = new List<string>[3];
                List<string> flu = new List<string>();
                List<string> con = new List<string>();
                List<string> lrn = new List<string>();
                for (int i = 0; i < fluentList.Length; i++)
                    if (fluentList[i] != empty)
                        flu.Add(fluentList[i]);
                for (int i = 0; i < conversationalList.Length; i++)
                    if (conversationalList[i] != empty)
                        con.Add(conversationalList[i]);
                for (int i = 0; i < learningList.Length; i++)
                    if (learningList[i] != empty)
                        lrn.Add(learningList[i]);
                langArr[0] = flu;
                langArr[1] = con;
                langArr[2] = lrn;
                languageRoles.Add(ID, langArr);
                await e.Channel.SendMessage("Successfully added languages for: " + ID);
            }
            else
                await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
        }

        private async Task AddMeme(CommandEventArgs e, string link)
        {
            if (initFlag)
            {
                bool flag = false;
                for (int i = 0; i < e.User.Roles.Count(); i++)
                {
                    if (e.User.Roles.ElementAt(i).ToString() == "Semi-Moderator")
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    memes.AddLast(link);
                    await e.Channel.SendMessage("Meme added to the meme list.");
                }
                else
                    PrintPremissionError(e);
            }
            else
                await e.Channel.SendMessage("Please ask a mod to use the `~init` function!");
        }

        // Functions that help with unrelated bot commands

        private void PrintPremissionError(CommandEventArgs e)
        {
            e.Channel.SendMessage(permissionError);
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private string RemoveFromEnd(string dirpath, string toRemove)
        {
            int i = 0;
            for (; i < dirpath.Length; i++)
            {
                string findString = dirpath.Substring(i, toRemove.Length);
                if (findString == toRemove)
                    break;
            }
            return dirpath.Substring(0, i);
        }

    }
}