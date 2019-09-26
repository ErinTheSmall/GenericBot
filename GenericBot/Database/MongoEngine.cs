﻿using GenericBot.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Database
{
    public class MongoEngine
    {
        private MongoClient mongoClient;

        public MongoEngine()
        {
            mongoClient = new MongoClient(Core.GlobalConfig.DbConnectionString);
        }

        /// <summary>
        /// Get the guild config if it's in the database, return a new config if not
        /// </summary>
        /// <param name="GuildId"></param>
        /// <returns></returns>
        public GuildConfig GetGuildConfig(ulong GuildId)
        {
            var _configDb = GetDatabaseFromGuildId(GuildId);
            var _collection = _configDb.GetCollection<GuildConfig>("config");

            if (_collection.Find(c => c.Id == GuildId).Any())
                return _collection.Find(c => c.Id == GuildId).First();
            else
                return new GuildConfig(GuildId);
        }
        /// <summary>
        /// Update the config in the database if it exists, add it if not
        /// </summary>
        /// <param name="guildConfig"></param>
        /// <returns></returns>
        public GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            var _configDb = GetDatabaseFromGuildId(guildConfig.Id);
            var _collection = _configDb.GetCollection<GuildConfig>("config");
            if (_collection.Find(c => c.Id == guildConfig.Id).Any())
                _collection.ReplaceOne(c => c.Id == guildConfig.Id, guildConfig);
            else _collection.InsertOne(guildConfig);

            return guildConfig;
        }
        /// <summary>
        /// Add a command to the database, overwrite if exists already.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");
            if (_collection.Find(c => c.Name == command.Name).Any())
                _collection.ReplaceOne(c => c.Name == command.Name, command);
            else _collection.InsertOne(command);
            return command;
        }
        public List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");
            var list = _collection.Find(new BsonDocument()).ToList();
            return list;
        }
        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId)
        {
            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == userId).Any())
                return _collection.Find(u => u.Id == userId).First();
            else return new DatabaseUser(userId);
        }
        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId)
        {
            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == user.Id).Any())
                _collection.FindOneAndReplace(u => u.Id == user.Id, user);
            _collection.InsertOne(user);

            return user;
        }

        public List<string> GetGuildIdsFromDb()
        {
            return mongoClient.ListDatabaseNames().ToList();
        }
        private IMongoDatabase GetDatabaseFromGuildId(ulong GuildId)
        {
            return mongoClient.GetDatabase(GuildId.ToString());
        }
    }
}
