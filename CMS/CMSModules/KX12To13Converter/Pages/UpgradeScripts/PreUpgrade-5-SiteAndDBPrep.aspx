<%@ Page Language="C#" AutoEventWireup="true" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts.SiteAndDBPrep" Theme="Default" MasterPageFile="~/CMSMasterPages/LiveSite/SimplePage.master" %>

<asp:Content ID="cnt" ContentPlaceHolderID="plcContent" runat="server">
   
    <div>
        <asp:Literal ID="ltrResult" runat="server" />
    </div>

    <div>
        <h1>Pre Upgrade Operations #5: Site and Upgrade Procedures</h1>
        <p>
            The final step is to set the sites to "Content Only" (MVC) and run some pre-upgrade 'fix' scripts that prevent the upgrade tool from blowing up due to foreign key constraints on legacy tables.
        </p>
        <p>
            Database Name: <asp:TextBox runat="server" ID="txtDatabaseName" />
        </p>
        <asp:Button runat="server" ID="btnSetSitesToContentOnly" Text="Set Sites to Content Only" OnClick="btnSetSitesToContentOnly_Click" CssClass="btn btn-primary" /> 
        <asp:Button runat="server" ID="btnRunPreUpgradeFixes" Text="Run Pre-upgrade DB fixes" OnClick="btnRunPreUpgradeFixes_Click" CssClass="btn btn-primary" />
        <asp:Button runat="server" ID="btnSetCompatabilityLevel" Text="Set DB compatability level" OnClick="btnSetCompatabilityLevel_Click" CssClass="btn btn-primary" />
    </div>
</asp:Content>
