-------------------------------------------------------------------------------
-------- POST UPGRADE Kentico Xperience 12 to Kentico Xperience 13  -----------
-------- Class Form, Xml Schema, and Alt Form Check                ------------
-- Instances of Kentico Xperience that started before KX 12 may have an old  --
-- Tables, Views, Index/Constraints/Functions, etc.  These should be updated,--
-- obsolete items removed, and items updated if they differ from a fresh     --
-- Kentico Xperience 13 installation.                                        --
--                                                                           --
-- INSTRUCTIONS:                                                             --
--  1. Have a fresh Kentico Xperience 13 database for comparison             --
--  2. Find and replace FRESH_XPERIENCEDB with the fresh Kentico 13 database --
--  3. Find and replace UPGRADED_XPERIENCEDB with the database you upgrade   --
--  4. Uncomment and run the Create Function StripMacros, used to ignore     --
--      Macro Signature mismatches and also order="#"                        --
--  5. Run the query                                                         --
--  6. Check the mismatches with a string comparison tool, and determine     --
--       if you need to update the value (keep in mind you may have          --
--       customized tables such as CMS.User/CMS.UserSettings)                --
--  7. If you deem it needs to be updated, run the UpdateStatement script.   --
--                                                                           --
--                                                                           --
-- ALWAYS Backup before running these.  Don't be THAT Guy                    --
-- Author: Trevor Fayas, version 1.0.0                                       --
-------------------------------------------------------------------------------

-- RUN THIS ONCE
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE FUNCTION [dbo].[StripMacros] 
--(
--	-- Add the parameters for the function here
--	@OriginalString nvarchar(max)
--)
--RETURNS nvarchar(max)
--AS
--BEGIN
--DECLARE @MyString nvarchar(max) = @OriginalString 

--WHILE CHARINDEX('{%', @MyString) > 0 AND CHARINDEX('%}', @MyString) > CHARINDEX('{%', @MyString)
--BEGIN
--    SELECT @MyString = REPLACE(@MyString, 
--                               SUBSTRING(@MyString, 
--                                         CHARINDEX('{%', @MyString), -- Substring from First % found, to the second % found to get the content to replace
--										 CHARINDEX('%}', @MyString)+1-CHARINDEX('{%', @MyString)+1
--								)
--                       , '')
--END
---- Remove order as well as that causes a lot of false positivies
--WHILE CHARINDEX('order="', @MyString) > 0 AND CHARINDEX('"', @MyString, CHARINDEX('order="', @MyString)+7) > CHARINDEX('order="', @MyString)
--BEGIN
--    SELECT @MyString = REPLACE(@MyString, 
--                               SUBSTRING(@MyString, 
--                                         CHARINDEX('order="', @MyString), -- Substring from First % found, to the second % found to get the content to replace
--										  CHARINDEX('"', @MyString, CHARINDEX('order="', @MyString)+7)-CHARINDEX('order="', @MyString)+1
--								)
--                       , '')
--END
--return @MyString
--END
--GO


