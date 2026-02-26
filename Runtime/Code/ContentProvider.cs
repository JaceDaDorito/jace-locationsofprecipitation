using System;
using RoR2;

namespace LOP
{
	public class ContentProvider : IContentPackProvider
	{
        public string identifier => LocationsOfPrecipitation.GUID + "." + nameof(ContentProvider);

        private readonly ContentPack _contentPack = new ContentPack();

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            _contentPack.identifier = identifier;

            InstantiateArtifactPortal.CreateAndRegisterLaptop(_contentPack);

            yield break;
        }
    }
}
