﻿using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]

    public class DeleteFileFixture : CoreTest
    {
        private void WithRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(@"C:\Test\Recycle Bin".AsOsAgnostic());
        }

        private void WithoutRecycleBin()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(String.Empty);
        }

        [Test]
        public void should_use_delete_when_recycleBin_is_not_configured()
        {
            WithoutRecycleBin();

            var path = @"C:\Test\TV\30 Rock\S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(path), Times.Once());
        }

        [Test]
        public void should_use_move_when_recycleBin_is_configured()
        {
            WithRecycleBin();

            var path = @"C:\Test\TV\30 Rock\S01E01.avi".AsOsAgnostic();

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(path, @"C:\Test\Recycle Bin\S01E01.avi".AsOsAgnostic(), true), Times.Once());
        }

        [Test]
        public void should_use_alternative_name_if_already_exists()
        {
            WithRecycleBin();

            var path = @"C:\Test\TV\30 Rock\S01E01.avi".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FileExists(@"C:\Test\Recycle Bin\S01E01.avi".AsOsAgnostic()))
                .Returns(true);

            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(path, @"C:\Test\Recycle Bin\S01E01_2.avi".AsOsAgnostic(), true), Times.Once());
        }

        [Test]
        public void should_call_fileSetLastWriteTime_for_each_file()
        {
            WindowsOnly();
            WithRecycleBin();
            var path = @"C:\Test\TV\30 Rock\S01E01.avi".AsOsAgnostic();


            Mocker.Resolve<RecycleBinProvider>().DeleteFile(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.FileSetLastWriteTimeUtc(@"C:\Test\Recycle Bin\S01E01.avi".AsOsAgnostic(), It.IsAny<DateTime>()), Times.Once());
        }
    }
}
