using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using KX12To13Converter;

[assembly: RegisterObjectType(typeof(PageBuilderConversionsInfo), PageBuilderConversionsInfo.OBJECT_TYPE)]

namespace KX12To13Converter
{
    /// <summary>
    /// Data container class for <see cref="PageBuilderConversionsInfo"/>.
    /// </summary>
    [Serializable]
    public partial class PageBuilderConversionsInfo : AbstractInfo<PageBuilderConversionsInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kx12to13converter.pagebuilderconversions";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PageBuilderConversionsInfoProvider), OBJECT_TYPE, "KX12To13Converter.PageBuilderConversions", "PageBuilderConversionsID", "PageBuilderConversionsLastModified", "PageBuilderConversionsGuid", null, null, null, null, null, null)
        {
            ModuleName = "KX12To13Converter",
            TouchCacheDependencies = true,
            ContainsMacros = false,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("PageBuilderConversionDocumentID", "cms.document", ObjectDependencyEnum.Required),
            },
            SupportsCloneToOtherSite = false
        };


        /// <summary>
        /// Page builder conversions ID.
        /// </summary>
        [DatabaseField]
        public virtual int PageBuilderConversionsID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PageBuilderConversionsID"), 0);
            }
            set
            {
                SetValue("PageBuilderConversionsID", value);
            }
        }


        /// <summary>
        /// Page builder conversion document ID.
        /// </summary>
        [DatabaseField]
        public virtual int PageBuilderConversionDocumentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PageBuilderConversionDocumentID"), 0);
            }
            set
            {
                SetValue("PageBuilderConversionDocumentID", value);
            }
        }


        /// <summary>
        /// Page builder conversion successful.
        /// </summary>
        [DatabaseField]
        public virtual bool PageBuilderConversionSuccessful
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("PageBuilderConversionSuccessful"), false);
            }
            set
            {
                SetValue("PageBuilderConversionSuccessful", value);
            }
        }


        /// <summary>
        /// Page builder conversion date processed.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PageBuilderConversionDateProcessed
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("PageBuilderConversionDateProcessed"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PageBuilderConversionDateProcessed", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Page builder conversion template JSON.
        /// </summary>
        [DatabaseField]
        public virtual string PageBuilderConversionTemplateJSON
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageBuilderConversionTemplateJSON"), String.Empty);
            }
            set
            {
                SetValue("PageBuilderConversionTemplateJSON", value, String.Empty);
            }
        }


        /// <summary>
        /// Page builder conversion page builder JSON.
        /// </summary>
        [DatabaseField]
        public virtual string PageBuilderConversionPageBuilderJSON
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageBuilderConversionPageBuilderJSON"), String.Empty);
            }
            set
            {
                SetValue("PageBuilderConversionPageBuilderJSON", value, String.Empty);
            }
        }


        /// <summary>
        /// If true, then this document needs to have the conversion run on it..
        /// </summary>
        [DatabaseField]
        public virtual bool PageBuilderConversionMarkForConversion
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("PageBuilderConversionMarkForConversion"), false);
            }
            set
            {
                SetValue("PageBuilderConversionMarkForConversion", value);
            }
        }


        /// <summary>
        /// Page builder conversion marked for send.
        /// </summary>
        [DatabaseField]
        public virtual bool PageBuilderConversionMarkedForSend
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("PageBuilderConversionMarkedForSend"), false);
            }
            set
            {
                SetValue("PageBuilderConversionMarkedForSend", value);
            }
        }


        /// <summary>
        /// Page builder conversion last send date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PageBuilderConversionLastSendDate
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("PageBuilderConversionLastSendDate"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PageBuilderConversionLastSendDate", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Will be true if the KX13 does not have a matching document to receive..
        /// </summary>
        [DatabaseField]
        public virtual bool PageBuilderConversionNoMatchFound
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("PageBuilderConversionNoMatchFound"), false);
            }
            set
            {
                SetValue("PageBuilderConversionNoMatchFound", value);
            }
        }


        /// <summary>
        /// Page builder conversion notes.
        /// </summary>
        [DatabaseField]
        public virtual string PageBuilderConversionNotes
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageBuilderConversionNotes"), String.Empty);
            }
            set
            {
                SetValue("PageBuilderConversionNotes", value, String.Empty);
            }
        }


        /// <summary>
        /// Page builder conversions guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid PageBuilderConversionsGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("PageBuilderConversionsGuid"), Guid.Empty);
            }
            set
            {
                SetValue("PageBuilderConversionsGuid", value);
            }
        }


        /// <summary>
        /// Page builder conversions last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PageBuilderConversionsLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("PageBuilderConversionsLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PageBuilderConversionsLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PageBuilderConversionsInfoProvider.DeletePageBuilderConversionsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PageBuilderConversionsInfoProvider.SetPageBuilderConversionsInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected PageBuilderConversionsInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="PageBuilderConversionsInfo"/> class.
        /// </summary>
        public PageBuilderConversionsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="PageBuilderConversionsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public PageBuilderConversionsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}