using System;
using System.Collections.Generic;
using System.Linq;
using GenericBot.Entities;
using LiteDB;

namespace GenericBot.Database
{
    /// <summary>
    /// Implementation of <see cref="IDatabaseEngine"/> for LiteDb
    /// </summary>
    public class LiteDbEngine : IDatabaseEngine
    {
        private LiteDatabase liteDatabase;

        public LiteDbEngine()
        {
            liteDatabase = new LiteDatabase(Core.GlobalConfig.DbConnectionString);
        }

        ///<inheritdoc cref="IDatabaseEngine.AddOrUpdateExceptionReport(ExceptionReport)"/>
        public ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report)
        {
            var _exceptionDb = liteDatabase.GetCollection<ExceptionReport>($"exceptions");
            ExceptionReport foundReport;
            if (_exceptionDb.FindAll().ToList()
                .HasElement(r => r.Message.Equals(report.Message) && r.StackTrace.Equals(report.StackTrace), out foundReport))
            {
                foundReport.Count++;
                _exceptionDb.Upsert(foundReport);
            }
            else
            {
                _exceptionDb.Insert(report);
                foundReport = report;
            }
            return foundReport;
        }

        ///<inheritdoc cref="IDatabaseEngine.AddQuote(string, ulong)"/>
        public Quote AddQuote(string quote, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED Quote TO {guildId}");
            var _quoteDb = liteDatabase.GetCollection<Quote>($"{guildId}-quotes");
            var q = new Quote
            {
                Content = quote,
                Id = _quoteDb.Count() == 0 ? 1 : (int)_quoteDb.Count() + 1,
                Active = true
            };
            _quoteDb.Insert(q);
            return q;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetAllQuotes(ulong)"/>
        public List<Quote> GetAllQuotes(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED Quote TO {guildId}");
            var _quoteDb = liteDatabase.GetCollection<Quote>($"{guildId}-quotes");
            return _quoteDb.FindAll().ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.RemoveQuote(int, ulong)"/>
        public bool RemoveQuote(int id, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] DELETE Quote {id} FROM {guildId}");
            var _quoteDb = liteDatabase.GetCollection<Quote>($"{guildId}-quotes");
            var _quote = _quoteDb.FindOne(q => q.Id == id);
            _quote.Active = false;
            try
            {
                _quoteDb.Upsert(_quote);
                return true;
            }
            catch (Exception ex)
            {
                Core.Logger.LogErrorMessage(ex, null);
                return false;
            }
        }

        ///<inheritdoc cref="IDatabaseEngine.AddStatus(Status)"/>
        public void AddStatus(Status status)
        {
            var _db = liteDatabase.GetCollection<Status>($"statusLog");
            _db.Insert(status);
        }

        ///<inheritdoc cref="IDatabaseEngine.AddToAuditLog(ParsedCommand, ulong)"/>
        public void AddToAuditLog(ParsedCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED AuditLog TO {guildId}");
            var _db = liteDatabase.GetCollection<AuditCommand>($"{guildId}-auditLog");
            _db.Insert(new AuditCommand(command));
        }

        ///<inheritdoc cref="IDatabaseEngine.GetAuditLog(ulong)"/>
        public List<AuditCommand> GetAuditLog(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] GOT AuditLog FROM {guildId}");
            var _db = liteDatabase.GetCollection<AuditCommand>($"{guildId}-auditLog");
            return _db.FindAll().ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveCustomCommand(CustomCommand, ulong)"/>
        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED CustomComand {command.Name} TO {guildId}");
            var _db = liteDatabase.GetCollection<CustomCommand>($"{guildId}-commands");
            _db.Upsert(command);
            return command;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetCustomCommands(ulong)"/>
        public List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            var _db = liteDatabase.GetCollection<CustomCommand>($"{guildId}-commands");
            return _db.FindAll().ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.DeleteCustomCommand(string, ulong)"/>
        public void DeleteCustomCommand(string name, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] DELETE CustomCommand {name} FROM {guildId}");
            var _db = liteDatabase.GetCollection<CustomCommand>($"{guildId}-commands");
            _db.DeleteMany(c => c.Name == name);
        }

