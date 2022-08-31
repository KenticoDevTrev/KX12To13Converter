using CMS.DataEngine;
using KX12To13Converter.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace KX12To13Converter.Base.PageOperations
{
    public class PreUpgrade3ConvertPageTypesForMVC : IPreUpgrade3ConvertPageTypesForMVC
    {
        public IEnumerable<DataClassInfo> GetPageBuidlerPageTypes()
        {
            return DataClassInfoProvider.GetClasses()
                .Where("ClassName <> 'CMS.Root' and ClassIsDocumentType = 1 and COALESCE(ClassIsContentOnly, 0) = 0 and ClassUrlPattern is null")
                .OrderBy("ClassDisplayName")
                .TypedResult;
        }

        public IEnumerable<DataClassInfo> GetPageBuilderContentOnly()
        {
            return DataClassInfoProvider.GetClasses()
                .Where("ClassName <> 'CMS.Root' and ClassIsDocumentType = 1 and ClassIsContentOnly = 1 and CLassUrlPattern is null")
                .OrderBy("ClassDisplayName")
                .TypedResult;
        }

        public IEnumerable<DataClassInfo> GetClassWithUrlPages()
        {
            return ConnectionHelper.ExecuteQuery(@"select distinct OuterC.* from View_CMS_Tree_Joined outerT
left join CMS_Class outerC on OuterC.CLassID = OuterT.NodeClassID
where coalesce(OuterC.ClassURLPattern, '') = '' and OuterC.ClassName <> 'CMS.root'
and NodeID in (
select innerT.NodeParentID from View_CMS_Tree_Joined innerT
left join CMS_CLass C on C.ClassID = innerT.NodeClassID 
where coalesce(C.ClassURLPattern, '') <> '' and C.ClassName <> 'cms.root')", null, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => new DataClassInfo(x));
        }

        public int GetTotalWidgetContentPages(DataClassInfo classObj)
        {
            return (int)ConnectionHelper.ExecuteQuery($"select count(*) as itemCount from View_CMS_Tree_Joined where NodeClassID = {classObj.ClassID} and ((len(COALESCE(DocumentContent, '')) > 0 and DocumentContent <> '<content></content>')  or LEN(COALESCE(DocumentWebParts, '')) > 0)", null, QueryTypeEnum.SQLQuery).Tables[0].Rows[0]["itemCount"];
        }
        public int GetTotalPages(DataClassInfo classObj)
        {
            return (int)ConnectionHelper.ExecuteQuery($"select count(*) as itemCount from View_CMS_Tree_Joined where NodeClassID = {classObj.ClassID}", null, QueryTypeEnum.SQLQuery).Tables[0].Rows[0]["itemCount"];
        }

        public void AdjustClass(string adjustToValue, string defaultTemplate, int classID)
        {
            string query = "";
            string subQuery = "";
            switch (adjustToValue.ToLower())
            {
                case "pagebuilder":
                    query = $" update CMS_Class set ClassURLPattern = '{{% NodeAliasPath %}}', ClassIsContentOnly = 1, CLassIsMenuItemType = 1 where ClassID = {classID}";
                    // Set default page template on documents if specified
                    if (!string.IsNullOrWhiteSpace(defaultTemplate))
                    {
                        subQuery = "update CMS_Document set DocumentPageTemplateConfiguration = '{\"identifier\":\"" + defaultTemplate.Trim() + "\",\"properties\":{}}' where DocumentPageTemplateConfiguration is null and DocumentNodeID in (Select NodeID from CMS_Tree where NodeClassID = " + classID.ToString() + ")";
                    }
                    break;
                case "urlonly":
                    query = $" update CMS_Class set ClassURLPattern = '{{% NodeAliasPath %}}', ClassIsContentOnly = 1, ClassIsMenuItemType = 0 where ClassID = {classID}";
                    subQuery = $"update CMS_Document set DocumentContent = null, DocumentWebParts = null where (LEN(COALESCE(DocumentContent, ''))+LEN(COALESCE(DocumentWebParts, '')) > 0) and DocumentNodeID in (Select nodeid from CMS_Tree where NodeClassID = {classID})";
                    break;
                case "neither":
                    query = $" update CMS_Class set ClassURLPattern = '', ClassIsContentOnly = 1, ClassIsMenuItemType = 0 where ClassID = {classID}";
                    subQuery = $"update CMS_Document set DocumentContent = null, DocumentWebParts = null where (LEN(COALESCE(DocumentContent, ''))+LEN(COALESCE(DocumentWebParts, '')) > 0) and DocumentNodeID in (Select nodeid from CMS_Tree where NodeClassID = {classID})";
                    break;
            }
            ConnectionHelper.ExecuteNonQuery(query, null, QueryTypeEnum.SQLQuery);
            if (!string.IsNullOrWhiteSpace(subQuery))
            {
                ConnectionHelper.ExecuteNonQuery(query, null, QueryTypeEnum.SQLQuery);
            }
        }

        public void EnableUrlFeature(string adjustToValue, DataClassInfo classObj)
        {
            string query = "";
            string subQuery = "";

            switch (adjustToValue.ToLower())
            {
                case "urlonly":
                    if (classObj.ClassIsCoupledClass)
                    {
                        query = $" update CMS_Class set ClassURLPattern = '{{% NodeAliasPath %}}', ClassIsContentOnly = 1, ClassIsMenuItemType = 0 where ClassID = {classObj.ClassID}";
                        subQuery = $"update CMS_Document set DocumentContent = null, DocumentWebParts = null where (LEN(COALESCE(DocumentContent, ''))+LEN(COALESCE(DocumentWebParts, '')) > 0) and DocumentNodeID in (Select nodeid from CMS_Tree where NodeClassID = {classObj.ClassID})";
                    }
                    else
                    {
                        TurnContainerToCouple(classObj);
                    }
                    break;
            }
            if (!string.IsNullOrWhiteSpace(query))
            {
                ConnectionHelper.ExecuteNonQuery(query, null, QueryTypeEnum.SQLQuery);
            }
            if (!string.IsNullOrWhiteSpace(subQuery))
            {
                ConnectionHelper.ExecuteNonQuery(query, null, QueryTypeEnum.SQLQuery);
            }
        }

        private void TurnContainerToCouple(DataClassInfo classObj)
        {
            string query = @"-- DO NOT MODIFY BELOW
declare @ClassName nvarchar(200);
declare @TableName nvarchar(200);
declare @FormIDFieldGuid nvarchar(50);
declare @FormNameFieldGuid nvarchar(50);
declare @FormIDSearchFieldGuid nvarchar(50);
declare @FormNameSearchFieldGuid nvarchar(50);
set @ClassName = @Namespace+'.'+@Name
set @TableName = @Namespace+'_'+@Name
set @FormIDFieldGuid = LOWER(Convert(nvarchar(50), NewID()));
set @FormNameFieldGuid = LOWER(Convert(nvarchar(50), NewID()));
set @FormIDSearchFieldGuid = LOWER(Convert(nvarchar(50), NewID()));
set @FormNameSearchFieldGuid = LOWER(Convert(nvarchar(50), NewID()));

-- Update Class
update CMS_Class set
ClassIsDocumentType = 1,
ClassIsCoupledClass = 1,
ClassXmlSchema = '<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema id=""NewDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""NewDataSet"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""'+@TableName+'"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""'+@Name+'ID"" msdata:ReadOnly=""true"" msdata:AutoIncrement=""true"" type=""xs:int"" />
              <xs:element name=""Name"">
                <xs:simpleType>
                  <xs:restriction base=""xs:string"">
                    <xs:maxLength value=""200"" />
                  </xs:restriction>
                </xs:simpleType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
    <xs:unique name=""Constraint1"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//'+@TableName+'"" />
      <xs:field xpath=""Folder2ID"" />
    </xs:unique>
  </xs:element>
</xs:schema>',
ClassFormDefinition = '<form version=""2""><field column=""'+@Name+'ID"" columntype=""integer"" guid=""'+@FormIDFieldGuid+'"" isPK=""true"" publicfield=""false""><properties><fieldcaption>'+@Name+'ID</fieldcaption></properties><settings><controlname>labelcontrol</controlname></settings></field><field column=""Name"" columnsize=""200"" columntype=""text"" guid=""'+@FormNameFieldGuid+'"" publicfield=""false"" visible=""true""><properties><fieldcaption>Name</fieldcaption></properties><settings><AutoCompleteEnableCaching>False</AutoCompleteEnableCaching><AutoCompleteFirstRowSelected>False</AutoCompleteFirstRowSelected><AutoCompleteShowOnlyCurrentWordInCompletionListItem>False</AutoCompleteShowOnlyCurrentWordInCompletionListItem><controlname>TextBoxControl</controlname><FilterMode>False</FilterMode><Trim>False</Trim></settings></field></form>',
ClassNodeNameSource = 'Name',
ClassTableName = @TableName,
ClassShowTemplateSelection = null,
ClassIsMenuItemType = null,
ClassSearchTitleColumn = 'DocumentName',
ClassSearchContentColumn='DocumentContent',
ClassSearchCreationDateColumn = 'DocumentCreatedWhen', 
ClassSearchSettings = '<search><item azurecontent=""True"" azureretrievable=""False"" azuresearchable=""True"" content=""True"" id=""'+@FormNameSearchFieldGuid+'"" name=""Name"" searchable=""False"" tokenized=""True"" /><item azurecontent=""False"" azureretrievable=""True"" azuresearchable=""False"" content=""False"" id=""'+@FormIDSearchFieldGuid+'"" name=""'+@Name+'ID"" searchable=""True"" tokenized=""False"" /></search>',
ClassInheritsFromClassID = 0,
ClassSearchEnabled = 1,
ClassIsContentOnly = 1,
ClassURLPattern = case when @EnsureUrlPattern = 1 then '{% NodeAliasPath %}' else ClassURLPattern end
where ClassName = @ClassName

-- Create table
declare @CreateTable nvarchar(max);
set @CreateTable = '
CREATE TABLE [dbo].['+@TableName+'](
	['+@Name+'ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_'+@TableName+'] PRIMARY KEY CLUSTERED 
(
	['+@Name+'ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
ALTER TABLE [dbo].['+@TableName+'] ADD  CONSTRAINT [DEFAULT_'+@TableName+'_Name]  DEFAULT (N'''') FOR [Name]'
exec(@CreateTable);

-- Populate joining table data, as well as generate the default url path entry based on nodealiaspath
declare @BindingAndVersionSQL nvarchar(max);

set @BindingAndVersionSQL = '
declare @ClassName nvarchar(200);
declare @TableName nvarchar(200);
declare @documentid int;
declare @documentname nvarchar(200);
declare @documentculture nvarchar(10);
declare @NodeID int;
declare @SiteID int;
declare @newrowid int;
set @ClassName = '''+@Namespace+'.'+@Name+'''
set @TableName = '''+@Namespace+'_'+@Name+'''
declare contenttable_cursor cursor for
 select * from (
  select
  COALESCE(D.DocumentID, NoCultureD.DocumentID) as DocumentID,
COALESCE(D.DocumentName, NoCultureD.DocumentName) as DocumentName,
 C.CultureCode,
NodeID, 
NodeSiteID
  from  CMS_Site S
    left join CMS_SiteCulture SC on SC.SiteID = S.SiteID
	left join CMS_Culture C on C.CultureID = SC.CultureID
	left join CMS_Tree on NodeSiteID = S.SiteID
	left join CMS_Class on ClassID = NodeClassID
	left outer join CMS_Document D on D.DocumentNodeID = NodeID and D.DocumentCulture = C.CultureCode
	left outer join CMS_Document NoCultureD on NoCultureD.DocumentNodeID = NodeID
	where ClassName = @ClassName
	and D.DocumentName is not null and NoCultureD.DocumentName is not null
	) cultureAcross
	group by DocumentID, DocumentName, CultureCode, NodeID, NodeSiteID
	order by DocumentID
open contenttable_cursor
fetch next from contenttable_cursor into @documentid, @documentname, @documentculture, @NodeID, @SiteID
WHILE @@FETCH_STATUS = 0  BEGIN
	-- insert into binding table --
	INSERT INTO [dbo].['+@TableName+'] ([Name]) VALUES (@documentname)
	
	-- Update document --
	set @newrowid = SCOPE_IDENTITY();
	update CMS_Document set DocumentForeignKeyValue = @newrowid where DocumentID = @documentid
	
	-- update also history --
	update CMS_VersionHistory set NodeXML = replace(NodeXML, ''<DocumentID>''+CONVERT(nvarchar(10), @documentid)+''</DocumentID>'', ''<DocumentID>''+CONVERT(nvarchar(10), @documentid)+''</DocumentID><DocumentForeignKeyValue>''+CONVERT(nvarchar(10), @newrowid)+''</DocumentForeignKeyValue>'') where DocumentID = @documentid
	
	FETCH NEXT FROM contenttable_cursor into @documentid, @documentname, @documentculture, @NodeID, @SiteID
END
Close contenttable_cursor
DEALLOCATE contenttable_cursor'
exec(@BindingAndVersionSQL)";

            // This will create the coupled table, initiate the default document of each one, and then enable the url feature
            ConnectionHelper.ExecuteNonQuery(query, new QueryDataParameters()
        {
            {"@Namespace", classObj.ClassName.Split('.')[0] },
            {"@Name", classObj.ClassName.Replace(classObj.ClassName.Split('.')[0], "").Trim('.') },
            {"@EnsureUrlPattern", true },
        }, QueryTypeEnum.SQLQuery);

        }
    }
}
