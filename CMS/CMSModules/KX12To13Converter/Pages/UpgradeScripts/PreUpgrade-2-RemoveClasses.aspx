<%@ Page Language="C#" AutoEventWireup="true" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts.RemoveClasses" Theme="Default" MasterPageFile="~/CMSMasterPages/LiveSite/SimplePage.master" %>

<asp:Content ID="cnt" ContentPlaceHolderID="plcContent" runat="server">

    <div>
        <asp:Literal ID="ltrResult" runat="server" />
    </div>

    <div>
        <h1>Pre Upgrade Operations #2: Cleanup Page Types</h1>
        <h2>Remove Unused Classes</h2>
        <p>
            Below is a list of Document Page Types that are not used on the content tree that can be removed.
        </p>
        <p>
            <strong>BE AWARE:</strong> Removing the page type also deletes the classes Queries and Code which may be referenced in custom code / the application.  Make sure to scan your custom code for any references, below should show probable places it or it's queries are referenced if found.
        </p>
        <p>
            You should have a backup of these classes and their content available in case you later discover they were referenced.
        </p>
        <asp:CheckBoxList runat="server" ID="cbxClasses" />

        <asp:Button runat="server" ID="btnDeleteClasses" OnClick="btnDeleteClasses_Click" Text="Delete Classes" />
        <hr />
        <h2>Remove Obsolete Used Classes</h2>
        <p>Some classes may have a pages but aren't used and can be deleted.</p>
        <p>
            <strong>BE AWARE:</strong> Removing the page type also deletes the classes Queries and Code which may be referenced in custom code / the application.  Make sure to scan your custom code for any references, below should show probable places it or it's queries are referenced if found.
        </p>
        <asp:CheckBoxList runat="server" ID="cbxUsedClasses" />

        <asp:Button runat="server" ID="btnDeletePagesAndClasses" OnClick="btnDeletePagesAndClasses_Click" Text="Delete Pages and Classes" />
    </div>
</asp:Content>
