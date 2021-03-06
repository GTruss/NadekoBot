﻿using NadekoBot.Services.Database.Repositories;
using NadekoBot.Services.Database.Repositories.Impl;
using System;
using System.Threading.Tasks;

namespace NadekoBot.Services.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        public NadekoContext _context { get; }

        private IQuoteRepository _quotes;
        public IQuoteRepository Quotes => _quotes ?? (_quotes = new QuoteRepository(_context));

        private IGuildConfigRepository _guildConfigs;
        public IGuildConfigRepository GuildConfigs => _guildConfigs ?? (_guildConfigs = new GuildConfigRepository(_context));

        private IDonatorsRepository _donators;
        public IDonatorsRepository Donators => _donators ?? (_donators = new DonatorsRepository(_context));

        private IClashOfClansRepository _clashOfClans;
        public IClashOfClansRepository ClashOfClans => _clashOfClans ?? (_clashOfClans = new ClashOfClansRepository(_context));

        private IReminderRepository _reminders;
        public IReminderRepository Reminders => _reminders ?? (_reminders = new ReminderRepository(_context));

        private ISelfAssignedRolesRepository _selfAssignedRoles;
        public ISelfAssignedRolesRepository SelfAssignedRoles => _selfAssignedRoles ?? (_selfAssignedRoles = new SelfAssignedRolesRepository(_context));

        private IBotConfigRepository _botConfig;
        public IBotConfigRepository BotConfig => _botConfig ?? (_botConfig = new BotConfigRepository(_context));

        private ICurrencyRepository _currency;
        public ICurrencyRepository Currency => _currency ?? (_currency = new CurrencyRepository(_context));

        private ICurrencyTransactionsRepository _currencyTransactions;
        public ICurrencyTransactionsRepository CurrencyTransactions => _currencyTransactions ?? (_currencyTransactions = new CurrencyTransactionsRepository(_context));

        private IUnitConverterRepository _conUnits;
        public IUnitConverterRepository ConverterUnits => _conUnits ?? (_conUnits = new UnitConverterRepository(_context));

        private IMusicPlaylistRepository _musicPlaylists;
        public IMusicPlaylistRepository MusicPlaylists => _musicPlaylists ?? (_musicPlaylists = new MusicPlaylistRepository(_context));

        private ICustomReactionRepository _customReactions;
        public ICustomReactionRepository CustomReactions => _customReactions ?? (_customReactions = new CustomReactionsRepository(_context));

        private IPokeGameRepository _pokegame;
        public IPokeGameRepository PokeGame => _pokegame ?? (_pokegame = new PokeGameRepository(_context));

        private IWaifuRepository _waifus;
        public IWaifuRepository Waifus => _waifus ?? (_waifus = new WaifuRepository(_context));

        private IDiscordUserRepository _discordUsers;
        public IDiscordUserRepository DiscordUsers => _discordUsers ?? (_discordUsers = new DiscordUserRepository(_context));

        private IVolunteersRepository _volunteers;
        public IVolunteersRepository Volunteers => _volunteers ?? (_volunteers = new VolunteersRepository(_context));

        private ITableReadsRepository _tableReads;
        public ITableReadsRepository TableReads => _tableReads ?? (_tableReads = new TableReadsRepository(_context));


        private IWritingPromptsRepository _writingPrompts;
        public IWritingPromptsRepository WritingPrompts => _writingPrompts ?? (_writingPrompts = new WritingPromptsRepository(_context));
        public UnitOfWork(NadekoContext context)
        {
            _context = context;
        }

        public int Complete() =>
            _context.SaveChanges();

        public Task<int> CompleteAsync() => 
            _context.SaveChangesAsync();

        private bool disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
                if (disposing)
                    _context.Dispose();
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
