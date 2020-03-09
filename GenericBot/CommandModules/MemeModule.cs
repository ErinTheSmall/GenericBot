using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GenericBot.CommandModules
{
    class MemeModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command mock = new Command("mock");
            mock.WorksInDms = true;
            mock.Description = "MOcKinG sPoNgeBoB TeXt";
            mock.ToExecute += async (context) =>
            {
                string mockedMessage = "";
                double rand = new Random().NextDouble();
                foreach (var c in context.ParameterString.ToLower())
                {
                    rand += new Random().NextDouble();
                    if (rand >= 1.10)
                    {
                        rand = new Random().NextDouble();
                        mockedMessage += char.ToUpper(c);
                    }
                    else
                        mockedMessage += c;
                }
                await context.Message.ReplyAsync(mockedMessage);
            };
            commands.Add(mock);

            Command clap = new Command("clap");
            clap.WorksInDms = true;
            clap.Usage = "Put the clap emoji between each word";
            clap.ToExecute += async (context) =>
            {
                await context.Message.ReplyAsync(context.Parameters.Rejoin(" :clap: ") + " :clap:");
            };
            commands.Add(clap);

            Command kanye = new Command("kanye"); //Module by Venus (Wickn), with a ton of help from chef, and other kind souls. And Google.
            kanye.WorksInDms = true;
            kanye.Usage = "Pastes a Kanye West quote, courtesy of kanye.rest";
            kanye.ToExecute += async (context) =>
            {
                string[] badWords = {"COSBY", "2024", "sex", "titties", "porn", "Trump", "Ni**as", "titty", "suppress"};
                string kanyeQuote = string.Empty;
                using (var webclient = new WebClient())
                {
                    kanyeQuote = webclient.DownloadString(new Uri("https://api.kanye.rest/?format=text"));
                    while(badWords.Any(kanyeQuote.Contains))
                    {
                        kanyeQuote = webclient.DownloadString(new Uri("https://api.kanye.rest/?format=text"));
                    }
                await context.Message.ReplyAsync("> " + kanyeQuote + "\n- Kanye West"); 
                }
            };
            commands.Add(kanye);
            
            return commands;
        }
    }
}
