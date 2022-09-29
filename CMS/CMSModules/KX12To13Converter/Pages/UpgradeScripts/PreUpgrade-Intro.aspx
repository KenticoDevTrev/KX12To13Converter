<%@ Page Language="C#" AutoEventWireup="true" Inherits="KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts.Intro" Theme="Default" MasterPageFile="~/CMSMasterPages/LiveSite/SimplePage.master" %>

<asp:Content ID="cnt" ContentPlaceHolderID="plcContent" runat="server">
    <div>
        <h1>Pre Upgrade Operations</h1>
        <p>
           <strong>WARNING</strong>: These operations should only be run on a cloned instance of the site (database and code) before your upgrade.  <strong>These operations will render your current Portal Engine site as inoperable</strong>, and are solely to prepare them for upgrading to KX13.
        </p>
        <p>Run these operations in order, preferably backing up the database in between each operation.</p>
        <p>These tools in essence "convert" your Portal Engine site to a KX12 MVC Site, which <em>is upgradeable</em> to KX13.</p>
        <p>Once these operations are completed, you should be able to run the Kentico Xperience 13 upgrade tool to upgrade the site.</p>
        <p><strong>Remove Classes Delay</strong>: The Remove Classes UI may take a long time if you have a large amount of pages to load due to it checking for Inherited Classes and ordering by total usage.</p>
    </div>
</asp:Content>
