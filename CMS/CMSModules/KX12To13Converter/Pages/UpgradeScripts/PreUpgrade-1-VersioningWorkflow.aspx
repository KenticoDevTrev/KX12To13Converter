<%@ Page Language="C#" AutoEventWireup="true" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts.VersioningWorkflow" Theme="Default" MasterPageFile="~/CMSMasterPages/LiveSite/SimplePage.master" %>

<asp:Content ID="cnt" ContentPlaceHolderID="plcContent" runat="server">

    <div>
        <asp:Literal ID="ltrResult" runat="server" />
    </div>

    <div>
        <h1>Pre Upgrade Operations #1: Versioning and Workflow</h1>

        <p>
           
        </p>
        <h2>A: Publish / Check in Pages</h2>
        <p>
            Version history should be eliminted, but before that occurs all documents must be in a checked in / published state.<br />
        </p>
        <h3>Workflow Configuration</h3>
        <div>
             <p>Site:
            <asp:DropDownList runat="server" ID="ddlSite" /><br /></p>
            <p>Path: <cms:FormControl runat="server" ID="txtPath" FormControlName="selectpath" />
            </p>
            <p>
                Page in Edit Mode, Previously Archived:
            <asp:DropDownList runat="server" ID="ddlPreviousArchivedEdit">
                <asp:ListItem Value="rollback" Text="Roll Back to Archived" />
                <asp:ListItem Value="archive" Text="Check in and Archive" Selected="True" />
                <asp:ListItem Value="publish" Text="Check in and Publish" />
            </asp:DropDownList>
            </p>
            <asp:Literal runat="server" ID="lstPreviousArchivedEdit"></asp:Literal>
            <hr />

            <p>
                Pages in Edit Mode, never Published:<asp:DropDownList runat="server" ID="ddlNeverPublished">
                    <asp:ListItem Value="archive" Text="Check in and Archive" />
                    <asp:ListItem Value="publish" Text="Check in and Publish" Selected="True" />
                </asp:DropDownList>
            </p>
            <asp:Literal runat="server" ID="lstNeverPublished"></asp:Literal>
            <hr />
            <p>
                Pages Unpublished with future published version waiting:<asp:DropDownList runat="server" ID="ddlUnpublishedWaitingPublishing">
                    <asp:ListItem Value="archive" Text="Set to future published state and Archive" />
                    <asp:ListItem Value="publish" Text="Promote future published state to current published state." Selected="True" />
                </asp:DropDownList>
            </p>
            <asp:Literal runat="server" ID="lstUnpublishedWaitingPublishing"></asp:Literal>
            <hr />
            <p>
                Pages Published with future published version waiting:
                <asp:DropDownList runat="server" ID="ddlPublishedWithFuturePublish">
                    <asp:ListItem Value="rollback" Text="Keep Current Live Publish and ignore future publish" />
                    <asp:ListItem Value="public" Text="Promote future published state to current published state" Selected="True" />
                </asp:DropDownList>
            </p>
            <asp:Literal runat="server" ID="lstPublishedWithFuturePublish"></asp:Literal>
            <hr />
            <p>
                Pages in Edit Mode, Previously Published:
                <asp:DropDownList runat="server" ID="ddlPreviouslyPublished">
                    <asp:ListItem Value="rollback" Text="Roll Back to previous Published state" />
                    <asp:ListItem Value="publish" Text="Check in and Publish" Selected="True" />
                </asp:DropDownList>
            </p>
            <asp:Literal runat="server" ID="lstPreviouslyPublished"></asp:Literal>
            <hr />
            <p>
                Pages with no Version History and are neither Published nor Archived:
                <asp:DropDownList runat="server" ID="ddlNoVersionHistory">
                    <asp:ListItem Value="archive" Text="Archive" Selected="True" />
                    <asp:ListItem Value="publish" Text="Publish" />
                </asp:DropDownList>
            </p>
            <asp:Literal runat="server" ID="lstNoVersionHistory" ></asp:Literal>
            <hr />
        </div>
        <asp:Button runat="server" ID="btnPublishAll" OnClick="btnPublishAll_Click" Text="Run Publish Operation" CssClass="btn btn-primary" /> <asp:Button runat="server" ID="btnPublishAllReport" OnClick="btnPublishAllReport_Click" Text="Run Report" CssClass="btn btn-primary" />
    </div>
    <hr />
    <div>
        <h2>B: Turn off Version History / Workflow and Clear</h2>
        <p>
            Now that all the pages are checked in / archived, etc.  The CMS_Document, CMS_Tree, and any Content Table should contain the correct and up to date data.  Disabling Version History and clearing the history should be done since there's no going back to Portal Engine once migrated.<br />
            <br />
            This operation will:
        </p>
        <ul>
            <li>Disable all workflows</li>
            <li>Remove all pages from Workflows</li>
            <li>Null out the version history fields on the documents</li>
            <li>Clear the CMS_VersionHistory table</li>
        </ul>
        <p><br />You may need to turn off Transaction Logs depending on the size of the database and space of the log drive. Afterwards, you may also want to (assuming you backed up prior) set the database recovery mode to "Simple", shrink log/database, then put back on Full and backup again.</p>
        <asp:Button runat="server" ID="btnDisableWorkflowClearHistory" OnClick="btnDisableWorkflowClearHistory_Click" Text="Disable Versioning, Workflow, and Clear History" CssClass="btn btn-primary" />

    </div>
</asp:Content>
