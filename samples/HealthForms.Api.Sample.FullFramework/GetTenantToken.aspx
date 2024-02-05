<%@ Page Title="Get Token" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GetTenantToken.aspx.cs" Inherits="HealthForms.Api.Sample.FullFramework.GetTenantToken"  Async="true"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <div class="text-center">
            <h1 class="display-4">Tenant Token</h1>
            <p>Learn more about authorizing a <a href="https://github.com/southportsolutions/healthforms.io">HealthForms.io tenant</a>.</p>

            <div><strong>Tenant ID:</strong><span id="TenantId" runat="server"></span></div>
            <div><strong>Tenant Token:</strong><span id="TenantToken" runat="server"></span></div>
        </div>
    </main>
</asp:Content>
