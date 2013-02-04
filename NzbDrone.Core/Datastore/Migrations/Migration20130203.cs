﻿using System.Collections.Generic;
using Migrator.Framework;
using NzbDrone.Core.Repository;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20130203)]
    public class Migration20130203 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            var objectDb = GetObjectDb();


            var rootFolderRepo = new RootFolderRepository(objectDb);


            using (var dataReader = Database.ExecuteQuery("SELECT * from RootDirs"))
            {
                var dirs = new List<RootDir>();
                while (dataReader.Read())
                {
                    var rootFolder = new RootDir { Path = dataReader["Path"].ToString() };
                    dirs.Add(rootFolder);
                }
                                objectDb.InsertMany(dirs);
            }
            //Database.RemoveTable("RootDirs");

        }
    }
}