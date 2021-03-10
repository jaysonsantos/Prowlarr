using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Indexers.Headphones
{
    public class Headphones : HttpIndexerBase<HeadphonesSettings>
    {
        public override string Name => "Headphones VIP";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;
        public override IndexerPrivacy Privacy => IndexerPrivacy.Private;
        public override string BaseUrl => "https://indexer.codeshy.com";
        public override IndexerCapabilities Capabilities => SetCapabilities();

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new HeadphonesRequestGenerator()
            {
                PageSize = PageSize,
                Settings = Settings,
                Capabilities = Capabilities,
                BaseUrl = BaseUrl
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new HeadphonesRssParser(Capabilities.Categories);
        }

        public Headphones(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger)
            : base(httpClient, indexerStatusService, configService, logger)
        {
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            base.Test(failures);

            if (failures.Any())
            {
                return;
            }
        }

        public override byte[] Download(HttpUri link)
        {
            var requestBuilder = new HttpRequestBuilder(link.FullUri);

            var downloadBytes = Array.Empty<byte>();

            var request = requestBuilder.Build();

            request.AddBasicAuthentication(Settings.Username, Settings.Password);

            try
            {
                downloadBytes = _httpClient.Execute(request).ResponseData;
            }
            catch (Exception)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                _logger.Error("Download failed");
            }

            return downloadBytes;
        }

        private IndexerCapabilities SetCapabilities()
        {
            var caps = new IndexerCapabilities
            {
                MusicSearchParams = new List<MusicSearchParam>
                       {
                           MusicSearchParam.Q
                       },
            };

            caps.Categories.AddCategoryMapping(3000, NewznabStandardCategory.Audio);
            caps.Categories.AddCategoryMapping(3010, NewznabStandardCategory.AudioMP3);
            caps.Categories.AddCategoryMapping(3040, NewznabStandardCategory.AudioLossless);

            return caps;
        }
    }
}