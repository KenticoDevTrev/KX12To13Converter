using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace KX12To13Converter
{
    /// <summary>
    /// Class providing <see cref="PageBuilderConversionsInfo"/> management.
    /// </summary>
    public partial class PageBuilderConversionsInfoProvider : AbstractInfoProvider<PageBuilderConversionsInfo, PageBuilderConversionsInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="PageBuilderConversionsInfoProvider"/>.
        /// </summary>
        public PageBuilderConversionsInfoProvider()
            : base(PageBuilderConversionsInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="PageBuilderConversionsInfo"/> objects.
        /// </summary>
        public static ObjectQuery<PageBuilderConversionsInfo> GetPageBuilderConversions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="PageBuilderConversionsInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="PageBuilderConversionsInfo"/> ID.</param>
        public static PageBuilderConversionsInfo GetPageBuilderConversionsInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="PageBuilderConversionsInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="PageBuilderConversionsInfo"/> to be set.</param>
        public static void SetPageBuilderConversionsInfo(PageBuilderConversionsInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="PageBuilderConversionsInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="PageBuilderConversionsInfo"/> to be deleted.</param>
        public static void DeletePageBuilderConversionsInfo(PageBuilderConversionsInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="PageBuilderConversionsInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="PageBuilderConversionsInfo"/> ID.</param>
        public static void DeletePageBuilderConversionsInfo(int id)
        {
            PageBuilderConversionsInfo infoObj = GetPageBuilderConversionsInfo(id);
            DeletePageBuilderConversionsInfo(infoObj);
        }
    }
}