select * from (
select UClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS as ClassName, 'ClassFormDefinition' COLLATE SQL_Latin1_General_CP1_CI_AS as MisMatchType, UClass.ClassFormDefinition COLLATE SQL_Latin1_General_CP1_CI_AS as CurrentValue, FClass.ClassFormDefinition COLLATE SQL_Latin1_General_CP1_CI_AS as ProperValue,
'Update UPGRADED_XPERIENCEDB.dbo.CMS_Class set ClassFormDefinition = '''+REPLACE(FClass.ClassformDefinition, '''', '''''')+''' where ClassID = '+CAST(UClass.ClassID as nvarchar(20)) COLLATE SQL_Latin1_General_CP1_CI_AS as UpdateStatement
from UPGRADED_XPERIENCEDB.dbo.CMS_Class UClass
inner join FRESH_XPERIENCEDB.dbo.CMS_Class FClass on FClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS = UClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS
where REPLACE(REPLACE(REPLACE(REPLACE(dbo.StripMacros(UClass.ClassFormDefinition), '"/>', '" />'), CHAR(13), ''), CHAR(10), ''), ' ', '') COLLATE SQL_Latin1_General_CP1_CI_AS <> REPLACE(REPLACE(REPLACE(REPLACE(dbo.StripMacros(FClass.ClassFormDefinition), '"/>', '" />'), CHAR(13), ''), CHAR(10), ''), ' ', '') COLLATE SQL_Latin1_General_CP1_CI_AS
UNION ALL

select UClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS as ClassName, 'ClassXmlSchema' COLLATE SQL_Latin1_General_CP1_CI_AS as MisMatchType, UClass.ClassXmlSchema COLLATE SQL_Latin1_General_CP1_CI_AS as CurrentValue, FClass.ClassXmlSchema COLLATE SQL_Latin1_General_CP1_CI_AS as ProperValue,
'Update UPGRADED_XPERIENCEDB.dbo.CMS_Class set ClassXmlSchema = '''+REPLACE(FClass.ClassXmlSchema, '''', '''''')+''' where ClassID = '+CAST(UClass.ClassID as nvarchar(20)) COLLATE SQL_Latin1_General_CP1_CI_AS as UpdateStatement 
from UPGRADED_XPERIENCEDB.dbo.CMS_Class UClass
inner join FRESH_XPERIENCEDB.dbo.CMS_Class FClass on FClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS = UClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS
where REPLACE(REPLACE(REPLACE(REPLACE(dbo.StripMacros(UClass.ClassXmlSchema), '"/>', '" />'), CHAR(13), ''), CHAR(10), ''), ' ', '') COLLATE SQL_Latin1_General_CP1_CI_AS <> REPLACE(REPLACE(REPLACE(REPLACE(dbo.StripMacros(FClass.ClassXmlSchema), '"/>', '" />'), CHAR(13), ''), CHAR(10), ''), ' ', '') COLLATE SQL_Latin1_General_CP1_CI_AS

UNION ALL
select UClass.ClassName+'.'+UAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS as ClassName, 'AltFormMismatch' COLLATE SQL_Latin1_General_CP1_CI_AS as MisMatchType, UAF.FormDefinition COLLATE SQL_Latin1_General_CP1_CI_AS as CurrentValue, FAF.FormDefinition COLLATE SQL_Latin1_General_CP1_CI_AS as ProperValue,
'Update UPGRADED_XPERIENCEDB.dbo.CMS_AlternativeForm set FormDefinition = '''+REPLACE(FAF.FormDefinition, '''', '''''')+''' where FormID = '+CAST(UAF.FormID as nvarchar(20)) COLLATE SQL_Latin1_General_CP1_CI_AS as UpdateStatement 
from UPGRADED_XPERIENCEDB.dbo.CMS_AlternativeForm UAF
inner join UPGRADED_XPERIENCEDB.dbo.CMS_Class UClass on UClass.classID = UAF.FormClassID
inner join FRESH_XPERIENCEDB.dbo.CMS_Class FClass on FClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS = UClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS
Inner join FRESH_XPERIENCEDB.dbo.CMS_AlternativeForm FAF on FAF.FormClassID = FClass.ClassID and FAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS = UAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS
where REPLACE(REPLACE(REPLACE(REPLACE(dbo.StripMacros(UAF.FormDefinition), '"/>', '" />'), CHAR(13), ''), CHAR(10), ''), ' ', '') COLLATE SQL_Latin1_General_CP1_CI_AS <> REPLACE(REPLACE(REPLACE(REPLACE(dbo.StripMacros(FAF.FormDefinition), '"/>', '" />'), CHAR(13), ''), CHAR(10), ''), ' ', '') COLLATE SQL_Latin1_General_CP1_CI_AS

union all
select UClass.ClassName+'.'+UAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS as ClassName, 'AltFormNotOnFresh' COLLATE SQL_Latin1_General_CP1_CI_AS as MisMatchType, UAF.FormDefinition COLLATE SQL_Latin1_General_CP1_CI_AS as CurrentValue, FAF.FormDefinition COLLATE SQL_Latin1_General_CP1_CI_AS as ProperValue,
'delete from UPGRADED_XPERIENCEDB.dbo.CMS_AlternativeForm where FormID = '+CAST(UAF.FormID as nvarchar(20)) COLLATE SQL_Latin1_General_CP1_CI_AS as UpdateStatement 
from UPGRADED_XPERIENCEDB.dbo.CMS_AlternativeForm UAF
inner join UPGRADED_XPERIENCEDB.dbo.CMS_Class UClass on UClass.classID = UAF.FormClassID
inner join FRESH_XPERIENCEDB.dbo.CMS_Class FClass on FClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS = UClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS
left join FRESH_XPERIENCEDB.dbo.CMS_AlternativeForm FAF on FAF.FormClassID = FClass.ClassID and FAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS = UAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS
where FAF.FormID is null


union all
select FClass.ClassName+'.'+FAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS as ClassName, 'AltFormMissingOnUpgrade' COLLATE SQL_Latin1_General_CP1_CI_AS as MisMatchType, UAF.FormDefinition as CurrentValue, FAF.FormDefinition as ProperValue,
'TODO '''+FAF.FormDefinition+''' where FormID = '+CAST(UAF.FormID as nvarchar(20)) as UpdateStatement 
from FRESH_XPERIENCEDB.dbo.CMS_AlternativeForm FAF
inner join FRESH_XPERIENCEDB.dbo.CMS_Class FClass on FClass.classID = FAF.FormClassID 
inner join UPGRADED_XPERIENCEDB.dbo.CMS_Class UClass on FClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS = UClass.ClassName COLLATE SQL_Latin1_General_CP1_CI_AS
left join UPGRADED_XPERIENCEDB.dbo.CMS_AlternativeForm UAF on FAF.FormClassID = FClass.ClassID and FAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS = UAF.FormName COLLATE SQL_Latin1_General_CP1_CI_AS
where UAF.FormID is null

) combined order by MisMatchType desc, ClassName





-- RUN THIS AFTER DONE
--DROP FUNCTION [dbo].[StripMacros]
--GO