        ///<inheritdoc cref="IDatabaseEngine.GetGiveaways(ulong)"/>
        public List<Giveaway> GetGiveaways(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] GOT giveaways FROM {guildId}");
            var _db = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");

            return _db.FindAll().ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.UpdateOrCreateGiveaway(Giveaway, ulong)"/>
        public Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] UPDATE giveaways FROM {guildId}");
            var _giveawayDb = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");
            _giveawayDb.Upsert(giveaway);
            return giveaway;
        }

        ///<inheritdoc cref="IDatabaseEngine.CreateGiveaway(Giveaway, ulong)"/>
        public Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] CREATE GIVEAWAY {giveaway.Id} FOR {guildId}");
            var _giveawayDb = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");
            while (_giveawayDb.Exists(g => g.Id == giveaway.Id))
            {
                giveaway.Id += new Random().Next(0, 10).ToString();
            }
            _giveawayDb.Insert(giveaway);
            return giveaway;
        }

        ///<inheritdoc cref="IDatabaseEngine.DeleteGiveaway(Giveaway, ulong)"/>
        public void DeleteGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] DELETE giveaway {giveaway.Id} FROM {guildId}");
            var _giveawayDb = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");
            _giveawayDb.DeleteMany(g => g.Id == giveaway.Id);
        }

        ///<inheritdoc cref="IDatabaseEngine.GetUserFromGuild(ulong, ulong, bool)"/>
        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[LiteDb] GOT User {userId} FROM {guildId}");
            var _db = liteDatabase.GetCollection<DatabaseUser>($"{guildId}-users");
            if (_db.Exists(u => u.Id == userId))
                return _db.Find(u => u.Id == userId).First();
            else return new DatabaseUser(userId);
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveUserToGuild(DatabaseUser, ulong, bool)"/>
        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[LiteDb] SAVED User {user.Id} TO {guildId}");
            var _db = liteDatabase.GetCollection<DatabaseUser>($"{guildId}-users");
            _db.Upsert(user);
            return user;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetAllUsers(ulong)"/>
        public List<DatabaseUser> GetAllUsers(ulong guildId)
        {
            var _db = liteDatabase.GetCollection<DatabaseUser>($"{guildId}-users");
            return _db.FindAll().ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.RemoveBanFromGuild(ulong, ulong)"/>
        public void RemoveBanFromGuild(ulong banId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] DELETE Ban {banId} FROM {guildId}");

            var _db = liteDatabase.GetCollection<GenericBan>($"{guildId}-bans");
            if (_db.Exists(b => b.Id == banId))
                _db.DeleteMany(b => b.Id == banId);
        }

        ///<inheritdoc cref="IDatabaseEngine.GetBansFromGuild(ulong, bool)"/>
        public List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] GET bans FROM {guildId}");
            var _db = liteDatabase.GetCollection<GenericBan>($"{guildId}-bans");
            return _db.FindAll().ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveBanToGuild(GenericBan, ulong)"/>
        public GenericBan SaveBanToGuild(GenericBan ban, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED Ban {ban.Id} TO {guildId}");
            var _db = liteDatabase.GetCollection<GenericBan>($"{guildId}-bans");
            _db.Upsert(ban);
            return ban;
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveGuildConfig(GuildConfig)"/>
        public GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED GuildConfig TO {guildConfig.Id}");

            var _db = liteDatabase.GetCollection<GuildConfig>($"{guildConfig.Id}-config");
            _db.Upsert(guildConfig);
            return guildConfig;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetGuildConfig(ulong)"/>
        public GuildConfig GetGuildConfig(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] GOT GuildConfig FROM {guildId}");
            var _configDb = liteDatabase.GetCollection<GuildConfig>($"{guildId}-config");
            if (_configDb.Find(c => c.Id == guildId).Any())
                return _configDb.Find(c => c.Id == guildId).First();
            else
                return new GuildConfig(guildId);
        }

        ///<inheritdoc cref="IDatabaseEngine.AddVerification(ulong, ulong)"/>
        public void AddVerification(ulong userId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] ADD VERIFICATION {userId} TO {guildId}");

            var _db = liteDatabase.GetCollection<VerificationEvent>($"{guildId}-verifications");

            var _event = new VerificationEvent(guildId, userId);
            _db.Insert(_event);
        }
    }
}
