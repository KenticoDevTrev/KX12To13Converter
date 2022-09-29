<%@ Page Language="C#" AutoEventWireup="true" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts.ConvertForms" Theme="Default" MasterPageFile="~/CMSMasterPages/LiveSite/SimplePage.master" %>

<asp:Content ID="cnt" ContentPlaceHolderID="plcContent" runat="server">
    <div>
        <asp:Literal ID="ltrResult" runat="server" />
    </div>

    <div>
        <h1>Pre Upgrade Operations #4: Convert Online Forms to MVC</h1>
        <p>
            The upgrade tool will not allow you to continue if you have Portal engine Online Forms.  This tool will do a basic conversion of the form.  You will need to provide any mapping of Portal Engine Controls to Page Builder Control Identies.
        </p>
        <p>
            Default Section Area Name: <asp:TextBox runat="server" ID="txtDefaultSectionIdentifier" Text="DefaultFormBuilderArea" /><br />
            Default Section Type: <asp:TextBox runat="server" ID="txtDefaultSectionType" Text="Kentico.DefaultSection" />
        </p>
        <p>
            See list of <a href="https://docs.xperience.io/developing-websites/form-builder-development/reference-system-form-components" target="_blank">default form components</a>, you can put your own ID and create one later.
        </p>
        <asp:TextBox runat="server" ID="txtConfig" Columns="50" Rows="10" TextMode="MultiLine" /><br />
        <asp:Button runat="server" ID="btnConvert" OnClick="btnConvert_Click" Text="Convert" CssClass="btn btn-primary" />
    </div>
</asp:Content>
