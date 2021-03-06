﻿using System;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors.SQLite;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class NzbDroneSqliteProcessorFactory : SqliteProcessorFactory
    {
        public override IMigrationProcessor Create(String connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqliteDbFactory();
            var connection = factory.CreateConnection(connectionString);
            var generator = new SqliteGenerator() { compatabilityMode = CompatabilityMode.STRICT };
            return new NzbDroneSqliteProcessor(connection, generator, announcer, options, factory);
        }
    }
}
